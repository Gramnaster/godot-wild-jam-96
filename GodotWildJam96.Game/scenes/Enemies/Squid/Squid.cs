using System;
using Godot;

namespace GodotWildJam96;

public partial class Squid : EnemyBase
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        ActionTimer.OneShot = true;
        ActionTimer.WaitTime = GD.RandRange(2.0, 4.0);
        Speed = 50.0f;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        float dt = (float)delta;
        MoveTo(dt);
        MoveAndSlide();
    }

    private void MoveTo(float dt)
    {
        Vector2 direction = (PlayerRef.GlobalPosition - GlobalPosition).Normalized();
        Velocity = direction * Speed;

        Rotation = Mathf.RotateToward(Rotation, direction.Angle(), TurnSpeed * dt);
    }
}
