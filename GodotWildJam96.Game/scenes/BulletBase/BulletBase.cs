using System;
using Godot;

public partial class BulletBase : Area2D
{
    [Export] private AnimatedSprite2D _bulletSprite;

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
    }

    // Called by ObjectMaker after instantiation, before node addded to scene tree.
    public void Setup(Vector2 position, Vector2 direction, float speed)
    {
        GlobalPosition = position;
        _velocity = direction * speed;
    }

    // Bullet disappears if it touches anything.
    private void OnAreaEntered(Area2D area)
    {
        QueueFree();
    }
}
