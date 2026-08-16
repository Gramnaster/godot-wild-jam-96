using Godot;
using System;

namespace GodotWildJam96;

public partial class Player : CharacterBody2D
{
	public const float SHIP_MOVESPEED = 300.0f;
    public Vector2 shipVelocity = new Vector2();

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

	public override void _PhysicsProcess(double delta)
	{
        GetInput();
		MoveAndSlide();
	}

    public void GetInput()
    {
        Vector2 shipVelocity = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        if (shipVelocity != Vector2.Zero)
        {
            shipVelocity = shipVelocity.Normalized();
        }
        Velocity = shipVelocity * SHIP_MOVESPEED;
    }
}
