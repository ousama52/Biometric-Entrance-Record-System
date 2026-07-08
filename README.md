# Fingerprint Entry Record System

A desktop entrance/attendance logging system that records entries using a
fingerprint sensor. It consists of three cooperating components:

| Component | Technology | Location |
|-----------|-----------|----------|
| **Desktop application** | VB.NET WinForms (.NET Framework 4.7.2) | [`src/`](src/) |
| **Sensor firmware** | Arduino C++ (Adafruit Fingerprint sensor) | [`arduino/`](arduino/) |
| **Database** | Embedded **SQLite** file (auto-created), created on first run | `entrancerecord.db` next to the app |

The Arduino reads fingerprints and streams events over USB serial; the VB.NET
app receives them, matches users, and stores entry records in a local SQLite
database file. **No database server needs to be installed or running** — on
first launch the app creates `entrancerecord.db` next to the executable, so it
can be built and tested entirely on its own.

> **Sample data included.** The original MySQL/MariaDB dump
> ([`database/entrancerecord.sql`](database/entrancerecord.sql)) has been
> migrated into a bundled SQLite seed
> ([`database/entrancerecord.db`](database/entrancerecord.db) — 97 users and
> their photos plus 31 attendance records). The app copies this seed on first
> run, so it starts fully populated; delete `entrancerecord.db` next to the
> executable to start empty.

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

- **Windows** with **Visual Studio 2022 or later** (verified with VS 2026 / v18).
  The **.NET Framework 4.7.2 targeting pack** must be installed (VS workload:
  *.NET desktop development*).
- **No database server is required.** The app uses the embedded
  **System.Data.SQLite** provider, restored automatically from NuGet; the native
  SQLite engine is copied next to the executable at build time.
- For the firmware: the **Arduino IDE** (or `arduino-cli`) and the
  **Adafruit Fingerprint Sensor Library** (install via Library Manager).

---

## Setup & build

### 1. Desktop application

Open [`src/Biometric Entrance Record System.sln`](src/) in Visual Studio and
press **F5**, or build from a Developer command prompt:

```powershell
# Restore NuGet packages (System.Data.SQLite.Core) then build
msbuild "src\Biometric Entrance Record System\Biometric Entrance Record System.vbproj" -t:Restore,Build -p:Configuration=Debug
```

That's the whole setup — there is no database to create or import. The build
copies the bundled seed database beside the `.exe` as `entrancerecord.seed.db`;
on first run the app copies it to the live `entrancerecord.db` (or creates an
empty one, with the `entrancerecord` and `recordtable` tables, if the seed is
missing). The database path, seed logic and connection string are owned by
[`src/Biometric Entrance Record System/Db.vb`](src/).

> **Testing without the fingerprint device:** the app runs fine with no Arduino
> connected — you'll get a non-blocking warning, and every screen stays usable.
> The **Record** tab has a manual *Time In/Out* box (look up a user by NRC) that
> works entirely without hardware.

### 2. Arduino firmware

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

- **Installer project:** the old Visual Studio Installer project
  (`Entry Record System.vdproj`) was **removed from the solution** because it
  blocks the solution from loading in Visual Studio 2026. The `.vdproj` file is
  still on disk (unreferenced) if you ever want to rebuild an installer with the
  *Microsoft Visual Studio Installer Projects* extension; note it points at two
  icon files outside the repository (`911262 (1).ico`, `911262 (2).ico`).

### Recently fixed

- **Migrated the database from MySQL/MariaDB to embedded SQLite.** The app now
  uses `System.Data.SQLite` and creates `entrancerecord.db` automatically, so it
  runs and can be tested standalone with no server. MySQL-only SQL (`CURDATE`,
  `WEEK`, `MONTH`, `NOW`) in the report queries was rewritten for SQLite, and
  dates are now stored in ISO `yyyy-MM-dd` format.
- **Migrated the sample data.** All 97 users (with photos) and 31 attendance
  records from the original dump were converted into `database/entrancerecord.db`
  and are seeded on first run, so the app opens fully populated.
- **Loading splash redesigned** around the original campus photo — it fills the
  window with a gradient scrim and an overlaid title/subtitle plus an
  indeterminate progress bar. The main window keeps its original appearance
  (Times New Roman fonts, the original data grids, and the rounded lavender
  frame).
- **Removed the "device required" blockage.** The app no longer refuses to open
  screens when the fingerprint reader is absent — it shows a non-blocking
  warning and stays fully usable.
- **Visual Studio 2026 compatibility:** dropped the `.vdproj` installer project
  from the solution and slimmed the project down to a single NuGet dependency
  (`System.Data.SQLite.Core`), removing the old MySQL package graph and its
  binding redirects.
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
