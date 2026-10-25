# Product Decisions Log — Simplest Calendar MVP

**Status**: Ratified 2026-10-25 (conversation + Spec Kit planning)  
**Purpose**: Preserve *why* each choice exists so implementers and future-us do not re-litigate or invent conflicting behavior.

Cross-links: [spec.md](./spec.md) · [plan.md](./plan.md) · [research.md](./research.md) · [data-model.md](./data-model.md) · constitution `.specify/memory/constitution.md`

---

## 1. Product identity

| Decision | Detail | Why |
|----------|--------|-----|
| Name | **Simplest Calendar**; exe/assembly `SimplestCalendar` | Simple product, simple name |
| Metaphor | Physical **whiteboard wiped each month** | User previously erased a board monthly and refilled; app must mimic that, not Outlook |
| Audience | Personal use first; Windows 11 only | Linux port is a separate future project if ever needed |
| UI language | English only; no language picker in v1 | Keep surface tiny |
| Accounts / cloud | None; fully offline | User does not want mail-app complexity |

---

## 2. Stack

| Decision | Detail | Why |
|----------|--------|-----|
| UI framework | **WinUI 3** + Windows App SDK | Modern Microsoft desktop stack; user chose over WPF when told it is current |
| Runtime | **.NET 10** | Prefer current over chasing LTS churn for a personal app |
| Packaging | **Unpackaged** classic / portable `.exe` | No Store/MSIX requirement; folder copy is the deploy model |
| UI skill | `.cursor/skills/winui-app` | Fluent/UX guidance before implementing visuals |
| Domain tests | xUnit + coverlet; **≥50%** line coverage on Core; `dotnet test` only (no CI yet) | Catch logic bugs; UI automation not required for MVP |

---

## 3. Process, tray, and windows (same app)

| Decision | Detail | Why |
|----------|--------|-----|
| Process model | **One process** only | Tray + reminders without multi-process coordination |
| Close main window | Hides to **system tray**; process keeps running | Reminders must still fire |
| Tray **Show** | Restores/shows the **existing main calendar window** of the same process — not a second app | User intent: “trayden aynı uygulama açılsın” |
| Tray **Quit** | Terminates the process | Explicit exit |
| Tray **Settings** | Opens Settings UI inside the **same process** | Convenience when main window is hidden |
| Settings window type | Separate **non-blocking** window (not a modal that locks an owner) | Clarification: “non-modal” ≠ “another application”. It means Settings is another window of *this* app so it can open while the main window is hidden, without modal-owner bugs |
| Main shell context menu | **None** on the app chrome | Simplicity; Settings via button / tray only |
| Day entry list | Intentional **modal/list UI** on day right-click — not a generic window context menu | Edit/delete entries for that day |
| Single instance | Second launch focuses existing instance | Avoid two tray icons / split state |
| Resources | Keep idle CPU/memory low (no busy loops) | Always-on tray citizen |

---

## 4. Storage (JSON, portable)

| Decision | Detail | Why |
|----------|--------|-----|
| Format | **JSON** files (`data/entries.json`, `data/settings.json`) beside exe | Tiny dataset; human-inspectable; no native SQLite binary; SQLite deferred unless proven needed |
| Location | **Portable** next to executable (e.g. `D:/storage/SimplestCalendar/`) | Survive OS reinstall/format if the folder lives on another drive; no AppData-as-primary |
| Runtime I/O model | Hold entries/settings **in memory**; serialize on save (and debounced autosave if used); deserialize **once** at startup | Avoids “constant ser/deser” cost; JSON perf is irrelevant at whiteboard scale when used this way |
| SQLite rejected for v1 | Discussed 2026-10-25 | File size and library weight are fine for SQLite, but add no value here; JSON + in-memory is enough |
| Wipe | On startup and month rollover, delete entries with date **before** current month start | Whiteboard ritual; prevent store growth |

---

## 5. Calendar navigation and grid

