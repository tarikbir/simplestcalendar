# Data Model: Simplest Calendar MVP

## Entry

| Field | Type | Rules |
|-------|------|-------|
| Id | GUID/string | Unique |
| Date | calendar date | Local date of the cell this segment belongs to |
| Note | string | Required, multi-line, may include emoji; no hard max for v1 |
| StartTime | time? | Null = untimed |
| DurationHours | int? | Null if untimed or no duration; if set, ≥1; requires StartTime |
| EndTime | time? | Derived for display; may be 00:00 for midnight-ending segment |
| IsStarred | bool | Default false |
| ReminderEnabled | bool | Default false |
| ReminderLeadMinutes | int | Default 5 when enabled; >0 |
| SplitGroupId | GUID/string? | Shared across midnight-split segments; null if not split |
| SplitIndex | int? | 0 = first segment, 1 = second; null if not split |
| CreatedAt / UpdatedAt | datetime | Local timestamps for debugging |

### Sorting (display)

1. `IsStarred == true` first  
2. Within star group: untimed before timed  
3. Timed: ascending `StartTime`  
4. Then non-starred untimed  
5. Then non-starred timed ascending `StartTime`  
6. Stable tie-break: `CreatedAt` then `Id`

### Midnight split

Input: date D, start S, duration H hours.  
If end stays on D: one entry.  
If end crosses midnight:  
- Entry A: Date=D, Start=S, End=00:00 (or duration remainder to midnight), Reminder as user set  
- Entry B: Date=D+1, Start=00:00, End=remaining, ReminderEnabled=false, same Note/Star/SplitGroupId  

If D+1 is beyond retention/navigation concerns, still store B until wipe rules apply.

### Overlap

Timed entries on the same `Date` overlap if time ranges intersect (treat end as exclusive at midnight boundary as implemented consistently). Warn only; still save.

## AppSettings

| Field | Type | Default |
|-------|------|---------|
| FirstDayOfWeek | weekday enum | Monday |
| MonthTitleFormat | string | `MMMM` |
| LaunchAtStartup | bool | false |
| Theme | enum | Dark (only active value v1) |
| WindowWidth / Height / X / Y | numbers | sensible defaults; min 900×600 |
| WindowState | normal/maximized | normal |

## Derived: ReminderOccurrence

- Timed + reminder: `Date+StartTime - LeadMinutes`  
- Untimed + reminder: local midnight of `Date` minus lead minutes  
- If occurrence `< now` at scheduling time and process was down: skip (no catch-up)

## Retention

Delete all entries with `Date < first day of current local month` on startup and on detected month change while running.

## Persistence files (logical)

- `entries.json` — array of Entry  
- `settings.json` — AppSettings  

Atomic write: write temp file then replace.

**Session rule**: While the process runs, in-memory collections are authoritative; JSON is the durable snapshot written on save (see [decisions.md](./decisions.md) §4).
