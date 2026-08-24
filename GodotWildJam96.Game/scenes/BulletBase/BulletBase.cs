using System;
using Godot;
using GodotWildJam96.Sim;
using SimVector2 = System.Numerics.Vector2;

namespace GodotWildJam96;

public sealed partial class BulletBase : Area2D
{
    [Export] private AnimatedSprite2D _bulletSprite;

    [Export] private float _maxLifetimeSeconds = 1.5f;

    private ProjectileMotion _motion;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Setup runs before the node is added to the tree. A bullet placed
        // directly in a scene never gets one, and falls back to sitting still
        // until its exported lifetime runs out -- as it did before the split.
        _motion ??= new ProjectileMotion(SimVector2.Zero, _maxLifetimeSeconds);
        AreaEntered += OnAreaEntered;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _ExitTree()
    {
        AreaEntered -= OnAreaEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        Position = _motion.Advance(Position.ToSim(), (float)delta).ToGodot();

        // Determines maximum range based on projectile lifetime
        if (_motion.HasExpired)
        {
            QueueFree();
        }
    }

    // Called by ObjectMaker after instantiation, before node addded to scene tree.
    public void Setup(Vector2 position, Vector2 direction, float speed, float lifetimeSeconds)
    {
        GlobalPosition = position;
        Rotation = direction.Angle();
        _motion = new ProjectileMotion((direction * speed).ToSim(), lifetimeSeconds);
    }

    // Bullet disappears if it touches anything.
    private void OnAreaEntered(Area2D area)
    {
        // Bullet can't interact with itself
        if (area is BulletBase) return;

        QueueFree();
    }
}
