# Feature Specification: Simplest Calendar MVP

**Feature Branch**: `001-calendar-mvp`

**Created**: 2026-10-25

**Status**: Draft

**Input**: User description: "WinUI 3 offline Windows 11 month whiteboard calendar: portable local data, month grid, day entries with optional time/duration/star/reminder/emoji, tray background process, OS notifications, settings, unit tests ≥50% coverage."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View current month whiteboard (Priority: P1)

A user opens Simplest Calendar and sees the current calendar month as the main screen. Days show entry summaries (`time - note` or note-only). Today is visually marked. Hovering a day highlights it. The user can move forward up to 11 months ahead (not 12) and return with a compact Today control. Past months are not available.

**Why this priority**: The month grid is the product; without it there is no whiteboard.

**Independent Test**: Launch app → current month visible → today marked → hover highlight → forward navigation capped at +11 months → Today returns to current month.

**Acceptance Scenarios**:

1. **Given** the app starts on a valid Windows 11 machine, **When** the main window appears, **Then** the viewed month is the current local-calendar month and the English month title uses the configured format (default: month name only, e.g. `October`).
2. **Given** the main window is open, **When** the pointer hovers a day cell, **Then** that day is visually highlighted (no tooltip required).
3. **Given** today falls in the viewed month, **When** the grid renders, **Then** today's cell has a distinct “today” treatment.
4. **Given** the user is on the current month, **When** they navigate forward repeatedly, **Then** they can reach at most 11 months ahead of the current month and cannot open a 12th month ahead or any past month.
5. **Given** the user viewed a future month, **When** they activate Today, **Then** the view returns to the current month without cluttering the header.

---

### User Story 2 - Add and edit day entries (Priority: P1)

The user left-clicks any day (empty or occupied) to add an entry via a shared add/edit form. Required: note text. Optional: start time (empty = all-day/untimed), timed duration in whole hours (displayed as start–end; midnight crossing splits into two day entries), star (important), reminder with lead minutes (presets 5/10/15/30, custom allowed, default 5), and an emoji control that opens the Windows emoji panel into the note. Right-click on a day with entries opens a modal list (not a window context menu) to pick an entry to edit or delete; middle-click on a highlighted list row deletes without opening the editor.

**Why this priority**: Capturing notes on days is the core whiteboard workflow.

**Independent Test**: Add timed/untimed/starred entries → see them on the grid sorted correctly → edit via day list → delete via list and middle-click.

**Acceptance Scenarios**:

1. **Given** any day cell, **When** the user left-clicks it, **Then** the shared add/edit form opens for a new entry on that day.
2. **Given** the form is open, **When** the user tries to save with an empty note, **Then** save is blocked and the note field is indicated as required.
3. **Given** a saved entry with no start time, **When** the month grid shows that day, **Then** the summary appears without a time and sorts above timed (non-starred) entries per the sort rules.
4. **Given** start time 18:00 and duration 2 hours, **When** saved, **Then** the day shows `18:00 - 20:00` style range (or equivalent clear start–end presentation) for that entry.
5. **Given** start time 23:00 and duration 3 hours, **When** saved, **Then** two entries exist: same-day `23:00–00:00` and next-day `00:00–02:00`, sharing note/star content rules defined in Assumptions.
6. **Given** a new timed entry overlaps an existing timed entry that day, **When** the user saves, **Then** a warning is shown but save is still allowed.
7. **Given** a day with two entries, **When** the user right-clicks the day, **Then** a modal list of those entries appears for edit/delete selection; **Given** a day with zero entries, **When** right-clicked, **Then** nothing opens.
8. **Given** the day entry list is open and a row is highlighted, **When** the user middle-clicks that row, **Then** that entry is deleted without opening the editor.
9. **Given** the add/edit form, **When** the user activates the emoji control, **Then** the Windows emoji panel is invoked so chosen emoji inserts into the note field.
10. **Given** entries on a day, **When** listed on the grid and in the day list, **Then** order is: starred first, then untimed, then timed ascending by start time (starred untimed before starred timed if both starred — see Assumptions).

---

### User Story 3 - Reminders via OS notifications (Priority: P2)

The user enables reminder on an entry. At the computed notify time (start time minus lead minutes, or configured equivalent for untimed if applicable — see Assumptions), Windows shows a toast. Clicking the toast opens a compact read-focused notification view (smaller than the editor). Missed reminders while the PC was off are discarded silently. Multiple due reminders each get their own toast. Focus Assist / DND follows OS defaults.

**Why this priority**: Reminders are optional but must work with the always-running tray process.

**Independent Test**: Create entry with reminder + short lead → wait/simulate due time → toast appears → click opens compact view; with reminder off, no toast.

**Acceptance Scenarios**:

