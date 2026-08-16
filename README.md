# godot-wild-jam-96
Hello, this is our primary project for the Godot Wild Jam #96.

# Setup
- Install the **.NET/Mono build** of **Godot 4.7.x**.
- Install **.NET SDK 8.0** or later.
- Clone via SSH (`git@github.com:Gramnaster/godot-wild-jam-96.git`) - you'll need your own SSH key added to GitHub, or ask for an HTTPS remote instead.
- Open `GodotWildJam96.Game/project.godot` in Godot, then hit the Build (hammer icon, top-right of the editor) once. This restores the NuGet analyzer packages (Roslynator, Meziantou, ErrorProne.NET) and builds both `GodotWildJam-96` and `GodotWildJam96.Sim`.
- Build warnings are treated as errors (`TreatWarningsAsErrors`), so a clean Build here confirms your setup is good before you start writing code.

# Lessons to Practise - as much as we can even fit:
- Top-down movement and collision
- Basic score / HUD logic
- Enemy spawning and pacing intuition
- Signals / autoload thinking
- Basic AI rhythm and boss-behavior instincts
- IDamageable as the first real gameplay interface
- Explicit C# state-machine structure instead of ad hoc logic
- Weapon-as-child-scene composition
- Mouse aim plus homing / steering math
- RayCast2D line-of-sight checks

# Architecture
We'll be trying to practise the Sim-View architecture, which is going to make things harder, but we need to know exactly where we stand anyway.

## GodotWildJam96.Sim
GodotWildJam96.sim shall have no references to any Godot functions whatsoever. Its purpose is to handle all of the primary logic of the game itself. Scene scripts ideally should only contain Godot functions to update the View.

## Scenes
Scenes, separated by folder. Scripts go into the same scene folder too.

## Assets
Contains images, sounds, etc.

## Classes
Shared utility code

## Globals
Honestly should only be used for SignalHub

## Resources
Godot Resources

## Extensions
C#
C# Dev Kit
csproj Extensions
Godot Docs for C#
Godot Files
Godot Snippets for C#
Godot Tools Enhanced Sharp
Roslynator

MSBuild project tools
GitLens
EditorConfig
Error Lens



