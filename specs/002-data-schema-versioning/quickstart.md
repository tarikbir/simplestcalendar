# Quickstart: Schema Versioning

## Automated

```powershell
dotnet test tests/SimplestCalendar.Core.Tests/SimplestCalendar.Core.Tests.csproj --filter SchemaVersion
./scripts/verify.ps1
```

## Manual

1. Run published or Debug exe once; confirm `data/entries.json` has `"version": 1` and `"appVersion"`.
2. Settings → About: app version and schema version visible.
3. Edit `entries.json` to `"version": 999`, restart: dialog warns; original still 999; `.bak-v999-*` exists; calendar empty until you restore/fix version.