1. **Given** an entry with reminder disabled, **When** its start time passes, **Then** no notification is shown for that entry.
2. **Given** an entry with reminder enabled, lead 5 minutes, and start 10:00, **When** local time reaches 09:55 and the app is running in tray or windowed, **Then** a Windows toast is shown.
3. **Given** a toast for an entry, **When** the user activates it, **Then** a compact notification view for that entry opens (not the full editor by default).
4. **Given** the PC was off across a notify time, **When** the app later starts, **Then** that missed reminder is not replayed.
5. **Given** two entries due at the same moment, **When** notify time arrives, **Then** two separate toasts are produced (no merging).

---

### User Story 4 - Tray lifetime and Quit (Priority: P2)

Closing the main window keeps a **single process** alive in the system tray so reminders can fire. Tray right-click offers Show, Settings, and Quit. **Show restores the same main calendar window** (same app instance — never launches a second copy). Settings opens inside that same process. Quit ends the process. Idle resource use stays low.

**Why this priority**: Background lifetime is required for reminders without a visible window.

**Independent Test**: Close main window → tray icon remains → Show restores the existing main window → Quit exits process; no duplicate processes on normal use.

**Acceptance Scenarios**:

1. **Given** the main window is open, **When** the user closes it, **Then** the process remains with a tray icon and no second process is started.
2. **Given** the app is tray-only, **When** the user chooses Show, **Then** the existing main calendar window of that process is shown again (not a new process).
3. **Given** the tray menu, **When** the user chooses Quit, **Then** the process exits and the tray icon disappears.

---

### User Story 5 - Settings and portable preferences (Priority: P3)

From the main UI Settings control or the tray, the user opens Settings as **another window of the same running app** (not a second executable). The window is non-blocking so it can open while the main calendar window is hidden. Settings include at least: first day of week (any weekday; default Monday), month title format string (default month name only), launch at Windows startup (default off), and theming placeholders (dark only active). Window size/position are remembered; minimum 900×600.

**Why this priority**: Configuration supports personal whiteboard habits without cluttering the main surface.

**Independent Test**: Change first day of week → grid headers reorder; toggle startup off by default; resize, restart, size restored.

**Acceptance Scenarios**:

1. **Given** Settings, **When** first day of week is set to Sunday, **Then** the month grid weekday headers and column order start on Sunday after apply/save.
2. **Given** default settings, **When** Settings is opened, **Then** launch-at-startup is off.
3. **Given** the user resized the main window, **When** they restart the app, **Then** the prior size (and position if stored) is restored, not below the minimum.
4. **Given** the app is in the tray only, **When** Settings is opened from the tray, **Then** Settings appears without requiring the main window to be visible first.

---

### User Story 6 - Monthly whiteboard wipe (Priority: P2)

When a new calendar month begins (detected at startup and while running), entries dated before the current month’s first day are permanently removed so storage does not grow like a historical archive.

**Why this priority**: Matches the physical whiteboard wipe ritual and keeps the portable store small.

**Independent Test**: Seed an entry in a prior month → advance system month or simulate rollover → entry removed; current and future (within horizon) entries remain.

**Acceptance Scenarios**:

1. **Given** stored entries in months before the current month, **When** the app starts or detects month rollover, **Then** those past-month entries are deleted.
2. **Given** entries in the current month or allowed future months, **When** wipe runs, **Then** those entries remain.

---

### Edge Cases

