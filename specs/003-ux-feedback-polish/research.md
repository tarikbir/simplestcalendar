# Research: 003-ux-feedback-polish

## Duration model

**Decision**: Store `durationMinutes` (int?) on entries; UI exposes hours + minutes NumberBoxes.
**Rationale**: Minute-accurate midnight math; simple migration (`hours * 60`).
**Alternatives**: Keep `durationHours` + add `durationMinutes` field — rejected (two sources of truth).

## Midnight split

**Decision**: Split by remaining minutes until midnight; first end `00:00`, second start `00:00`, second end = remainder. Keep `SplitGroupId` for optional metadata only; Delete/Save operate on single `Id`.
**Rationale**: Matches user request for independent entities.

## Time input

**Decision**: WinUI `TimePicker` as primary start-time control; optional clear for untimed.
**Rationale**: Constitution “prefer OS capabilities”; removes free-text HH:mm friction.

## Month font

**Decision**: Bundle Fraunces (OFL) under `Assets/Fonts`, apply to month title only.
**Rationale**: User asked for Google Font month display; WinUI cannot load Google CDN at runtime.

## Layout growth

**Decision**: Keep `UniformGridLayout` with `ItemsStretch="Fill"`; remove fixed visual floor that prevents growth; wrap entry `ItemsControl` in `ScrollViewer`.
**Rationale**: Addresses fullscreen empty band and 4+ entry clipping.

## Quit hang

**Decision**: Persist window bounds asynchronously without `GetResult()` on UI close path; dispose tray without blocking deadlock.
**Rationale**: Sync-over-async on close is the primary hang risk.
