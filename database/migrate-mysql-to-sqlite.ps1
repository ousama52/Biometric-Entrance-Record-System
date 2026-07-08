<#
.SYNOPSIS
    Converts the original phpMyAdmin/mysqldump dump (entrancerecord.sql) into the
    embedded SQLite database used by the app (entrancerecord.db).

.DESCRIPTION
    Parses the `entrancerecord` rows (image stored as a 0x… hex blob) and the
    `recordtable` rows, unescapes MySQL string literals, normalises dates to ISO
    (yyyy-MM-dd), and writes them into a fresh SQLite file with the same schema
    Db.vb creates. Re-run this whenever entrancerecord.sql changes.

    The app's build copies the resulting entrancerecord.db next to the .exe as
    entrancerecord.seed.db, and Db.EnsureCreated() seeds from it on first run.

.NOTES
    Requires System.Data.SQLite.dll, which is restored/built with the project.
    Build the solution once (Debug) before running, or pass -Dll explicitly.
#>
param(
  [string]$Dump = (Join-Path $PSScriptRoot "entrancerecord.sql"),
  [string]$Out  = (Join-Path $PSScriptRoot "entrancerecord.db"),
  [string]$Dll  = (Join-Path $PSScriptRoot "..\src\Biometric Entrance Record System\bin\Debug\System.Data.SQLite.dll")
)

Add-Type -Path $Dll
$ErrorActionPreference = "Stop"

if ([System.IO.File]::Exists($Out)) { [System.IO.File]::Delete($Out) }
$text = [System.IO.File]::ReadAllText($Dump)
$STR = "(?:\\.|[^'\\])*"   # MySQL single-quoted string body

function Unescape([string]$s) {
  if ($null -eq $s) { return $null }
  $sb = New-Object System.Text.StringBuilder
  for ($i=0; $i -lt $s.Length; $i++) {
    $ch = $s[$i]
    if ($ch -eq '\' -and $i+1 -lt $s.Length) {
      $n = $s[$i+1]; $i++
      switch ($n) {
        "'" { [void]$sb.Append("'") }; '"' { [void]$sb.Append('"') }
        '\' { [void]$sb.Append('\') }; 'n' { [void]$sb.Append("`n") }
        'r' { [void]$sb.Append("`r") }; 't' { [void]$sb.Append("`t") }
        '0' { }; default { [void]$sb.Append($n) }
      }
    } else { [void]$sb.Append($ch) }
  }
  return $sb.ToString()
}
function IsoDate([string]$d) {
  if ($d -match '^\s*(\d{4})/(\d{1,2})/(\d{1,2})\s*$') {
    return ("{0}-{1:D2}-{2:D2}" -f [int]$matches[1],[int]$matches[2],[int]$matches[3])
  }
  return $d
}

