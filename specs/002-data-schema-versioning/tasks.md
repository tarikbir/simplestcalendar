# Tasks: Data Schema Versioning

**Input**: Design docs in `/specs/002-data-schema-versioning/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

## Phase 1: Foundational

- [x] T001 Add `DataSchema.cs` and `DataFileBackup.cs` under `src/SimplestCalendar.Core/Services/`
- [x] T002 Add `SchemaMigration.cs` migration pipeline (Current=1, no-op steps) under same folder
- [x] T003 [P] Set `Version`/`InformationalVersion` on `src/SimplestCalendar/SimplestCalendar.csproj` (currently `0.3.0`); expose app version string for stores

## Phase 2: US1 — Safe load / save

- [x] T004 [US1] Version-aware `LoadAsync`/`SaveAsync` on `JsonEntryStore.cs` (migrate-up, missing≡1, write version+appVersion)
- [x] T005 [US1] Same for `JsonSettingsStore.cs`
- [x] T006 [P] [US1] Update MVP contract note in `specs/001-calendar-mvp/contracts/data-store.md` to point at 002 rules (or mirror migrate/backup bullets)

## Phase 3: US2 — Future / corrupt safety

- [x] T007 [US2] Ensure future-version and corrupt paths use backup + warning on both stores
- [x] T008 [US2] Add `tests/SimplestCalendar.Core.Tests/SchemaVersionTests.cs` (round-trip, missing version, future+backup, corrupt)

## Phase 4: US3 — UI

- [x] T009 [US3] `AppServices` aggregates `DataLoadWarning`; pass app version into stores
- [x] T010 [US3] About panel shows app + schema versions (`SettingsWindow.xaml` / ViewModel)
- [x] T011 [US3] One-shot startup ContentDialog when `DataLoadWarning` is set (`MainWindow` or `App` after activate)

## Phase 5: Verify

- [x] T012 Run `./scripts/verify.ps1`; fix blockers
