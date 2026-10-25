# Implementation Plan: Data Schema Versioning

**Branch**: `002-data-schema-versioning` | **Date**: 2026-10-25 | **Spec**: [spec.md](./spec.md)

## Summary

Add schema version gates and a migration pipeline for portable JSON (`entries.json`, `settings.json`). Current schema = **1**. Future versions refuse load with non-destructive backup. Surface warnings in UI; show app + schema versions in About.

## Technical Context

| Item | Value |
|------|--------|
| Language | C# / .NET 10 |
| UI | WinUI 3 unpackaged (existing) |
| Storage | JSON beside exe (`data/`) — System.Text.Json |
| Testing | xUnit in `SimplestCalendar.Core.Tests` |
| Target | Windows 11 x64 |

## Constitution Check

- Offline / no accounts: OK  
- Portable JSON: OK  
- Core logic testable without UI: OK  
- English UI strings: OK  

## Project Structure

```text
src/SimplestCalendar.Core/Services/
  DataSchema.cs          # Current = 1, AppVersion helper hook
  DataFileBackup.cs      # timestamped .bak-vN-...
  SchemaMigration.cs     # ordered migrators (empty for v1)
  JsonEntryStore.cs      # version-aware load/save
  JsonSettingsStore.cs   # version-aware load/save
src/SimplestCalendar/
  SimplestCalendar.csproj  # Version 0.3.0
  Services/AppServices.cs  # DataLoadWarning
  MainWindow / Settings    # dialog + About versions
tests/.../SchemaVersionTests.cs
```

## Complexity Tracking

None — extends existing stores; no new persistence technology.

## Phase 0 / 1

See [research.md](./research.md), [data-model.md](./data-model.md), [contracts/data-store.md](./contracts/data-store.md), [quickstart.md](./quickstart.md).
