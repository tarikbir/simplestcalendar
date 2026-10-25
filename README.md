# Simplest Calendar

Offline Windows 11 month whiteboard. A personal calendar that stays local: month grid, day notes, reminders, and a system tray — no accounts, no sync, no cloud.

Built with **WinUI 3** and **.NET 10** (unpackaged / portable).

## Features

- Month grid with hover, today highlight, and starred entries
- Day entries: note, optional start time, duration (hours + minutes), star, reminder
- Midnight-spanning durations split into day segments (deletable independently)
- System tray (close-to-tray or quit), Windows toast reminders
- Settings: first day of week, month title format, startup, close behavior
- Portable JSON store next to the exe (`data/`), with schema versioning for safe upgrades

## Requirements

- Windows 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (to build)
- Windows App SDK / WinUI workload (Visual Studio “Windows application development”)
- [Inno Setup](https://jrsoftware.org/isinfo.php) only if you build the installer (`scripts/build-installer.ps1` can install it via winget)

## Install / run (from source)

```powershell
# Verify: tests, coverage gate, build, publish, short launch smoke
./scripts/verify.ps1

# Or publish a self-contained folder:
./scripts/publish.ps1
# → artifacts/publish/SimplestCalendar.exe

# Optional Setup.exe (needs Inno Setup):
./scripts/build-installer.ps1
# → artifacts/installer/SimplestCalendar-Setup-<version>.exe
```

Debug build output (path may vary slightly by SDK):

`src/SimplestCalendar/bin/x64/Debug/net10.0-windows*/win-x64/SimplestCalendar.exe`

On first run the app creates `data/` beside the executable. JSON files include a schema `version` and `appVersion`. Older schemas migrate up; newer schemas are refused with a `.bak-…` copy so upgrades/downgrades do not destroy data. App and schema versions are shown under **Settings → About**.

## Privacy

Everything stays on your machine. No telemetry, accounts, or network features are part of the product. Calendar notes live in local JSON under the portable `data/` folder.

## Development notes

Product decisions and Spec Kit plans live under `specs/` (start with [`specs/001-calendar-mvp/decisions.md`](specs/001-calendar-mvp/decisions.md)). Cursor Spec Kit / WinUI agent skills live under `.cursor/skills/` and `.specify/` for contributors who use that workflow; they are not required to build the app.

## Credits

- Month title font: [Fraunces](https://github.com/undercasetype/Fraunces) (SIL Open Font License 1.1) — see [`src/SimplestCalendar/Assets/Fonts/OFL.txt`](src/SimplestCalendar/Assets/Fonts/OFL.txt)

## License

This project is licensed under the [MIT License](LICENSE). The bundled Fraunces font remains under the SIL Open Font License 1.1 independently of the app license.