- Duration that ends exactly at midnight creates a same-day segment only through 00:00 boundary rules without an empty next-day stub.
- Saving duration without a start time is invalid or ignored (duration requires start time).
- Navigating near the +11 month cap disables/blocks further forward navigation.
- Corrupted or missing data file: app starts with empty data and does not crash; user can continue.
- Overlapping warning appears only for timed-vs-timed overlaps on the same calendar day.
- Reminder on untimed entries: see Assumptions (notify at lead minutes before local midnight of that day, or disabled — chosen default documented there).
- Single-instance expectation: starting a second copy should focus the existing instance rather than run two tray icons (preferred).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Application MUST run offline on Windows 11 with no account or network requirement for core use.
- **FR-002**: Application MUST present an English-only UI for v1 (no language picker).
- **FR-003**: Main view MUST be a month grid for the selected month, defaulting to the current local month on launch.
- **FR-004**: Users MUST NOT navigate to months before the current month; users MAY navigate forward at most 11 months from the current month; a Today action MUST return to the current month.
- **FR-005**: Day cells MUST show entry summaries using time (or time range) and note text; today MUST be marked; hover MUST highlight the day without requiring a tooltip.
- **FR-006**: Left-click on a day MUST open the shared add form for that day whether or not entries exist.
- **FR-007**: Right-click on a day with entries MUST open a modal entry list for edit/delete; right-click on an empty day MUST do nothing.
- **FR-008**: Entry note text MUST be required; multi-line notes MUST be supported without a fixed low character cap for v1.
- **FR-009**: Entry start time MUST be optional; missing start time means untimed; untimed entries sort above timed non-starred entries per sort rules.
- **FR-010**: Optional whole-hour duration MUST compute end time; ranges crossing midnight MUST split into two day-bound entries per Assumptions.
- **FR-011**: Overlapping timed entries on the same day MUST warn on save but MUST still allow save.
- **FR-012**: Starred entries MUST show a star affordance and sort above non-starred entries.
- **FR-013**: Sort order MUST be: all starred (untimed before timed, timed by start ascending), then non-starred untimed, then non-starred timed by start ascending.
- **FR-014**: Reminder MUST be optional (default off for new entries); when on, lead minutes MUST default to 5 and support presets 5/10/15/30 plus custom positive minutes.
- **FR-015**: When reminder is due and enabled, the app MUST show a Windows toast; toast activation MUST open a compact notification view.
- **FR-016**: Missed reminders while the process was not running MUST NOT be replayed on next start.
- **FR-017**: Closing the main window MUST keep a single process in the tray; tray MUST offer Show, Settings, Quit; Show MUST reveal the existing main calendar window of that process; Quit MUST terminate the process; a second launch MUST activate the existing instance.
- **FR-018**: Main window MUST NOT expose a generic application context menu; day list UI is an intentional modal, not shell context-menu chrome.
- **FR-019**: Settings MUST open as another window of the same process (reachable from the main UI and tray), must not require a visible main window, with tabs/categories covering Calendar (first day of week, month title format), General (launch at startup), Appearance (dark theme active; future themes stubbed), and About/Data (portable data location info).
- **FR-020**: First day of week MUST be configurable to any weekday; default Monday.
- **FR-021**: Month title format MUST be user-configurable; default is month name only (no year).
- **FR-022**: Launch at Windows startup MUST be optional and default off.
- **FR-023**: Theme for v1 MUST be dark only; theming hooks MAY exist for later modes.
- **FR-024**: Main window size (and position if stored) MUST be remembered across sessions with a defined minimum size.
- **FR-025**: Application data and settings MUST persist in a portable store beside the executable.
- **FR-026**: On startup and on month rollover, entries before the current month MUST be permanently deleted.
- **FR-027**: Emoji control MUST invoke the Windows emoji panel for insertion into the note (no custom emoji catalog required for v1).
- **FR-028**: Day entry list MUST support delete; middle-click on a highlighted row MUST delete that entry.
- **FR-029**: Domain logic MUST be covered by unit tests with minimum 50% line coverage on testable assemblies; `dotnet test` is the quality gate (no CI required for v1).

### Key Entities

- **CalendarMonthView**: The month being displayed; constrained by navigation rules relative to “today’s month.”
- **DayCell**: A date within the grid (including leading/trailing padding days if shown); interaction target for click/hover.
- **Entry**: A whiteboard note bound to one calendar date, with optional start time, optional duration/end, star flag, reminder settings, and note body (may contain emoji).
- **EntrySplitGroup**: Logical link between segments created by midnight-crossing duration (same user intent).
- **AppSettings**: First day of week, month title format, startup flag, window bounds, theme mode (dark).
- **ReminderOccurrence**: Derived notify instant for an entry; delivered via OS toast or skipped if missed.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A new user can open the app and identify today on the current month grid within 10 seconds without configuration.
- **SC-002**: A user can create a timed starred note with reminder on a day in under 60 seconds using only the add form.
- **SC-003**: After closing the main window, a due reminder still produces a Windows toast without relaunching manually.
- **SC-004**: After month rollover, zero entries remain dated before the new current month.
- **SC-005**: Forward navigation never exposes a month more than 11 months after the current month.
- **SC-006**: Portable folder copy (exe + data) to another path on the same machine continues to show the same entries without a reinstall wizard.
- **SC-007**: Domain unit test projects report at least 50% line coverage on testable assemblies via `dotnet test` coverage tooling.

## Assumptions

> **Ratified 2026-10-25.** Full “why” tables: [decisions.md](./decisions.md). Do not change silently.

- Target users are individuals on Windows 11; Linux/macOS are out of scope for this feature.
- Local time zone only; no multi-timezone conversion in v1.
- Storage is portable **JSON** beside the exe; working set is **in-memory** with serialize-on-save (not continuous disk ser/deser). SQLite is out for v1 unless a measured bottleneck appears.
- Month grid “padding” days outside the viewed month are non-interactive or visually muted and do not enable past-month navigation.
- Duration is whole hours only for v1 (1h, 2h, …).
- Midnight-split segments copy note text and star; reminder remains enabled only on the first segment (original start); the second segment has reminder off to avoid duplicate toasts. Segments share an `EntrySplitGroup` id for future editing enhancements.
- Untimed entries with reminder enabled notify at `local midnight of that day minus lead minutes` (i.e. previous evening).
- Compact notification view is read-oriented; v1 may include a single “Edit” action into the full form.
- Minimum main window size: **900×600** device-independent pixels; size/position remembered.
- Single-instance app: second launch activates the existing instance (same process the tray already represents).
- English month/weekday names come from BCL culture `en-US` (or invariant English) regardless of OS UI language.
- No import/export, recurrence rules beyond duration split, attachments, or calendars other than the single whiteboard in v1.
- Note field has no fixed character cap in v1 (multi-line).
