# Contract: Portable Data Store (schema versioning)

Extends MVP paths: `<exe>/data/entries.json`, `settings.json`.

## Document envelope

```json
{
  "version": 1,
  "appVersion": "1.0.0",
  "entries": [ ]
}
```

Settings envelope same `version` / `appVersion` plus existing preference fields.

## Load rules

| Condition | Behavior |
|-----------|----------|
| Missing file | Empty list / defaults |
| Missing `version` | Treat as **1** |
| `version == Current` | Deserialize |
| `version < Current` | Run migrators to Current; atomic rewrite |
| `version > Current` | Copy to `.bak-v{N}-{timestamp}`; **do not** overwrite original; empty/defaults + warning |
| Corrupt JSON | Backup copy of original; empty/defaults + warning |

## Save rules

Always write `version: DataSchema.Current` and current `appVersion`. Atomic `.tmp` + replace (unchanged).
