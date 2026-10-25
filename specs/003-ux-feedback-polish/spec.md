# Feature Specification: UX Feedback Polish

**Feature Branch**: `003-ux-feedback-polish`

**Created**: 2026-10-25

**Status**: Draft

**Input**: User feedback batch: editor layout/time/duration UX, month nav Prev–Today–Next, today/star/highlight/scroll/fullscreen growth, independent split deletes, minute-aware duration (23:30+1h→00:30), gear settings, Fraunces month font, Resources placeholder icon, clean Quit without hang.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Minute-aware timed duration and midnight split (Priority: P1)

A user creates a timed entry with hours and minutes (for example 2 hours 15 minutes). When an entry starts at 23:30 with a 1-hour duration, the end is 00:30 the next day (split into two independent day rows if needed). Whole-hour-only duration is no longer acceptable.

**Why this priority**: Wrong end times and missing minutes break core whiteboard accuracy.

**Independent Test**: Save 23:30 + 1h → next-day segment ends 00:30; save 18:00 + 2h15m → same-day end 20:15.

**Acceptance Scenarios**:

1. **Given** start 23:30 and duration 1 hour 0 minutes, **When** saved, **Then** the timed span ends at 00:30 next calendar day (presented as one or two day-bound rows consistent with midnight crossing).
2. **Given** start 18:00 and duration 2 hours 15 minutes, **When** saved, **Then** the same-day entry shows end 20:15.
3. **Given** a timed duration shorter than 1 minute, **When** the user tries to save, **Then** save is blocked with a clear validation message.
4. **Given** existing entries that stored whole hours only, **When** data is loaded after upgrade, **Then** those durations remain equivalent (N hours → N×60 minutes).

---

### User Story 2 - Independent split segments (Priority: P1)

When a timed entry crosses midnight, the app still creates two day rows, but deleting or editing one row does not automatically remove or rewrite the other. The user owns each deletion.

**Why this priority**: Accidental group delete loses notes the user wanted to keep.

**Independent Test**: Create midnight-crossing entry → delete first segment → second remains; delete second alone → first remains.

**Acceptance Scenarios**:

1. **Given** a two-segment midnight split, **When** the user deletes only the first segment, **Then** the second segment remains.
2. **Given** a two-segment midnight split, **When** the user edits only one segment’s note, **Then** the other segment is unchanged unless the user edits it separately.
3. **Given** either segment, **When** deleted via day list or middle-click, **Then** only that segment is removed.

---

### User Story 3 - Month navigation Prev / Today / Next (Priority: P1)

The user moves months with left-aligned Previous, Today, and Next controls. Previous never goes before the current calendar month. Next remains capped at +11 months ahead. Settings stays separate on the right.

**Why this priority**: Forward-only navigation trapped users in future months.

**Independent Test**: Go Next → Prev returns; on current month Prev is disabled; Today jumps home.

**Acceptance Scenarios**:

1. **Given** the current month, **When** the header renders, **Then** Prev / Today / Next are left-aligned as a group and Prev is disabled.
2. **Given** a future viewed month, **When** the user activates Prev, **Then** the view moves one month earlier, not before the current month.
3. **Given** any allowed future month, **When** the user activates Today, **Then** the view returns to the current month.

---

### User Story 4 - Readable month grid: today, stars, growth, scroll (Priority: P1)

On the main month grid, today has a distinct cell background (not only blue day number). Starred entries show a star in the cell summary. The grid grows with window height (fullscreen has no empty dead band). Days with many notes scroll inside the cell so entries are not clipped after ~4 items.

**Why this priority**: The whiteboard is unusable if today, importance, and overflow notes are invisible.

**Independent Test**: Mark today visually; star appears on grid; maximize window → cells expand; add 5+ notes → scroll within cell.

**Acceptance Scenarios**:

1. **Given** today is in the viewed month, **When** the grid renders, **Then** today’s cell background is distinctly colored in addition to the day-number treatment.
2. **Given** a starred entry, **When** shown on the month grid, **Then** its summary includes a star marker.
3. **Given** the main window is maximized, **When** the grid lays out, **Then** day cells expand vertically to fill available space rather than leaving a large empty band.
4. **Given** a day with five or more entry summaries, **When** the cell cannot show them all, **Then** the user can scroll the entry list inside that cell.

