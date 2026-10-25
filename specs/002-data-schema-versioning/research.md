# Research: Data Schema Versioning

## Decision: Integer schema `version` in each JSON document

**Rationale**: Already in MVP contract; simple for migrations; independent of assembly version.  
**Alternatives**: Single global manifest file (extra I/O / drift risk); SemVer strings (harder comparisons).

## Decision: Migrate-up in process; refuse newer schemas with backup

**Rationale**: Older app must never overwrite newer data. Backup keeps forensics; empty in-memory session lets the user still use the app.  
**Alternatives**: Crash on startup (hostile); attempt partial read (unsafe).

## Decision: Missing `version` ≡ schema 1

**Rationale**: Files already in the wild from MVP may omit careful versioning discipline; treat as current era.  
**Alternatives**: Treat as 0 requiring migration step (needless for identical shape).

## Decision: Persist `appVersion` for diagnostics only

**Rationale**: Helps support; must not gate load.  
**Alternatives**: Omit (less supportable).

## Decision: Migration pipeline via ordered steps on JsonNode

**Rationale**: Future field renames/adds implement as `FromVersion → FromVersion+1` without rewriting loaders. Current=1 means no steps yet.
