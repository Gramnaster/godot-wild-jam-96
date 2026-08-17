using System;
using Godot;

namespace GodotWildJam96;

public partial class BulletBase : Area2D
{
    [Export] private AnimatedSprite2D _bulletSprite;

    [Export] private float _maxLifetimeSeconds = 1.5f;
    private float _elapsedSeconds;

    private Vector2 _velocity;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _ExitTree()
    {
        AreaEntered -= OnAreaEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        Position += _velocity * (float)delta;

        // Determines maximum range based on projectile lifetime
        _elapsedSeconds += (float)delta;
        if (_elapsedSeconds >= _maxLifetimeSeconds)
        {
            QueueFree();
        }
    }

    // Called by ObjectMaker after instantiation, before node addded to scene tree.
    public void Setup(Vector2 position, Vector2 direction, float speed, float lifetimeSeconds)
    {
        GlobalPosition = position;
        _velocity = direction * speed;
        Rotation = direction.Angle();
        _maxLifetimeSeconds = lifetimeSeconds;
    }

    // Bullet disappears if it touches anything.
    private void OnAreaEntered(Area2D area)
    {
        // Bullet can't interact with itself
        if (area is BulletBase) return;

        QueueFree();
    }
}
