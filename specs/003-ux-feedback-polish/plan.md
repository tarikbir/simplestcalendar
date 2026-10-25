# Implementation Plan: UX Feedback Polish

**Branch**: `003-ux-feedback-polish` | **Date**: 2026-10-25 | **Spec**: [spec.md](./spec.md)

## Technical Context

**Language/Version**: C# / .NET (WinUI 3 unpackaged)  
**Primary Dependencies**: Windows App SDK, CommunityToolkit.Mvvm  
**Storage**: Portable JSON beside exe; schema v2  
**Testing**: xUnit Core tests ≥50%  
**Target Platform**: Windows 11  

## Constitution Check

- Simplicity: polish + correctness only — PASS  
- Portable data + schema migrator — PASS  
- Prefer OS TimePicker / emoji / tray — PASS  
- Domain logic in Core + tests — PASS  
- Spec-driven + winui-app skill — PASS (Fraunces title is intentional display exception)

## Project Structure

### Source

- `src/SimplestCalendar.Core` — Entry, DurationSplit, EntryWorkflow, SchemaMigration, OverlapDetector
- `src/SimplestCalendar` — MainPage, EntryEditor, DayEntriesList, AppHost, Assets/Fonts, Assets/Resources
- `tests/SimplestCalendar.Core.Tests` — duration, workflow delete, schema migration, nav

### Structure Decision

Reuse existing projects; no new assemblies.

## Phase 0 / 1

See [research.md](./research.md), [data-model.md](./data-model.md), [contracts/data-store.md](./contracts/data-store.md), [quickstart.md](./quickstart.md).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|--------------------------------------|
| Custom Fraunces on title | User-requested month display font | Segoe-only fails the feedback |
