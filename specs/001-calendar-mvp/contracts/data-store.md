# Contract: Portable Data Store

## Paths

- Root: `<exe-directory>/data/`
- `entries.json`
- `settings.json`

## entries.json

```json
{
  "version": 1,
  "entries": [
    {
      "id": "uuid",
      "date": "2026-10-25",
      "note": "string",
      "startTime": "18:00:00",
      "durationHours": 2,
      "endTime": "20:00:00",
      "isStarred": true,
      "reminderEnabled": true,
      "reminderLeadMinutes": 5,
      "splitGroupId": null,
      "splitIndex": null,
      "createdAt": "2026-10-25T12:00:00",
      "updatedAt": "2026-10-25T12:00:00"
    }
  ]
}
```

- `startTime` / `endTime` / duration fields omitted or null when untimed.
- `appVersion` (string) written on save for diagnostics; optional on load.
- Schema rules (migrate-up, future refuse + backup, missing `version` ≡ 1): see [../../002-data-schema-versioning/contracts/data-store.md](../../002-data-schema-versioning/contracts/data-store.md).
- Unknown future `version`: refuse load with safe empty state + user-visible error once (non-destructive backup copy preferred).

## settings.json

```json
{
  "version": 1,
  "firstDayOfWeek": "Monday",
  "monthTitleFormat": "MMMM",
  "launchAtStartup": false,
  "theme": "Dark",
  "window": {
    "width": 1100,
    "height": 720,
    "x": null,
    "y": null,
    "state": "Normal"
  }
}
```

## Operations

| Op | Behavior |
|----|----------|
| LoadEntries | Missing file → empty list; called at process start into memory |
| SaveEntries | Atomic replace; called after edits (or debounced autosave), not on every UI tick |
| LoadSettings | Missing → defaults; once at start into memory |
| SaveSettings | Atomic replace when settings change |
| WipeBefore(monthStart) | Persist entries with `date >= monthStart` only |

**I/O pattern**: In-memory session is source of truth while running; disk is the durable snapshot. See [decisions.md](../decisions.md) §4.

No network; no encryption in v1 (local personal files).