| Decision | Detail | Why |
|----------|--------|-----|
| Default view | Current local month | Whiteboard is “this month” |
| Past months | **Not navigable** | No historical planning in this product |
| Future months | Forward at most **11** months from current month (**not** 12) | Vacation planning yes; intentional snub of “one year from today” |
| Today control | Compact control to return to current month | Recover after browsing future months |
| First day of week | Setting; any weekday; **default Monday** | Personal preference flexibility |
| Month title | Configurable format; **default month name only** (e.g. `October`, no year) | User tokens: `MMMM` / `MMM` / `MM` / `yyyy` / `yy`; invalid → fallback `MMMM` |
| Calendar math | .NET BCL only | Simplest correct source; no holiday API |
| Time zone | Local only | Personal app; TZ portability later |
| Hover | Highlight only; **no tooltip** | UX clarity without clutter |
| Today cell | Visually marked | Obvious orientation |
| Grid summaries | `time - note` or note-only; no separate title field | Note is the content |
| Padding days | Muted / non-interactive; must not enable past-month nav | Grid completeness without breaking rules |

---

## 6. Entries (notes)

| Decision | Detail | Why |
|----------|--------|-----|
| Add | Left-click day (empty or filled) → shared add form | Speed |
| Edit/delete list | Right-click day with entries → modal list; empty day → nothing | No accidental menus |
| Same form | Create and edit share one form | Less UI to learn |
| Note | Required; multi-line; **no hard character cap** in v1 | JSON has no tiny field limit; 255 was only if DB forced it |
| Start time | Optional; empty = untimed “day note” | General todos without a clock |
| Sort | Starred → untimed → timed by start (see data-model for full rules) | Important and all-day notes rise |
| Duration | Whole **hours** only; UI shows start–end; app computes end | User picks “2 hours” not end clock primarily |
| Midnight cross | Split into two date-bound segments (e.g. 23:00–00:00 + next day 00:00–02:00), same note/star; **reminder only on first segment** | Avoid duplicate toasts; still show correct day ranges |
| Overlap | Warn only; still allow save | Simplicity over conflict engine |
| Star | Icon + sort boost | Importance |
| Emoji | Button opens **Windows emoji panel** into note; no custom catalog | Stickers later; OS panel now |
| Delete | From day list; **middle-click** highlighted row deletes without opening editor | Speed; highlight required so delete target is clear |
| Move to other day | Not in editor — editor is day-scoped | “Why write today if moving?” |

---

## 7. Reminders and notifications

| Decision | Detail | Why |
|----------|--------|-----|
| Default | Reminder **off** | Optional |
| Lead | Default **5** minutes; presets 5/10/15/30; custom minutes allowed | Fast presets + flexibility |
| Delivery | Windows toast APIs; respect Focus Assist / DND | OS-native |
| Toast click | Compact notification view (smaller than editor) | Glance, not full edit by default |
| Missed while off | **Silent skip** — no catch-up storm | Simple |
| Multiple due | **Separate** toasts; no merging | Less logic |
| Untimed + reminder | Notify at **local midnight of that day minus lead minutes** | Documented default so behavior is testable |

---

## 8. Settings

| Decision | Detail | Why |
|----------|--------|-----|
| Entry points | Main UI Settings button **and** tray Settings | Same app |
| Tabs | Calendar · General · Appearance · About/Data | Room for stubs without cluttering main UI |
| Startup with Windows | Optional; **default off** | User choice |
| Theme | Dark only shipped; hooks may exist for later | Skill picks dark palette; light later |
| Window bounds | Remembered; minimum **900×600** | Resizable; not fixed; readable floor |

---

## 9. Explicit non-goals (v1)

- Past-month archive / history  
- 12th future month  
- Accounts, sync, sharing  
- SQLite / AppData-primary store  
- Custom in-app emoji catalog  
- Timeline conflict UI  
- Localization / multi-TZ  
- GitHub CI  
- Generic right-click context menu on the main window  

---

## 10. Amendment rule

Changing any row above requires updating this file **and** the related Spec Kit artifact (`spec.md` / `data-model.md` / constitution) in the same change set, with a one-line “why” note.