$cn = New-Object System.Data.SQLite.SQLiteConnection("Data Source=$Out;Version=3;")
$cn.Open()
$tx = $cn.BeginTransaction()
$mk = $cn.CreateCommand()
$mk.CommandText = @"
CREATE TABLE IF NOT EXISTS entrancerecord (Name TEXT NOT NULL, ID TEXT NOT NULL PRIMARY KEY, NRC TEXT NOT NULL, Email TEXT, Roll TEXT NOT NULL, Images BLOB NOT NULL, Rank TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS recordtable (RecID INTEGER PRIMARY KEY AUTOINCREMENT, ID TEXT NOT NULL, Date TEXT NOT NULL, TimeIn TEXT NOT NULL, AM TEXT NOT NULL, TimeOut TEXT, PM TEXT);
"@
[void]$mk.ExecuteNonQuery()

$parts  = $text -split 'INSERT INTO `recordtable`',2
$erText = $parts[0]
$rtText = if ($parts.Count -gt 1) { $parts[1] } else { "" }

# ---- entrancerecord (7 fields, 6th is a 0x hex blob) ----
$erRx = [regex]::new("\(\s*'($STR)'\s*,\s*'($STR)'\s*,\s*'($STR)'\s*,\s*(?:'($STR)'|NULL)\s*,\s*'($STR)'\s*,\s*0x([0-9a-fA-F]+)\s*,\s*'($STR)'\s*\)",[System.Text.RegularExpressions.RegexOptions]::Singleline)
$ins = $cn.CreateCommand()
$ins.CommandText = "INSERT OR REPLACE INTO entrancerecord (Name,ID,NRC,Email,Roll,Images,Rank) VALUES (@n,@id,@nrc,@em,@roll,@img,@rank)"
$pN=$ins.Parameters.Add("@n",[System.Data.DbType]::String); $prmId=$ins.Parameters.Add("@id",[System.Data.DbType]::String)
$pNRC=$ins.Parameters.Add("@nrc",[System.Data.DbType]::String); $pEM=$ins.Parameters.Add("@em",[System.Data.DbType]::String)
$pRoll=$ins.Parameters.Add("@roll",[System.Data.DbType]::String); $pImg=$ins.Parameters.Add("@img",[System.Data.DbType]::Binary)
$pRank=$ins.Parameters.Add("@rank",[System.Data.DbType]::String)
$erCount=0
foreach ($m in $erRx.Matches($erText)) {
  $pN.Value=Unescape $m.Groups[1].Value; $prmId.Value=Unescape $m.Groups[2].Value
  $pNRC.Value=Unescape $m.Groups[3].Value
  $pEM.Value= if ($m.Groups[4].Success) { Unescape $m.Groups[4].Value } else { [DBNull]::Value }
  $pRoll.Value=Unescape $m.Groups[5].Value
  $pImg.Value=[Convert]::FromHexString($m.Groups[6].Value); $pRank.Value=Unescape $m.Groups[7].Value
  [void]$ins.ExecuteNonQuery(); $erCount++
}

# ---- recordtable (int PK + 6 fields, last two nullable) ----
$rtRx = [regex]::new("\(\s*(\d+)\s*,\s*'($STR)'\s*,\s*'($STR)'\s*,\s*'($STR)'\s*,\s*'($STR)'\s*,\s*(NULL|'$STR')\s*,\s*(NULL|'$STR')\s*\)",[System.Text.RegularExpressions.RegexOptions]::Singleline)
$ins2 = $cn.CreateCommand()
$ins2.CommandText = "INSERT OR REPLACE INTO recordtable (RecID,ID,Date,TimeIn,AM,TimeOut,PM) VALUES (@rid,@id,@dt,@ti,@am,@to,@pm)"
$qRid=$ins2.Parameters.Add("@rid",[System.Data.DbType]::Int64); $qID=$ins2.Parameters.Add("@id",[System.Data.DbType]::String)
$qDt=$ins2.Parameters.Add("@dt",[System.Data.DbType]::String); $qTi=$ins2.Parameters.Add("@ti",[System.Data.DbType]::String)
$qAm=$ins2.Parameters.Add("@am",[System.Data.DbType]::String); $qTo=$ins2.Parameters.Add("@to",[System.Data.DbType]::String)
$qPm=$ins2.Parameters.Add("@pm",[System.Data.DbType]::String)
function StripQuotes([string]$v) { if ($v -eq 'NULL') { return $null }; return Unescape ($v.Substring(1, $v.Length-2)) }
$rtCount=0
foreach ($m in $rtRx.Matches($rtText)) {
  $qRid.Value=[int64]$m.Groups[1].Value; $qID.Value=Unescape $m.Groups[2].Value
  $qDt.Value=IsoDate (Unescape $m.Groups[3].Value); $qTi.Value=Unescape $m.Groups[4].Value; $qAm.Value=Unescape $m.Groups[5].Value
  $to=StripQuotes $m.Groups[6].Value; $qTo.Value= if ($null -eq $to) { [DBNull]::Value } else { $to }
  $pm=StripQuotes $m.Groups[7].Value; $qPm.Value= if ($null -eq $pm) { [DBNull]::Value } else { $pm }
  [void]$ins2.ExecuteNonQuery(); $rtCount++
}

$tx.Commit(); $cn.Close()
Write-Host "entrancerecord rows : $erCount"
Write-Host "recordtable rows    : $rtCount"
Write-Host "wrote $Out"
