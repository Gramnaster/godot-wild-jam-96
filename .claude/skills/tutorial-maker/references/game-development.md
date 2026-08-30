# Game-Development Tutorial Profile

Read this reference for any course about game programming, an engine, gameplay
architecture, content pipelines, or an engine-hosted language. It specializes
the shared tutorial workflow without making one engine or language universal.

## Specialize by profile, not by assumption

Freeze the target stack before designing code-changing lessons:

- engine and exact version;
- language, binding or scripting runtime, SDK, and target framework;
- renderer, dimensions, export platforms, and hardware constraints when they
  affect the topic;
- test framework, editor requirements, command-line tools, and asset pipeline;
- networking, determinism, persistence, modding, localization, and platform
  services only when they are in scope.

Engine and language specificity helps when it fixes a real lifecycle, API,
serialization, editor, or verification constraint. It hurts when a local
project preference is presented as a general game-development rule. Keep the
course's reusable principle visible, then state how the selected stack realizes
it.

## Trace the playable path

Before defining the sequence, trace one complete player-visible path:

```text
input or event
  -> engine lifecycle callback
  -> gameplay state/rule
  -> engine bridge or scene object
  -> animation, audio, UI, physics, or rendering
  -> observable result
```

Record who owns each state, who is allowed to mutate it, and which subsystem
controls timing. Do not infer ownership from filenames. Inspect scenes,
prefabs/resources, project settings, imported assets, autoloads/singletons,
input maps, serialized references, and editor wiring as well as source code.

For an architecture course, distinguish:

- the engine-independent rule;
- the engine integration constraint;
- the example project's chosen design;
- the scale or requirement that would justify a different design.

Do not turn a progression such as prototype -> component system -> ECS, or
mixed logic -> sim-view, into a maturity ladder. These are choices with costs
and triggers, not mandatory promotions.

## Checkpoint contract

Each cumulative lesson begins at a recoverable, successful checkpoint and ends
at another. Its execution card records:

- player-visible behavior before and after;
- source, scene/resource, asset, configuration, package, and generated-file
  deltas;
- every new symbol or editor object and the lesson that introduces it;
- exact editor actions that cannot be represented by source alone;
- commands and manual observations that prove the new behavior;
- deliberate omissions and the later lesson that owns them.

Repository tip may contain future scenes, scripts, exports, assets, or broken
experiments. Compare it with the lesson's before-state. Never ask the reader to
use a node, resource, input action, autoload, package, project setting, scene,
or helper that no completed prerequisite introduced.

Generated engine caches, build artifacts, imported output, and local editor
state need an explicit policy. Record the actual repository's ignore rules and
give teammates the restore/import/build/run steps needed after a clean clone.
Do not copy a generic ignore list without checking the selected engine and
version.

## Default game-dev lesson shape

Adapt to the publication format, but preserve this learning order for a
code-changing lesson:

1. **Playable target** — the exact behavior the reader will observe.
2. **Starting checkpoint** — required prior scene, code, assets, and settings.
3. **One mental model** — the lifecycle, ownership, data flow, or math needed
   for this change.
4. **Implementation delta** — focused code and data changes in dependency order.
5. **Engine/editor wiring** — scene tree, serialized fields, input actions,
   resources, imports, or project settings.
6. **Smallest useful proof** — verify pure rules first where possible.
7. **Playable proof** — run the actual engine path and state expected evidence.
8. **Diagnostic case** — symptom, likely cause, inspection point, and fix for
   the failure readers can now create.
9. **Decision boundary** — why this design fits and what constraint would
   change it.
10. **Transfer exercise** — change one gameplay constraint without copying the
    completed code mechanically.

Conceptual lessons may replace implementation and wiring with a trace,
classification exercise, or comparison of live alternatives. They still need
an observable checked conclusion.

## Verification ladder

Use the lowest layer capable of proving each claim, then include the engine path
when the claim crosses into it:

1. pure unit test for engine-independent rules;
2. project build and analyzer/compiler checks;
3. engine or scene-level automated test when the project genuinely supports it;
4. editor/runtime play pass for lifecycle, physics, input, animation, audio,
   rendering, UI, and serialized wiring;
5. clean-clone/import check for setup and generated artifacts;
6. target-platform export or device test for platform-specific claims;
7. multi-process, latency, reconnect, or authority test for networking claims.

A green unit test cannot prove editor wiring. A successful build cannot prove
frame-order behavior. An editor play pass on one machine cannot prove export or
device behavior. State what each check proves and what remains manual.

## Godot C# profile

Activate this subsection only when Godot C# is the selected stack.

- Pin the Godot version, Godot .NET SDK, target framework, and renderer or
  export platform when relevant. Do not silently substitute a newer API.
- Verify C# names, signatures, attributes, events/signals, lifecycle overrides,
  and return types against the applicable Godot C# documentation or bindings.
  A GDScript example is conceptual evidence, not proof that pasted C# compiles.
- Treat `.cs`, `.tscn`, `.tres`, project settings, input actions, autoloads,
  exported Inspector assignments, and imported assets as one implementation
  surface. List every part the lesson changes.
- State whether the engine, a Node, a Resource, or a pure C# object owns each
  gameplay value. Keep engine-independent rules testable when the course's
  architecture calls for that boundary; do not impose the boundary on every
  Godot tutorial.
- Teach `_Process`, `_PhysicsProcess`, input callbacks, physics integration,
  rendering, and scene lifecycle with their actual cadence and ownership. Do
  not describe render FPS, physics ticks, and a custom simulation clock as the
  same thing.
- Verify pure C# logic with the project's test runner, compile the actual Godot
  solution/project, then play the relevant scene. Include an export or device
  check only for claims that depend on it.
- Give exact editor steps only where source files cannot establish the state,
  and provide a source-control check when serialized scene/resource changes are
  expected.

Project doctrines such as sim-view separation, event-bus style, exported node
references, composition versus inheritance, or a namespace layout remain local
contracts unless the lesson explicitly teaches their adoption criteria and
tradeoffs.
