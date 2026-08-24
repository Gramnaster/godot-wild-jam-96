# Solar Defense

Working title recovered from the Windows export path (`SolarDefense.exe`
in `export_presets.cfg`) — the Godot project itself is still named
`GodotWildJam-96` (`config/name` in `project.godot`), submitted for Godot
Wild Jam 96. 2D top-down arcade space-action: the player pilots a ship
around a starfield, siphoning energy from suns to keep a 6-level energy
pool charged while dodging and shooting `Squid` and `Devourer` enemies —
hitting 0 energy levels ends the run (`GameOverScreen.tscn`). Single-player,
no networking, built by a 2-contributor jam team. Optimize for finishing a
short-lived codebase, not for scaling one.

## Versions (do not assume newer)

- Godot **4.7** (`config/features` in `project.godot`), pinned at 4.7.1 per
  `.claude/knowledge/decisions/004-godot-version-pin-4.7.1.md`.
- `Godot.NET.Sdk/4.7.1`, `TargetFramework net8.0`, with an unused
  `net9.0` override for `GodotTargetPlatform=android` (no Android export
  preset exists in `export_presets.cfg`).
- No `<Nullable>` or `<LangVersion>` set in the Game project — nullable
  reference types are off by default. The separate `GodotWildJam96.Sim`
  project has `<Nullable>enable</Nullable>`, but see Architecture below —
  it holds no hand-written code.
- `TreatWarningsAsErrors`, `AnalysisLevel=latest`, and
  `EnforceCodeStyleInBuild` are all on, backed by six analyzer packages
  (Roslynator, SonarAnalyzer.CSharp, Meziantou.Analyzer,
  ErrorProne.NET.CoreAnalyzers) — see
  `.claude/knowledge/decisions/006-nasa-power-of-ten-adapted-for-godot-csharp.md`.
  A warning fails the build; any new suppression needs a stated
  Godot-specific reason in `.editorconfig`, matching the existing ones
  there (S125, S1075, IDE0044, IDE0060, MA0004/0011/0046, RCS1163/1169).

## Build

```
dotnet build "GodotWildJam-96.sln"
```

```
dotnet test "GodotWildJam96.Tests/GodotWildJam96.Tests.csproj"
```

`GodotWildJam96.Tests` is a plain xUnit project (no Godot test runner) that
references `GodotWildJam96.Game` directly and covers engine-free logic only
— see
`.claude/knowledge/decisions/007-unit-test-harness-scoped-to-pure-logic.md`
for why, and `.claude/knowledge/refactor-roadmap.md` for what it's expected
to cover next. Anything touching the scene tree or `GD.*` still has no
automated coverage. Verification bar: a clean `dotnet build`, `dotnet test`
green, then a manual play pass in the editor.

## Architecture

- Core loop is reactive/event-driven, not a ticking background sim — 15 of
  25 hand-written scripts override `_PhysicsProcess`/`_Process` directly on
  their Godot node.
- No sim-view split. `GodotWildJam96.Sim` exists as a second,
  Godot-reference-free class library — scaffolded for exactly that
  separation — but holds zero hand-written source; every glob hit under it
  is generator output in `obj/`. All gameplay logic lives directly in node
  scripts under `GodotWildJam96.Game`. Don't route new logic through it
  without a concrete need (threading, headless unit tests, networking) —
  see `.claude/rules/doctrine.md` on this being earned, not default.
- One global event bus, `globals/EventBus.cs`, registered as the
  `EventBus` autoload: a self-assigned `static Instance`, one plain C#
  `event Action<T>` per event, and `EmitOnX` helpers that
  null-conditionally invoke. Zero `[Signal]` declarations, zero
  editor-wired `[connection]` blocks in any `.tscn` — all wiring is `+=`/
  `-=` in code.