---

### User Story 5 - Clean application Quit (Priority: P1)

Closing the main window respects Settings → General → “Close app on exit” (default Off): Off hides to tray; On quits. Tray Quit always ends the process. Neither path hangs, freezes, or shows “Not responding.” Window state is still persisted when practical.

**Why this priority**: Hang-on-close makes the product feel broken; default close-to-tray matches modern desktop apps.

**Independent Test**: With Close app on exit Off, window close keeps the process alive in the tray; tray Quit exits within a few seconds. With the setting On, window close exits the same way. UI remains responsive until exit.

**Acceptance Scenarios**:

1. **Given** Close app on exit is Off (default) and the tray is available, **When** the user closes the main window, **Then** the process stays running in the tray and the window can be shown again from the tray.
2. **Given** Close app on exit is On, **When** the user closes the main window, **Then** the process exits without an unresponsive hang.
3. **Given** the tray Quit command, **When** activated, **Then** the process exits cleanly regardless of Close app on exit.

---

### User Story 6 - Entry editor UX (Priority: P2)

The add/edit form uses an intuitive start-time control (not free-typed HH:mm as the primary path), puts timed duration hours+minutes on one row (dialog may grow), and places the emoji control on the right of the note/title area—not beside the time field.

**Why this priority**: Editor friction blocks daily capture.

**Independent Test**: Open editor → TimePicker for start; duration row has hours and minutes; emoji sits by note.

**Acceptance Scenarios**:

1. **Given** the editor, **When** setting a start time, **Then** the user can pick time without typing a free-form HH:mm string as the primary method.
2. **Given** timed duration enabled, **When** the form shows, **Then** duration hours and minutes appear on a single row with the timed-duration control.
3. **Given** the editor, **When** looking for emoji, **Then** the emoji control is right-aligned relative to the note/title field, not beside start time.

---

### User Story 7 - Hover highlight and middle-click delete (Priority: P2)

Hovering a day highlights the day cell and emphasizes that day’s entry summaries. Hovering a specific summary line emphasizes that line further. Middle-click on a summary line on the month grid deletes that entry without opening add/edit. Middle-click elsewhere on the day cell does nothing (does not open add). Left-click still opens add. In the day entry list modal, middle-click on a row also deletes that entry.

**Why this priority**: Highlight and middle-click are daily power interactions.

**Independent Test**: Hover day → cell + entries emphasized; hover one line → stronger; middle-click line → deleted, no add dialog; middle-click empty cell → no dialog; day-list middle-click still deletes.

**Acceptance Scenarios**:

1. **Given** the pointer over a day cell, **When** hover is active, **Then** the cell background and the entry summary lines in that cell are visually emphasized.
2. **Given** the pointer over a specific entry summary line, **When** hover is active, **Then** that line is emphasized more strongly than sibling lines.
3. **Given** an entry summary line on the month grid, **When** the user middle-clicks it, **Then** that entry is deleted without opening the add/edit form.
4. **Given** a day cell (empty area or day number, not an entry line), **When** the user middle-clicks, **Then** no add/edit dialog opens.
5. **Given** the day list modal with a row under the pointer, **When** the user middle-clicks, **Then** that entry is deleted without opening the editor.

---

### User Story 8 - Visual chrome: font, gear, resources icon (Priority: P3)

The month title uses a distinctive bundled display font (Fraunces from Google Fonts, packaged with the app). Settings is a gear icon. App icons and replaceable visuals live under a Resources folder with a calendar-style placeholder the user can replace before the next build.

**Why this priority**: Polish and brand; does not block core capture.

**Independent Test**: Month title renders in Fraunces; Settings shows gear; Resources placeholder icon used for window/tray paths.

**Acceptance Scenarios**:

1. **Given** the main header, **When** the month title is shown, **Then** it uses the bundled Fraunces display face (not the default UI body font alone).
2. **Given** the main header, **When** opening Settings, **Then** the control is a gear icon rather than the word “Settings.”
3. **Given** a fresh build, **When** inspecting app visuals, **Then** icon assets resolve from an `Assets/Resources` placeholder set the user can overwrite.

