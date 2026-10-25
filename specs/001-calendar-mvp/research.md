# Research: Simplest Calendar MVP

## Decision: WinUI 3 (Windows App SDK), unpackaged

- **Rationale**: Microsoft’s current Windows desktop UI stack; Fluent/dark theme; Windows 11 native look; matches user choice over WPF.
- **Alternatives considered**: WPF (easier classic tray samples, older stack); Avalonia (cross-platform not needed).
- **Notes**: Tray icon is not first-party in WinUI; use Win32 `Shell_NotifyIcon` / a small helper (e.g. NativeTray or hand-rolled P/Invoke). Prefer one thin adapter in the app project.

## Decision: Portable JSON beside executable

- **Rationale**: Expected data volume is tiny (current month + ≤11 future months, wiped monthly). JSON is simplest to inspect/debug; SQLite is unnecessary until proven slow. Reconfirmed 2026-10-25 after size/library vs ser-deser discussion: SQLite file/libs are not “too big,” but add no benefit at this scale.
- **Runtime pattern (mandatory)**: Keep the working set **in memory**. Deserialize once at startup; serialize on explicit save and optional debounced autosave after edits. Do **not** read/write JSON on every UI interaction or reminder tick.
- **Alternatives considered**: SQLite, LiteDB, `%LocalAppData%` store (rejected for portability preference).
- **Notes**: Resolve data root as `AppContext.BaseDirectory` (or exe directory) + `data/`. Document that debugging from `bin/` writes beside build output — acceptable for portable design. Full product rationale: [decisions.md](./decisions.md) §4.

## Decision: Domain library + xUnit + coverlet

- **Rationale**: Constitution requires ≥50% line coverage on testable assemblies; UI tests deferred.
- **Alternatives considered**: UI automation (WinAppDriver) — optional later, not MVP gate.

## Decision: Reminder scheduling in-process

- **Rationale**: Single tray process; compute next due reminder and set one timer (or short idle check) rather than Windows Task Scheduler complexity for v1.
- **Alternatives considered**: Background task / Task Scheduler — heavier, harder for portable unpackaged apps.
- **Missed reminders**: On startup, skip any notify time already in the past (spec).

## Decision: Windows App SDK toast notifications

- **Rationale**: OS notification center, Focus Assist/DND respect by default.
- **Notes**: Unpackaged apps must register an AUMID / sparse package or use supported unpackaged toast paths per current Windows App SDK docs — capture exact registration steps during implementation bootstrap (`winui-app` lifecycle/notifications reference).

## Decision: Emoji via OS panel

- **Rationale**: User requested no custom emoji catalog; invoke Windows emoji panel (typically Win+.) focused on the note control.
- **Alternatives considered**: In-app emoji picker — deferred.

## Decision: Settings as a second window of the same process

- **Clarification**: This is **not** a second application. Tray **Show** restores the existing main calendar window; tray **Settings** opens Settings inside the same process. “Non-modal” only means Settings does not lock a hidden owner window the way a modal dialog would.
- **Rationale**: Usable from tray while main window is hidden; avoids modal-over-hidden-owner issues; matches user intent (“trayden aynı uygulama açılsın”).
- **Tabs**: Calendar | General | Appearance | About/Data.
- **See**: [decisions.md](./decisions.md) §3.

## Decision: Month title format tokens

- **Rationale**: User wants customizable format; default month name only.
- **Proposed tokens** (implement & document in Settings help text): `MMMM` full month, `MMM` abbr, `MM` numeric, `yyyy`/`yy` year. Invalid patterns fall back to `MMMM`.

## Decision: Calendar grid math via BCL

- **Rationale**: No external holiday API; `DateOnly` + culture English weekday names; first-day-of-week from settings.

## Decision: UI guidance skill

- **Choice**: Copied OpenAI curated `winui-app` skill into `.cursor/skills/winui-app` (Fluent, controls, theming, notifications references).
- **Alternatives**: Microsoft `win-dev-skills` tree currently emphasizes WinDbg plugins in the cloned snapshot; `winui-app` is the more complete WinUI UX/dev skill for this repo.
