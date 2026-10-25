# Simplest Calendar

Offline Windows 11 month whiteboard (WinUI 3 / .NET 10). Spec Kit plans live under `specs/001-calendar-mvp/` — start with [decisions.md](specs/001-calendar-mvp/decisions.md).

## Build & verify

```powershell
./scripts/verify.ps1
```

Debug exe: `src/SimplestCalendar/bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64/SimplestCalendar.exe`  
Published: `artifacts/publish/SimplestCalendar.exe` (`./scripts/publish.ps1`)

Portable `data/` is created beside the exe on first run. JSON files carry a schema `version` (currently **1**) plus `appVersion`; older schemas migrate up, newer schemas are refused with a `.bak-…` copy so upgrades/downgrades do not destroy data. See Settings → About for app and schema versions.

## Features

Month grid, day entries (time/duration/star/reminder), system tray, Windows toast reminders, settings, JSON portable store.
