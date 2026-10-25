# Tasks: Simplest Calendar MVP

**Input**: Design documents from `/specs/001-calendar-mvp/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Required for domain (≥50% line coverage on `SimplestCalendar.Core`).

**UI skill**: Apply `.cursor/skills/winui-app` for all XAML/Fluent work (dark theme).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: US1…US6 map to spec user stories

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Solution scaffold

- [x] T001 Create `src/` and `tests/` directories per [plan.md](./plan.md)
- [x] T002 Scaffold unpackaged WinUI 3 app `src/SimplestCalendar` on net10.0 (`dotnet new winui` with unpackaged + MVVM options as available); set ProductName/AssemblyName to Simplest Calendar / SimplestCalendar
- [x] T003 [P] Add class library `src/SimplestCalendar.Core` (net10.0)
- [x] T004 [P] Add xUnit + coverlet project `tests/SimplestCalendar.Core.Tests` referencing Core
- [x] T005 Create solution file linking App, Core, Tests; App references Core
- [x] T006 [P] Add `.gitignore` for .NET/WinUI build artifacts; keep portable `data/` out of git via ignore of runtime data beside bin (document in README later)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Models, persistence, settings, app host shell — blocks stories

- [x] T007 [P] Implement `Entry` and `AppSettings` models in `src/SimplestCalendar.Core/Models/`
- [x] T008 [P] Implement entry sort comparer in `src/SimplestCalendar.Core/Sorting/EntrySort.cs`
- [x] T009 [P] Implement duration/midnight split helper in `src/SimplestCalendar.Core/Calendar/DurationSplit.cs`
- [x] T010 [P] Implement month navigation bounds (+0…+11, no past) in `src/SimplestCalendar.Core/Calendar/MonthNavigation.cs`
- [x] T011 [P] Implement month grid builder (BCL dates, first-day-of-week) in `src/SimplestCalendar.Core/Calendar/MonthGridBuilder.cs`
- [x] T012 Implement JSON `IEntryStore` / `ISettingsStore` with atomic writes per [contracts/data-store.md](./contracts/data-store.md) in `src/SimplestCalendar.Core/Services/` — **in-memory session cache**; load once at startup; persist on save (no per-UI-tick disk I/O); see [decisions.md](./decisions.md) §4
- [x] T013 Implement retention wipe `WipeBefore(monthStart)` in Core persistence service
- [x] T014 [P] Unit tests for sort, split, navigation, grid, wipe, store round-trip in `tests/SimplestCalendar.Core.Tests/`
- [x] T015 Wire App startup: resolve portable data dir, load settings/entries into memory, run wipe, apply dark theme; empty MainWindow shell with Settings button placeholder
- [x] T016 Single-instance activation guard in App project (second launch → show existing main window / same process as tray)

**Checkpoint**: `dotnet test` green; app launches dark empty shell

---

## Phase 3: User Story 1 — View current month whiteboard (Priority: P1) 🎯 MVP

**Goal**: Month grid, today mark, hover highlight, forward nav ≤11, Today control, title format

**Independent Test**: See spec US1 acceptance scenarios

### Tests

- [x] T017 [P] [US1] Unit tests for month title formatting tokens in `tests/SimplestCalendar.Core.Tests/MonthTitleFormatterTests.cs`
- [x] T018 [P] [US1] Unit tests extending navigation/grid edge cases (cap at +11) in Core.Tests

### Implementation

- [x] T019 [US1] `MonthTitleFormatter` in `src/SimplestCalendar.Core/Calendar/`
- [x] T020 [US1] Main month ViewModel in `src/SimplestCalendar/ViewModels/MonthViewModel.cs` (current month, navigate, today)
- [x] T021 [US1] Month grid XAML view with day cells, today style, hover highlight (no tooltip) per winui-app skill in `src/SimplestCalendar/Views/MonthView.xaml`
- [x] T022 [US1] Compact Today + next-month controls in main chrome; disable next at +11 cap
- [x] T023 [US1] Bind entry summaries into day cells (read-only for now if US2 incomplete: empty OK)

**Checkpoint**: Month UI usable without add/edit

---

## Phase 4: User Story 2 — Add and edit day entries (Priority: P1)

**Goal**: Shared add/edit form, day list modal, sort on grid, overlap warn, emoji panel, middle-click delete

**Independent Test**: Spec US2 scenarios

### Tests

- [x] T024 [P] [US2] Unit tests for overlap detection in `tests/SimplestCalendar.Core.Tests/OverlapTests.cs`
- [x] T025 [P] [US2] Unit tests for create/update/delete + split persistence in Core.Tests

### Implementation

- [x] T026 [US2] Overlap detector in `src/SimplestCalendar.Core/Calendar/OverlapDetector.cs`
- [x] T027 [US2] Entry editor ViewModel + validation (note required; duration needs start) in `src/SimplestCalendar/ViewModels/EntryEditorViewModel.cs`
- [x] T028 [US2] Shared EntryEditor dialog/page XAML (note, time, duration hours, star, reminder section stubs OK if US3 later, emoji button) in `src/SimplestCalendar/Views/EntryEditorView.xaml`
- [x] T029 [US2] Left-click day → open editor for create
- [x] T030 [US2] Day entries modal list (edit/delete) on right-click when non-empty; nothing when empty — `src/SimplestCalendar/Views/DayEntriesListView.xaml`
- [x] T031 [US2] Middle-click delete on highlighted list row
- [x] T032 [US2] Invoke Windows emoji panel into note box from emoji button (app Services adapter)
- [x] T033 [US2] Persist saves; refresh month grid summaries with sort rules; overlap warning UI

**Checkpoint**: Full whiteboard note workflow without reminders/tray

---

## Phase 5: User Story 6 — Monthly whiteboard wipe (Priority: P2)

**Goal**: Auto-delete past-month entries on startup and month rollover while running

**Independent Test**: Spec US6

### Tests

- [x] T034 [P] [US6] Unit tests for rollover detection helper in Core.Tests

### Implementation

- [x] T035 [US6] Ensure wipe on startup (already T013) is invoked from App
- [x] T036 [US6] Timer/clock hook to detect local month change while running; wipe + refresh UI

**Checkpoint**: Past months cannot accumulate

---

## Phase 6: User Story 4 — Tray lifetime and Quit (Priority: P2)

**Goal**: Close-to-tray; Show / Settings / Quit; single process

**Independent Test**: Spec US4

### Implementation

- [x] T037 [US4] Tray icon adapter (Win32/NativeTray) in `src/SimplestCalendar/Services/TrayService.cs`
- [x] T038 [US4] Close main window → hide to tray (same process); tray Show restores **existing** main window; Quit exits
- [x] T039 [US4] Tray tooltip “Simplest Calendar”; menu Show / Settings / Quit (Settings = same process window, not new exe)
- [x] T040 [US4] Avoid duplicate tray icons with single-instance (T016); verify Show never spawns a second process

**Checkpoint**: App survives without main window

---

## Phase 7: User Story 3 — Reminders via OS notifications (Priority: P2)

**Goal**: Lead minutes, presets, toasts, compact view, no missed replay, no merge

**Independent Test**: Spec US3

### Tests

- [x] T041 [P] [US3] Unit tests for notify-time calculation (timed + untimed) in Core.Tests
- [x] T042 [P] [US3] Unit tests for scheduler skip-past behavior in Core.Tests

### Implementation

- [x] T043 [US3] `ReminderScheduler` domain helper in Core + app-hosted timer
- [x] T044 [US3] Complete reminder fields in EntryEditor (toggle, presets 5/10/15/30, custom, default 5)
- [x] T045 [US3] Windows toast notifier adapter (unpackaged AUMID registration as required) in `src/SimplestCalendar/Services/ToastService.cs`
- [x] T046 [US3] Compact notification view window/page on toast activation
- [x] T047 [US3] Respect OS Focus Assist/DND by using stock toast APIs only

**Checkpoint**: Tray + reminders work together

---

## Phase 8: User Story 5 — Settings and portable preferences (Priority: P3)

**Goal**: Tabbed settings window; first day of week; title format; startup; window bounds; dark-only appearance stub

**Independent Test**: Spec US5

### Tests

- [x] T048 [P] [US5] Unit tests for settings defaults and serialization in Core.Tests

### Implementation

- [x] T049 [US5] Settings ViewModel + tabbed window (Calendar / General / Appearance / About-Data) in `src/SimplestCalendar/Views/SettingsWindow.xaml`
- [x] T050 [US5] Wire FirstDayOfWeek + MonthTitleFormat to month grid live/on-save
- [x] T051 [US5] Launch-at-startup toggle (default off) via Windows startup APIs suitable for unpackaged apps
- [x] T052 [US5] Remember window size/position; enforce min 900×600
- [x] T053 [US5] Appearance tab: Dark selected; light/system disabled or stubbed
- [x] T054 [US5] About/Data tab shows portable data path; Settings openable from tray without main window

**Checkpoint**: Spec settings complete

---

## Phase 9: Polish & Cross-Cutting

- [x] T055 [P] Enable coverlet in test project; verify ≥50% line coverage on Core; document command in `specs/001-calendar-mvp/quickstart.md`
- [x] T056 [P] README with portable run instructions (English)
- [x] T057 Run [quickstart.md](./quickstart.md) manual smoke checklist; fix blockers
- [x] T058 Self-contained publish script/notes for classic exe distribution (optional stretch if time)

---

## Dependencies & Execution Order

- Phase 1 → Phase 2 (blocks all) → US1 (Phase 3) → US2 (Phase 4) → US6 (Phase 5) → US4 (Phase 6) → US3 (Phase 7) → US5 (Phase 8) → Polish
- US6 can start right after Phase 2 persistence exists; ordered after US2 here so wipe is validated with real entries
- US3 depends on US4 tray host for closed-window delivery
- US5 Settings can partially proceed after Phase 2 but tray entry needs US4

### Parallel opportunities

- T003/T004, T007–T011, T017/T018, T024/T025, T041/T042, T048, T055/T056

## Implementation Strategy

1. Finish Setup + Foundational  
2. Ship US1 month grid (demoable)  
3. Add US2 entries (true MVP whiteboard)  
4. Wipe → Tray → Reminders → Settings → coverage/README  
