# Implementation Plan: Simplest Calendar MVP

**Branch**: `001-calendar-mvp` | **Date**: 2026-10-25 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-calendar-mvp/spec.md`

## Summary

Build an unpackaged WinUI 3 (.NET 10) desktop app that behaves like a monthly whiteboard: current-month grid, day entries (note, optional time/duration/star/reminder), portable JSON beside the exe (**in-memory working set**, serialize on save), system tray lifetime in the **same single process** (Show restores the existing main window), Windows toast reminders, and a tabbed Settings window of that same process. UI follows the project `winui-app` skill (Fluent, dark theme). Domain logic lives in a testable class library with ≥50% line coverage.

**Authoritative product decisions**: [decisions.md](./decisions.md) — read before changing behavior.

## Technical Context

**Language/Version**: C# / .NET 10

**Primary Dependencies**: WinUI 3, Windows App SDK (unpackaged), CommunityToolkit.Mvvm (optional but preferred for VM boilerplate), Microsoft.Windows.SDK.BuildTools as required by template

**Storage**: Portable JSON files next to the executable (`data/entries.json`, `data/settings.json` or equivalent)

**Testing**: xUnit (or MSTest if template-default), coverlet for line coverage, `dotnet test`

**Target Platform**: Windows 11 x64 (unpackaged self-contained publish as stretch goal; framework-dependent OK for early dev)

**Project Type**: Desktop app (WinUI 3) + domain class library + unit test project

**Performance Goals**: Idle tray footprint modest (no polling hotter than ~1s for reminder scheduler; prefer timer aligned to next due reminder); month grid render snappy for ≤ a few hundred entries in horizon

**Constraints**: Offline; single process; no past-month navigation; +11 month forward cap; dark theme only v1; English UI; portable data; OS toasts + DND respect

**Scale/Scope**: Single user; one whiteboard; main window + settings window + compact toast view + tray

**UI/UX skill**: `.cursor/skills/winui-app` (Fluent controls, Mica/dark resources, native shells)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Gate | Status |
|------|--------|
| I. Personal whiteboard simplicity | PASS — scope is month grid + entries + settings + tray |
| II. Local-only portable data | PASS — JSON beside exe |
| III. Prefer OS capabilities | PASS — tray, toasts, emoji panel, WinUI chrome |
| IV. Test domain; thin UI | PASS — `SimplestCalendar.Core` + unit tests ≥50% |
| V. Spec-driven + winui-app skill | PASS — this plan; skill installed |

Post-design: unchanged PASS.

## Project Structure

### Documentation (this feature)

```text
specs/001-calendar-mvp/
├── decisions.md         # Ratified product “why” log (conversation lock-in)
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── data-store.md
├── checklists/
│   └── requirements.md
├── spec.md
└── tasks.md
```

### Source Code (repository root)

```text
SimplestCalendar.slnx                 # or .sln
src/
  SimplestCalendar/                   # WinUI 3 unpackaged app
    App.xaml
    MainWindow.xaml
    Views/
    ViewModels/
    Services/                         # WinUI-facing adapters (tray, toast, emoji)
  SimplestCalendar.Core/              # Domain + persistence abstractions
    Models/
    Services/
    Sorting/
    Calendar/
tests/
  SimplestCalendar.Core.Tests/
```

**Structure Decision**: Two production projects (App + Core) and one test project. Keeps WinUI types out of unit tests while allowing portable JSON persistence to be tested in Core.

## Complexity Tracking

> No constitution violations requiring justification.
