# Data Model: Schema Versioning

## DataSchema

| Field | Notes |
|-------|--------|
| `Current` | `const int = 1` |
| App version string | From assembly informational version; injected into stores on save |

## EntriesDocument / SettingsDocument (on disk)

| Field | Type | Notes |
|-------|------|--------|
| `version` | int | Schema version; required on save |
| `appVersion` | string? | Diagnostic; optional on load |
| payload | … | Existing `entries` / settings fields unchanged for v1 |

## LoadWarning

Store-level `string?` after `LoadAsync` when backup+refuse or corrupt path taken. Aggregated on `AppServices.DataLoadWarning`.

## Backup file naming

`{originalFileName}.bak-v{version}-{yyyyMMddHHmmss}` beside the original (e.g. `entries.json.bak-v999-20261025160000`).
