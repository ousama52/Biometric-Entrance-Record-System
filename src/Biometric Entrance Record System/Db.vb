Imports System.Data.SQLite
Imports System.IO

''' <summary>
''' Owns the embedded SQLite database used by the application.
''' The database is a single file that lives next to the executable, so the
''' app can be run and tested on its own with no MySQL/SQL server to install
''' or start. The schema is created automatically on first run.
''' </summary>
Module Db

    ''' <summary>Full path to the SQLite database file (next to the .exe).</summary>
    Public ReadOnly Property DbPath As String
        Get
            Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "entrancerecord.db")
        End Get
    End Property

    ''' <summary>ADO.NET connection string for the SQLite database file.</summary>
    Public ReadOnly Property ConnectionString As String
        Get
            Return "Data Source=" & DbPath & ";Version=3;"
        End Get
    End Property

    ''' <summary>Seed database shipped with the app (migrated from the original
    ''' MySQL dump). Copied to <see cref="DbPath"/> on first run if present.</summary>
    Public ReadOnly Property SeedPath As String
        Get
            Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "entrancerecord.seed.db")
        End Get
    End Property

    ''' <summary>
    ''' Creates the database file and tables if they do not already exist.
    ''' Safe to call repeatedly (idempotent). Mirrors the original MySQL
    ''' schema (entrancerecord + recordtable) using SQLite types.
    ''' </summary>
    Public Sub EnsureCreated()
        If Not File.Exists(DbPath) Then
            ' First run: start from the bundled seed data if it is available,
            ' otherwise create an empty database. Either way the schema is
            ' guaranteed below.
            If File.Exists(SeedPath) Then
                File.Copy(SeedPath, DbPath)
            Else
                SQLiteConnection.CreateFile(DbPath)
            End If
        End If

        Using cn As New SQLiteConnection(ConnectionString)
            cn.Open()
            Using cmd As New SQLiteCommand(cn)
                cmd.CommandText =
                    "CREATE TABLE IF NOT EXISTS entrancerecord (" &
                    "Name TEXT NOT NULL, " &
                    "ID TEXT NOT NULL PRIMARY KEY, " &
                    "NRC TEXT NOT NULL, " &
                    "Email TEXT, " &
                    "Roll TEXT NOT NULL, " &
                    "Images BLOB NOT NULL, " &
                    "Rank TEXT NOT NULL);"
                cmd.ExecuteNonQuery()

                cmd.CommandText =
                    "CREATE TABLE IF NOT EXISTS recordtable (" &
                    "RecID INTEGER PRIMARY KEY AUTOINCREMENT, " &
                    "ID TEXT NOT NULL, " &
                    "Date TEXT NOT NULL, " &
                    "TimeIn TEXT NOT NULL, " &
                    "AM TEXT NOT NULL, " &
                    "TimeOut TEXT, " &
                    "PM TEXT);"
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

End Module
