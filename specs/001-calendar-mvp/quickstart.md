# Quickstart: Simplest Calendar MVP

## Prerequisites

- Windows 11
- .NET 10 SDK
- WinUI / Windows App SDK workload (see `.cursor/skills/winui-app` setup references; do **not** run machine-wide `winget configure` unless the user approves)

## Dev loop

```powershell
# One-shot gates (test, coverage, build, publish, short launch):
./scripts/verify.ps1

# Or step-by-step:
dotnet build src/SimplestCalendar/SimplestCalendar.csproj -c Debug -p:Platform=x64
dotnet test tests/SimplestCalendar.Core.Tests/SimplestCalendar.Core.Tests.csproj -p:CollectCoverage=true -p:CoverletOutputFormat=cobertura -p:Threshold=50 -p:ThresholdType=line
./scripts/publish.ps1
# Launch Debug:
# src/SimplestCalendar/bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64/SimplestCalendar.exe
# Launch published:
# artifacts/publish/SimplestCalendar.exe
```

## Manual smoke checklist

Verified 2026-10-25 via automated Core smoke (`SmokeWorkflowTests`) + `scripts/verify.ps1` (test/coverage/build/publish/launch). Publish keeps ReadyToRun off and wipes `artifacts/publish` first (avoids net10 CsWinRT crash). UI checklist for human confirmation:

1. App opens on current month; today marked; English title.
2. Hover day → highlight.
3. Left-click day → add note → appears on grid.
4. Timed + duration across midnight → two segments.
5. Overlap → warning, still saves.
6. Right-click day with items → modal list; edit; middle-click delete.
7. Reminder on → toast (or simulated scheduler); click → compact view.
8. Close window → tray remains (same process); **Show** restores the same main window; Quit exits.
9. Settings from tray → same process Settings window; first day of week changes grid.
10. Restart → window size restored; data beside exe under `data/` (JSON; edited in memory while running).

Product decisions lock-in: [decisions.md](./decisions.md).
