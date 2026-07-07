# Fingerprint Entry Record System

A desktop entrance/attendance logging system that records entries using a
fingerprint sensor. It consists of three cooperating components:

| Component | Technology | Location |
|-----------|-----------|----------|
| **Desktop application** | VB.NET WinForms (.NET Framework 4.7.2) | [`src/`](src/) |
| **Sensor firmware** | Arduino C++ (Adafruit Fingerprint sensor) | [`arduino/`](arduino/) |
| **Database** | MySQL / MariaDB schema + data dump | [`database/`](database/) |

The Arduino reads fingerprints and streams events over USB serial; the VB.NET
app receives them, matches users, and stores entry records in a MySQL database.

---

## Repository structure

```
.
├── src/                                   # VB.NET WinForms desktop application
│   ├── Biometric Entrance Record System.sln
│   ├── Biometric Entrance Record System/ # Main WinForms project (net472)
│   │   ├── Form1.vb / .Designer.vb        # Main application window
│   │   ├── LoadingForm.vb / .Designer.vb  # Splash / loading screen
│   │   ├── My Project/                    # Assembly info, resources, settings
│   │   ├── Resources/                     # Icons & images used by the UI
│   │   ├── App.config                     # DB connection string + binding redirects
│   │   └── packages.config                # NuGet dependency manifest
│   ├── Entry Record System/               # Visual Studio Installer project (*.vdproj)
│   └── packages/                          # Restored NuGet packages (not tracked in git)
│
├── arduino/                               # Arduino firmware sketches
│   ├── Entrance_Record_System/            # Runtime firmware: scans & reports entries
│   └── Fingerprint_Enroll/                # Utility firmware: enrolls new fingerprints
│
├── database/
│   └── entrancerecord.sql                 # phpMyAdmin dump of the `entrancerecord` DB
│
├── .gitignore
└── README.md
```

> **Note on naming:** the VB.NET solution, project, assembly, root namespace and
> installer references were all corrected from the original misspelling
> *"Birometric"* to *"Biometric"* and verified with a clean build.

---

## Prerequisites

- **Windows** with **Visual Studio 2022 or later** (the solution was last used
  with VS 2026 / v18). The **.NET Framework 4.7.2 targeting pack** must be
  installed (VS workload: *.NET desktop development*).
- **MySQL Server** or **MariaDB** (the dump was produced with MariaDB 10.4).
- For the firmware: the **Arduino IDE** (or `arduino-cli`) and the
  **Adafruit Fingerprint Sensor Library** (install via Library Manager).
- To build the `.vdproj` installer: the
  **Microsoft Visual Studio Installer Projects** extension (installer projects
  cannot be built from the command line — only inside Visual Studio).

---

## Setup & build

### 1. Database

Create the database and import the dump (the schema and sample data live in the
same file):

```bash
mysql -u root -p -e "CREATE DATABASE IF NOT EXISTS entrancerecord;"
mysql -u root -p entrancerecord < database/entrancerecord.sql
```

Or import `database/entrancerecord.sql` through phpMyAdmin / MySQL Workbench.

### 2. Desktop application

Open [`src/Biometric Entrance Record System.sln`](src/) in Visual Studio and
press **F5**, or build from a Developer command prompt:

```powershell
# Restore NuGet packages (populates src/packages) then build
msbuild "src\Biometric Entrance Record System\Biometric Entrance Record System.vbproj" -t:Restore,Build -p:Configuration=Debug
```

The database connection is configured in two places (both currently point at a
local server with user `root` and an empty password):

- [`src/Biometric Entrance Record System/App.config`](src/) — `connectionString` app setting
- `Form1.vb` — an inline `MySqlConnection` string

Update both to match your MySQL/MariaDB credentials.

### 3. Arduino firmware

1. Open the desired sketch folder in the Arduino IDE:
   - `arduino/Fingerprint_Enroll/` — run first to enrol fingerprints.
   - `arduino/Entrance_Record_System/` — the runtime firmware.
2. Install the **Adafruit Fingerprint Sensor Library**.
3. Wiring used by the sketches:
   - Fingerprint sensor on hardware `Serial1` (Mega/Leonardo) or
     `SoftwareSerial(2, 3)` on an Uno; sensor baud rate **57600**.
   - Status LEDs on pins **8** and **9**, buzzer on pin **11**.
   - USB serial to the PC runs at **9600** baud.
4. Select your board/port and upload.

---

## Known issues

- **Installer project external references:** `Entry Record System.vdproj` points
  at two icon files outside the repository
  (`...\Downloads\911262 (1).ico`, `911262 (2).ico`); provide these locally if
  you rebuild the installer. The `.vdproj` also cannot be built from the command
  line — it requires the *Visual Studio Installer Projects* extension.
- **Hardcoded credentials:** database connection strings are committed in source
  and `App.config`. Move them to per-environment configuration for production.

### Recently fixed

- Restored three controls (`PanelNo`, `Label44`, `PictureBox12`) that were
  referenced in `Form1.vb` but missing from `Form1.Designer.vb`; the project now
  compiles cleanly.
- Corrected the *"Birometric"* → *"Biometric"* misspelling across the solution,
  project, assembly name and root namespace.

---

## Notes for contributors

- `src/packages/` and all `bin/`, `obj/`, `.vs/`, `Debug/`, `Release/` folders
  are generated and are **not** tracked in git (see [`.gitignore`](.gitignore)).
  Run `msbuild -t:Restore` (or let Visual Studio restore) after cloning.
- Keep each Arduino sketch inside a folder whose name matches the `.ino` file —
  this is required by the Arduino toolchain.