- Entity variants that share behavior use C# inheritance with `protected
  virtual` hooks: `EnemyBase` → `Squid`, `Devourer`; `Sun` → `MainSun`.
  Scene composition isn't the pattern for variants here.
- No authority model, no host/client split, no fixed-point math —
  single-player, nothing to keep in sync across machines.

## Conventions

- Node references are `[Export]` fields wired in the editor — every
  `.tscn` with a wired node path uses `node_paths=PackedStringArray(...)`,
  and there are zero `GetNode<T>()`/`%UniqueName` lookups in the codebase.
  Don't introduce one.
- Namespaces are flat: every file declares `namespace GodotWildJam96;`
  (file-scoped) — enforced by the analyzer suite (MA0047/S3903 fail the
  build on a type with no namespace). `.editorconfig` explicitly sets
  `dotnet_diagnostic.IDE0130.severity = none` for this — it's deliberate
  policy, not drift.
- Public members are PascalCase properties (`{ get; set; }`, `[Export]`
  where editor-wired); private members stay `_camelCase` fields. A raw
  public field fails the build (S1104). This includes `[Export]` fields
  exposed to the Inspector — see `Sun.EnergyValuebar`, `Sun.SiphonSound`,
  `Spawner.SunScene`, etc. for the pattern.
- Style: Allman braces, 4-space indent, LF endings, 120-char lines,
  explicit types except where `var`'s type is apparent
  (`csharp_style_var_when_type_is_apparent`).
- `sealed` is applied inconsistently (3 of 25 classes) — no enforced
  project-wide rule either way; don't assume a leaf class must be sealed.

## Out of scope

- Networking = Solo. No multiplayer, no authority model, no prediction or
  rollback, no relay/transport concerns. Don't introduce multiplayer or
  fixed-point (Fix64) guidance.
- Determinism = No. Floating-point is fine everywhere. No fixed-point
  math, no lockstep, no replay hashing.
- No mod loading, no def registries, no mod-parity stamps.
- Strings are authored directly in English. No `TranslationServer` wiring
  until localization is a stated goal.
- No save-schema migration layer — this is a one-and-done jam release, not
  a live-service game.
- No Steamworks, EOS, or console SDK integration; no entitlement checks.
- No trust boundary. No save/load system exists yet either, and
  persistence, modding, and platform integration are all absent — skip
  input-validation doctrine written for networked or user-content systems.
- Prefer the smallest thing that works. Don't add abstraction layers for a
  codebase this short-lived.
- Hand-written DI and hand-written state machines are the convention (see
  `EventBus`, `protected virtual` hooks above). Don't add a DI or FSM
  package (no Chickensoft tooling is referenced in either `.csproj`).

## Where the rules live

- [`.claude/rules/doctrine.md`](.claude/rules/doctrine.md) — the
  three-level performance model, when lockstep/Fix64/host-authority
  actually apply, and why sim-view separation is earned, not default.
- [`.claude/rules/priorities.md`](.claude/rules/priorities.md) — the
  conflict tiebreaker order: correctness > simplicity > documented intent
  > readability > performance (split into free habits vs. gated tools).
- [`.claude/rules/godot-csharp-conventions.md`](.claude/rules/godot-csharp-conventions.md) —
  the per-axis idiom choices (Export vs. `GetNode`, `[Signal]` vs. `event`,
  inheritance vs. composition, namespaces) this project picked, and why.
- [`.claude/rules/performance.md`](.claude/rules/performance.md) — free
  habits vs. complexity-adding tools, plus the Godot-specific hot-path
  list.
- [`.claude/rules/skill-authoring.md`](.claude/rules/skill-authoring.md) —
  conventions for this project's own `.claude/skills/*` files.
- `.claude/knowledge/decisions/` — six ADRs recording calls already made
  (events over `[Signal]`, `[Export]` over `GetNode`, the net8.0/Godot
  version pins, the NASA Power-of-Ten adaptation).
- [`.claude/knowledge/godot-csharp-gotchas.md`](.claude/knowledge/godot-csharp-gotchas.md),
  [`multithreading-csharp-godot.md`](.claude/knowledge/multithreading-csharp-godot.md),
  [`gaming-patterns-index.md`](.claude/knowledge/gaming-patterns-index.md) —
  reference material: verified C# interop pitfalls, when threading is
  actually justified, and a problem → pattern index.

<!-- godot-init: 2026-08-24 | genre=other(arcade-action) net=solo det=no
     dim=2d hw=low-end exports=desktop(windows,macos,linux) team=small(2)
     ambition=jam live=one-and-done persist=none mod=none platform=none
     l10n=none input=kbm tooling=none -->
