# Tasks: 003-ux-feedback-polish

## Phase 1 — Schema & Core

- [x] T001 Bump `DataSchema.Current` to 2; add `V1ToV2DurationMigrator` (hours→minutes)
- [x] T002 Replace `Entry.DurationHours` with `DurationMinutes`; update OverlapDetector
- [x] T003 Rewrite `DurationSplit.CreateTimedEntry` for minute-accurate split
- [x] T004 Update `EntryDraft` / `EntryWorkflow` Validate/Materialize/Save/Delete (single-id delete/edit)
- [x] T005 Update Core tests (DurationSplit, EntryWorkflow, SchemaVersion)

## Phase 2 — Editor

- [x] T006 EntryEditorViewModel: TimeOnly? + DurationHours/Minutes UI props → DurationMinutes draft
- [x] T007 EntryEditorView XAML: note+emoji row, TimePicker, duration one row; grow MaxHeight

## Phase 3 — Main calendar UI

- [x] T008 MonthViewModel: CanGoPrevious + GoPrevious; FormatSummary with star; today cell bg; hover entry emphasize
- [x] T009 MainPage: Prev/Today/Next left; gear settings; Fraunces title; stretch + ScrollViewer cells
- [x] T010 DayEntriesListView: reliable middle-click delete

## Phase 4 — Chrome & Quit

- [x] T011 Bundle Fraunces font; Assets/Resources placeholder icon; wire ResolveAppIconPath
- [x] T012 Fix AppHost.Quit hang (no GetResult on UI path)
- [x] T013 Build, test, launch verify

## Phase 5 — Grid middle-click + entry highlight (follow-up)

- [x] T014 DayEntrySummaryLine with day/entry hover brushes
- [x] T015 MainPage: left-only add; middle on line deletes; middle on empty cell no-op
- [x] T016 DayEntriesListView AddHandler handledEventsToo for middle-delete
- [x] T017 Spec FR-013/014 + User Story 7 updated
