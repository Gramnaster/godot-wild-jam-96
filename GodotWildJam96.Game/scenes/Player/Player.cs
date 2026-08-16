using System;
using System.Xml;
using Godot;

namespace GodotWildJam96;

public partial class Player : CharacterBody2D
{
    public const float SHIP_MOVESPEED = 300.0f;

    // Radians per scond. Tau is one full revolution per second.
    [Export] public float TurnSpeed { get; set; } = Mathf.Tau;

    public Vector2 shipVelocity = new Vector2();

    private float _targetRotation;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("shoot"))
        {
            GD.Print("Shoot");
        }

        if (@event.IsActionPressed("switch_weapon_left"))
        {
            GD.Print("Switch Weapon Left");
        }

        if (@event.IsActionPressed("switch_weapon_right"))
        {
            GD.Print("Switch Weapon Right");
        }
    }

    public override void _Ready()
    {
        // Set initial _rotation for use in GetInput
        _targetRotation = Rotation;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Converted to float since a lot of methods need deltaTime in float
        float dt = (float)delta;
        GetInput(dt);
        MoveAndSlide();
    }

    public void GetInput(float dt)
    {
        Vector2 shipVelocity = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        Velocity = shipVelocity * SHIP_MOVESPEED;

        // Gives us where ship should be pointing
        if (shipVelocity != Vector2.Zero)
        {
            _targetRotation = shipVelocity.Angle();
        }

        // Final ship rotation is what it should be pointed towards
        Rotation = Mathf.RotateToward(Rotation, _targetRotation, TurnSpeed * dt);
    }
}
