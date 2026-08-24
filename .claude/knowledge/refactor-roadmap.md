# Refactor Roadmap: Solar Defense as a Teaching Codebase

## Table of Contents

- [Context](#context)
- [The seam that makes any of this possible](#the-seam-that-makes-any-of-this-possible)
- [Tier 1 — safe, mechanical, no behavior change](#tier-1--safe-mechanical-no-behavior-change)
- [Tier 2 — first real extraction: Player.FindClosestSun](#tier-2--first-real-extraction-playerfindclosestsun)
- [Tier 3 — Spawner SRP split](#tier-3--spawner-srp-split)
- [Tier 4 — Player.cs decomposition (deferred)](#tier-4--playercs-decomposition-deferred)
- [Honest pattern notes](#honest-pattern-notes)
- [Flagged, not scheduled](#flagged-not-scheduled)

## Context

This document sequences the path from "working jam submission with zero test
coverage" to "exemplar teaching codebase," ordered safest-first. Each tier
depends on the previous one landing and staying green under
`GodotWildJam96.Tests`. No tier in this document has been executed yet —
see [ADR-007](decisions/007-unit-test-harness-scoped-to-pure-logic.md) for
what *was* shipped this pass (the harness itself, zero gameplay changes).

## The seam that makes any of this possible

`GodotSharp.dll` is a plain managed .NET library containing two different
kinds of API:

- **Pure managed types** — `Vector2`, `Mathf`, `Transform2D`. No engine boot
  required; these run in a normal `dotnet test` host.
- **A P/Invoke bridge to the native engine** — `GD.Randf()`,
  `GD.RandRange()`, every `Node`/scene-tree operation. These require the
  native Godot runtime and cannot run in a plain test host.

Every tier below follows one rule because of this: **extracted logic may use
`Vector2`/`Mathf` freely, but must never call `GD.*`.** Randomness is
injected as a plain `System.Random` parameter instead — seedable
(`new Random(12345)`) for deterministic tests, no interface required (see
[Honest pattern notes](#honest-pattern-notes)).

## Tier 1 — safe, mechanical, no behavior change

No characterization tests needed first; these are refactorings a compiler
and the existing analyzer suite already fully protect.

- Remove 4 unused `using`s in
  [Spawner.cs:2-7](../../GodotWildJam96.Game/scenes/Spawner/Spawner.cs#L2-L7)
  — `System.ComponentModel.DataAnnotations`, `System.Linq`, `System.Net`,
  and `Microsoft.VisualBasic` (an IDE auto-import accident).
- Delete the self-assignment no-op at
  [Spawner.cs:138](../../GodotWildJam96.Game/scenes/Spawner/Spawner.cs#L138)
  (`_sunPos = new Vector2(_sunPos.X, _sunPos.Y);`).
- Fix the misleading comment at
  [Spawner.cs:65](../../GodotWildJam96.Game/scenes/Spawner/Spawner.cs#L65)
  claiming the sun group is assigned there — it actually happens in
  `Sun._Ready` ([Sun.cs:59](../../GodotWildJam96.Game/scenes/Sun/Sun.cs#L59)).
- Normalize [EventBus.cs](../../GodotWildJam96.Game/globals/EventBus.cs):
  the `EmitOnX` helpers are inconsistently `static`
  (`EmitOnDamageTakenPlayer`, `EmitOnTeleport`, `EmitOnAllSunsSpawn`,
  `EmitOnCreateBullet`, `EmitOnCreateExplosion`, `EmitOnDevourerEntered`,
  `EmitOnEnergySiphoned`, `EmitOnSpawnDevourers`, `EmitOnEnemySiphonStart`,
  `EmitOnEnemySiphonStop`) vs. instance (`EmitOnShipEntered`,
  `EmitOnShipExited`, `EmitOnSiphonStart`, `EmitOnPlayerSiphonEnd`,
  `EmitOnPlayerSiphonReset`) for the same kind of call. Making all of them
  static is a compiler-checked, behavior-preserving move. Also comment out
  the stray live `GD.Print` calls at lines 72, 78, 83, 99, 105, per this
  project's existing "comment, don't delete" debug-scaffolding convention.
- Collapse the near-duplicate `siphon_out`/`siphon_in` blocks in
  [Player.cs:132-163](../../GodotWildJam96.Game/scenes/Player/Player.cs#L132-L163),
  which differ only by `_siphonType`. Genuine DRY, small blast radius.

## Tier 2 — first real extraction: Player.FindClosestSun

[Player.cs:438-456](../../GodotWildJam96.Game/scenes/Player/Player.cs#L438-L456)
is the highest-value first target — better than `Spawner`, because the pure
part is trivially separable and extracting it fixes two live defects at the
same time:

- **Latent crash.** `_closestSun` stays `null` when no suns exist in the
  tree, then is dereferenced unconditionally a few lines later.
- **Per-frame allocation.** Runs every `_Process` frame doing
  `GetTree().GetNodesInGroup(GameConstants.GroupSuns).OfType<Sun>()` — a
  LINQ enumerator allocation every frame, exactly the kind of thing
  [performance.md](../rules/performance.md#free-habits--apply-by-default-not-just-on-hot-paths)
  names as a free habit to avoid regardless of profiling status.

"Given a set of positions and a player position, return the nearest" is
pure, `Vector2`-only math, needs no injected randomness, and is trivially
unit-testable including the empty-set case. Ideal first extraction — do
this one first, prove the pattern works end to end, then move to Tier 3.

## Tier 3 — Spawner SRP split

[Spawner.cs](../../GodotWildJam96.Game/scenes/Spawner/Spawner.cs) holds four
unrelated spawn policies plus a `MainSun` bootstrap in 140 lines. Per-method
purity, mapped in full:

| Method | Purity |
|---|---|
| `SunSpawnCalculator()` (L135-139) | Closest to pure — still calls `GD.RandRange` |
| `OffscreenSpawnOffset()` (L110-133) | Mixed — 2 lines coupled to `GetViewport().GetCamera2D()` / viewport size, rest is pure math |
| `SpawnSuns`, `EnsurePositionValid`, `SpawnDevourers`, `SpawnSquid`, `Trial`, `_Ready`, `_ExitTree` | Fully engine-coupled |

Plan: extract the pure placement math into free functions/a small class
taking `Random` as a parameter, leaving the engine-coupled shell —
`Instantiate`/`AddChild`/`GetWorld2D`/`GetViewport` — in the node.

> [!NOTE]
> Sequence matters and is unavoidable: the initial extraction (~15 lines) is
> **unprotected**, because there is nothing to pin until the seam exists.
> Do the mechanical move first, write characterization tests against the
> extracted method immediately after, *then* restructure further with tests
> green. Say this plainly rather than implying the first step is covered.

Characterization tests must pin **current** behavior before any correction,
including a known quirk at
[Spawner.cs:137](../../GodotWildJam96.Game/scenes/Spawner/Spawner.cs#L137):

```csharp
Vector2.FromAngle((float)GD.RandRange(0, Mathf.Tau)) * GD.RandRange(-5000, 5000)
```

`GD.RandRange(-5000, 5000)` takes `int` arguments and returns an `int`, and
multiplying by radius directly — not `sqrt`-corrected — means placement is
uniform in *radius*, not area: suns cluster toward the origin, and a
negative draw just mirrors the angle rather than pushing further out.
Whether to correct this distribution is a separate, deliberate decision to
make *after* the current behavior is pinned by a test, not folded silently
into the extraction.

## Tier 4 — Player.cs decomposition (deferred)

[Player.cs](../../GodotWildJam96.Game/scenes/Player/Player.cs) is ~480 lines
spanning input, movement, braking physics, thruster animation, charge-shot
combat, siphon interaction, damage/health, and light-radius safety — a
textbook God Object mapping to
[`E15 - Component`](../gaming-patterns-index.md). It is deliberately
sequenced last:

- It is the single most game-feel-critical file in the project — regressions
  here are the most player-visible of anything in scope.
- It has almost no unit-testable surface once the pure pieces (Tier 2) are
  removed; what's left is input handling and physics tuning that rides on
  manual verification alone, not automated tests.

Do not start this tier until Tiers 1-3 are done and green, and expect it to
lean on the manual-play verification bar stated in
[CLAUDE.md](../../CLAUDE.md) rather than test coverage.

## Honest pattern notes

The teaching value of this roadmap is as much in what it *doesn't* recommend
as what it does:

- The Tier 3 split is **Extract Class + constructor injection**, not
  Strategy. Strategy implies runtime-swappable variants; `Spawner`'s four
  spawn kinds are never swapped at runtime. Naming it Strategy would be
  cargo-culting a pattern name onto code that doesn't have the shape the
  pattern describes.
- Randomness is injected as a plain `System.Random` parameter, not an
  `IRandomSource` interface with exactly one implementation —
  [priorities.md](../rules/priorities.md) rejects that abstraction outright
  (no second implementation, no polymorphism need). Seeding
  (`new Random(12345)` vs. `new Random()`) already gives deterministic
  tests. **Dependency inversion does not mean "always add an interface."**
- An Open/Closed registry or factory for enemy spawning (so a new enemy type
  needs no `Spawner` edit) is **not recommended**. Two enemy types
  (`Squid`, `Devourer`) in a finished, submitted jam game is speculative
  generality — OCP is earned by observed change frequency, and this project
  has none forecast.
- [EnemyBase.cs](../../GodotWildJam96.Game/scenes/Enemies/EnemyBase/EnemyBase.cs)
  is already a good Template Method example: `protected virtual` hooks
  (`OnTimeout`, `OnScreenEntered`, `OnHitBoxBodyEntered`, `OnHitBoxAreaEntered`,
  `OnSunsReady`), plus real encapsulation via read-only
  `AnimateSprite`/`ActionTimer` properties that expose owned nodes to
  subclasses without allowing replacement. Cite it as the reference example
  in this codebase — it does not need "improving" to make the point.

## Flagged, not scheduled

Found while mapping the codebase for this roadmap. Listed for a deliberate
decision, not silently fixed as part of any tier above:

- `_interruptDamage`
  ([Player.cs:86](../../GodotWildJam96.Game/scenes/Player/Player.cs#L86))
  is declared but never assigned anywhere, so it is always `0.0f`. Since
  `TakeDamage` checks `if (dmg > _interruptDamage)`, every nonzero hit
  interrupts an active siphon. This may be the intended feel, but as
  written it's accidental — there's no code path that sets a real
  threshold.
- `Stolenpower`
  ([EnemyBase.cs:20](../../GodotWildJam96.Game/scenes/Enemies/EnemyBase/EnemyBase.cs#L20))
  should be `StolenPower` by this project's naming convention. It's
  `[Export]`, so a rename needs matching `.tscn` export-key edits, not just
  a C# rename.
- `EnemyBase`'s `protected` members are raw PascalCase fields, not
  properties. S1104 (the analyzer rule that forced 16 `public` fields to
  properties this session) only flags `public` members, so these were never
  caught. The convention for `protected` fields is currently undecided in
  this codebase — worth a stated decision one way or the other rather than
  leaving it ambiguous.
- `SPAWN_ATTEMPTS`
  ([Spawner.cs:50](../../GodotWildJam96.Game/scenes/Spawner/Spawner.cs#L50))
  is a SCREAMING_CASE local variable, inconsistent with the project's
  `_camelCase` private-field / PascalCase-property convention.

---

Version: 1.0 — 2026-08-24