---

### Edge Cases

- Duration 0 hours and 0 minutes is rejected.
- Start near midnight with duration that ends exactly at midnight.
- Prev on current month remains disabled; Next at +11 months remains disabled.
- Empty day hover still highlights the cell; no entry lines to emphasize.
- Quit while a save is in progress still exits without deadlock.
- Migrating mixed timed/untimed entries leaves untimed entries unchanged.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Timed duration MUST support hours and minutes; minimum total duration is 1 minute.
- **FR-002**: End time MUST equal start plus duration across midnight correctly (e.g. 23:30 + 60 minutes → 00:30 next day).
- **FR-003**: Midnight-crossing materialization MAY create two day-bound rows; delete and edit MUST affect only the targeted row.
- **FR-004**: Month header MUST provide left-aligned Previous, Today, and Next; Previous MUST NOT open months before the current month; Next MUST remain capped at 11 months ahead.
- **FR-005**: Today’s cell MUST use a distinct background treatment in addition to day-number styling.
- **FR-006**: Starred entries MUST show a star on month-grid summaries.
- **FR-007**: Month grid day cells MUST expand with available window height.
- **FR-008**: Day cells with overflowing entry summaries MUST allow in-cell vertical scrolling.
- **FR-009**: Application Quit (tray Quit, or window close when Close app on exit is On) MUST exit without hanging the UI. When Close app on exit is Off (default), window close MUST hide to the tray instead of quitting.
- **FR-010**: Entry editor MUST use a native time-picking control for start time as the primary input.
- **FR-011**: Timed duration controls MUST appear on one row; the editor dialog MAY grow taller as needed.
- **FR-012**: Emoji control MUST sit to the right of the note/title field.
- **FR-013**: Day hover MUST emphasize both the cell and its entry summaries; hovering a specific summary line MUST emphasize that line further.
- **FR-014**: Middle-click on a month-grid entry summary line MUST delete that entry only and MUST NOT open add/edit; middle-click on non-entry areas of a day cell MUST NOT open add/edit; day-list middle-click MUST also delete the pointed entry only.
- **FR-015**: Month title MUST use bundled Fraunces; Settings MUST be a gear icon; replaceable icons MUST live under `Assets/Resources` with a calendar placeholder.
- **FR-016**: On-disk data MUST migrate prior whole-hour durations to minute-equivalent values without user action.

### Key Entities

- **Entry**: Day note; optional start; optional duration in total minutes; optional end; star; reminder; optional split metadata that does not couple delete.
- **Month view**: Viewed month bounded by current month … current+11; today marker; hover state.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 23:30 + 1 hour always ends at 00:30 next day in saved data and UI summaries.
- **SC-002**: Users can enter at least 2h 15m duration without converting to clock-only fields.
- **SC-003**: Deleting one midnight segment never removes the sibling segment.
- **SC-004**: From a future month, users return to the current month with one Prev chain or Today in under 3 seconds of interaction.
- **SC-005**: With 5+ entries on a day, all summaries are reachable via in-cell scroll.
- **SC-006**: Maximized window uses vertical space for day cells (no large empty band below the grid).
- **SC-007**: Quit completes without a “Not responding” hang in normal use.
- **SC-008**: First-time users can set start time and duration without typing free-form `HH:mm` as the only path.

## Assumptions

- Window close defaults to hide-to-tray; Settings → Close app on exit (default Off) opts into quit-on-close. Tray Show/Settings/Quit otherwise unchanged; tray Quit always exits.
- Middle-click quick-delete is primary on month-grid entry summary lines; day-list middle-delete remains as a secondary path.
- Left-click on a day cell remains the only pointer path that opens add from the grid.
- In-cell scroll is the overflow solution; month-grid page scroll is out of scope.
- Fraunces is the chosen Google Font, bundled locally (no CDN at runtime).
- Schema version increments once for duration field migration.
- Dark theme only remains in force.
- Reminder/toast semantics unchanged except where duration affects end time.
