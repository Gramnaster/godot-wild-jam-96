# godot-wild-jam-96
Hello, this is our primary project for the Godot Wild Jam #96.

# Lessons to Practise - as much as we can even fit:
Top-down movement and collision
Basic score / HUD logic
Enemy spawning and pacing intuition
Signals / autoload thinking
Basic AI rhythm and boss-behavior instincts
IDamageable as the first real gameplay interface
Explicit C# state-machine structure instead of ad hoc logic
Weapon-as-child-scene composition
Mouse aim plus homing / steering math
RayCast2D line-of-sight checks

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


