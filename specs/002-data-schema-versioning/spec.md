# Feature Specification: Data Schema Versioning

**Feature Branch**: `002-data-schema-versioning`  
**Created**: 2026-10-25  
**Status**: Ready  
**Input**: Protect portable JSON data across app upgrades via schema versioning, migrate-up, and non-destructive refusal of newer schemas.

## User Scenarios & Testing

### User Story 1 - Safe load across upgrades (Priority: P1)

User upgrades Simplest Calendar. Existing `data/entries.json` and `data/settings.json` load without data loss when schema is current or older; older schemas are migrated and rewritten.

**Why this priority**: Without this, upgrades can silently mis-read or wipe personal calendar data.

**Independent Test**: Place v1 (or version-less) JSON under a temp data dir; load store; assert entries/settings intact and file rewritten at current schema with `appVersion`.

**Acceptance Scenarios**:

1. **Given** a valid schema v1 file, **When** the app loads, **Then** data is in memory and saves write `version: 1` plus `appVersion`.
2. **Given** a file missing `version`, **When** loaded, **Then** it is treated as schema 1 and used normally.
3. **Given** an older schema than `DataSchema.Current` (once migrations exist), **When** loaded, **Then** migrations run and the file is rewritten at Current.

---

### User Story 2 - Future schema does not destroy data (Priority: P1)

User opens data written by a newer app with an older build. The older build must not overwrite the newer file.

**Why this priority**: Downgrade / mixed installs must not corrupt forward-compatible data.

**Independent Test**: Write JSON with `version: 999`; load; assert original unchanged, `.bak-v999-*` exists, in-memory state is empty/defaults, warning set.

**Acceptance Scenarios**:

1. **Given** `version` greater than Current, **When** loaded, **Then** original file is left intact, a timestamped backup copy is created, store uses empty/defaults, and a load warning is set.
2. **Given** corrupt JSON, **When** loaded, **Then** same non-destructive backup + safe empty/defaults + warning (original not wiped).

---

### User Story 3 - See versions in About (Priority: P2)

User opens Settings → About / Data and sees app version and data schema version; if load warned, they see a clear message at startup.

**Why this priority**: Supportability and trust when something goes wrong.

**Independent Test**: Launch app; open About; versions visible. Seed a future-version file; restart; dialog or status explains the issue.

**Acceptance Scenarios**:

1. **Given** a normal install, **When** About opens, **Then** app version and schema version are shown with the data path.
2. **Given** a load warning, **When** main window activates, **Then** a one-shot dialog explains that data was not loaded and a backup was kept.

---

### Edge Cases

- Missing data files → empty/defaults (unchanged).
- Concurrent `.tmp` save failures → existing atomic replace behavior.
- Backup path collision → append uniqueness (timestamp already).

## Requirements

- **FR-001**: Every entries/settings document carries integer `version` (schema) on save.
- **FR-002**: Saves also write diagnostic `appVersion` string; load does not require it.
- **FR-003**: `version > Current` → backup + refuse load into empty/defaults + warning; never overwrite original.
- **FR-004**: `version < Current` → ordered migration pipeline then rewrite at Current.
- **FR-005**: Missing `version` → treat as 1.
- **FR-006**: Corrupt JSON → backup + empty/defaults + warning.
- **FR-007**: About shows app + schema versions; startup surfaces load warnings once.
- **FR-008**: Assembly/InformationalVersion pinned (start `1.0.0`).

## Success Criteria

- Core tests cover round-trip version, missing version, future version backup, corrupt file.
- `verify.ps1` still passes.
- Manual: upgrading from current files does not lose entries.
