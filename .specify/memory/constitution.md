<!--
Sync Impact Report
- Version change: 1.0.0 → 1.0.1
- Modified principles: none (clarifications only)
- Added sections: none
- Removed sections: N/A
- Clarified: JSON in-memory I/O pattern; tray Show = same process/window; decisions.md as product lock-in
- Follow-up TODOs: none
-->

# Simplest Calendar Constitution

## Core Principles

### I. Personal Whiteboard Simplicity
Simplest Calendar is a personal, offline month whiteboard — not a full PIM,
mail client, or multi-user product. Visible UI MUST stay minimal: month grid,
entries, settings, tray. Prefer the smallest behavior that solves the stated
need. Do not add accounts, sync, sharing, or speculative features.

**Rationale**: The product exists to replace a physical whiteboard that is
wiped each month, not to compete with Outlook or Google Calendar.

### II. Local-Only, Portable Data
All application data MUST live beside the executable (portable folder layout).
No cloud accounts. No mandatory installer paths under AppData as the primary
store. Persistence MUST remain readable/writable without elevated privileges
for normal user folders.

**Rationale**: Users may keep the app on a dedicated drive (e.g. `D:/storage/
SimplestCalendar`) so a Windows reinstall does not erase notes.

### III. Prefer OS Capabilities
Where Windows 11 exposes a suitable capability, use it: system tray, toast
notifications (respecting Focus Assist / DND), emoji panel, theme resources,
windowing. Avoid reinventing OS chrome or shipping a second notification stack.

**Rationale**: Native behavior is more reliable, familiar, and cheaper to
maintain than custom substitutes.

### IV. Test Domain Logic; Keep UI Thin
Business rules (calendar month bounds, entry sorting, duration split across
midnight, retention wipe, reminder timing, settings defaults) MUST live in
testable .NET libraries with unit tests. UI (WinUI XAML) stays thin. Minimum
line coverage for testable assemblies: **50%**. UI automation is optional and
not required for MVP unless a flow cannot be covered by unit tests.

**Rationale**: Calendar and reminder bugs are costly; thin UI plus unit tests
catch regressions without fragile full-UI harnesses.

### V. Spec-Driven Delivery
Non-trivial behavior changes MUST go through Spec Kit (`/speckit-specify` →
plan → tasks → implement) and remain consistent with this constitution. WinUI
UI/UX work MUST follow the project `winui-app` skill (Fluent-first, native
controls, dark theme for v1).

**Rationale**: Spec-first prevents silent scope creep; the WinUI skill keeps
visual and interaction choices coherent.

## Stack & Product Boundaries

- **Runtime**: .NET 10, C#, WinUI 3 (Windows App SDK), unpackaged / standalone
  `.exe` suitable for classic portable use on **Windows 11**.
- **Language**: English UI only for v1 (no localization switch).
- **Theme**: Dark only for v1; theming infrastructure MAY exist for later
  light/system modes, but MUST NOT ship alternate themes yet.
- **Storage**: Prefer a simple local JSON store unless proven inadequate;
  SQLite only if JSON I/O becomes a real bottleneck (expected data volume is
  tiny: current month + up to 11 future months). Runtime MUST keep the working
  set in memory and persist on save — not continuous disk serialize/deserialize.
  Product decision lock-in: `specs/001-calendar-mvp/decisions.md`.
- **Calendar math**: Use .NET BCL date/time APIs (`DateOnly` / `TimeOnly` /
  `DateTime` / `Calendar`) — no external calendar data service for v1.
- **Retention**: Entries belonging to months before the current calendar month
  MUST be deleted (whiteboard wipe). Navigation MAY look forward at most
  **11 months** from the current month (not 12). Past months are not navigable.
- **Process model**: Single process; closing the main window hides to tray;
  tray Show restores the existing main window of that process (never a second
  app instance); Quit from tray ends the process. Keep idle resource use low
  (no busy loops).
- **Window chrome**: No application-level right-click context menu on the main
  shell. Day-level edit lists use an intentional modal/flyout UI, not a generic
  window context menu. Tray may expose Show / Settings / Quit; Settings is
  another window of the same process.

## Quality Gates

- Domain/service unit tests accompany rule changes; run `dotnet test` before
  considering a change done.
- Unpackaged build must launch to a visible main window (or tray-only start
  if that mode is explicitly specified later).
- Reminder delivery uses Windows notification APIs and MUST respect OS DND /
  Focus Assist defaults.

## Governance

This constitution supersedes informal chat decisions when they conflict.
Amendments require updating this file with a Sync Impact Report comment,
semantic version bump (MAJOR for incompatible principle changes, MINOR for
new principles/sections, PATCH for clarifications), and Spec Kit follow-up
if active specs are affected. Reviews MUST check simplicity, portability,
OS-native preference, and test coverage floors.

**Version**: 1.0.1 | **Ratified**: 2026-10-25 | **Last Amended**: 2026-10-25
