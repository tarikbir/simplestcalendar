# Data Model: 003-ux-feedback-polish

## Entry (schema v2)

| Field | Type | Notes |
|-------|------|--------|
| id | Guid | Unchanged |
| date | DateOnly | Unchanged |
| note | string | Unchanged |
| startTime | TimeOnly? | Unchanged |
| durationMinutes | int? | **New** — total timed length; null if no timed duration |
| endTime | TimeOnly? | Computed on materialize |
| isStarred | bool | Unchanged |
| reminderEnabled | bool | Unchanged |
| reminderLeadMinutes | int | Unchanged |
| splitGroupId | Guid? | Metadata only; does not couple delete |
| splitIndex | int? | Unchanged |
| createdAt / updatedAt | DateTime | Unchanged |

**Removed from on-disk v2**: `durationHours`.

## Migration 1 → 2

For each entry object in JSON:

- If `durationHours` present and numeric: set `durationMinutes = durationHours * 60`, remove `durationHours`.
- Else leave duration fields as-is / null.

Bump root `version` to 2.

## EntryDraft

`DurationMinutes` replaces `DurationHours`. Validate: if set, require start time and `DurationMinutes >= 1`.
