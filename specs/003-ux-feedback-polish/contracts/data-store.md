# Contract: data store (schema v2)

## Root

```json
{
  "version": 2,
  "appVersion": "semver",
  "savedAt": "ISO-8601",
  "entries": [ /* Entry */ ]
}
```

## Entry (v2)

```json
{
  "id": "guid",
  "date": "yyyy-MM-dd",
  "note": "string",
  "startTime": "HH:mm" ,
  "durationMinutes": 135,
  "endTime": "HH:mm",
  "isStarred": false,
  "reminderEnabled": false,
  "reminderLeadMinutes": 5,
  "splitGroupId": "guid-or-null",
  "splitIndex": 0,
  "createdAt": "ISO-8601",
  "updatedAt": "ISO-8601"
}
```

`durationHours` MUST NOT appear after migration. Untimed entries omit `startTime` / `durationMinutes` / `endTime` as appropriate.
