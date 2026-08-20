using System;
using Godot;
using GodotWildJam96;

namespace GodotWildJam96;

public partial class Shooter : Node2D
{
    [Export] private PackedScene _bulletScene;
    [Export] private float _speed = 500.0f;
    [Export] private float _bulletLifetimeSeconds = 1.5f;
    [Export] private float _shootDelay = 0.7f;
    [Export] private AudioStreamPlayer2D _shootSound;
    [Export] private Timer _shootTimer;

    private bool _canShoot = true;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _shootTimer.WaitTime = _shootDelay;
        _shootTimer.Timeout += OnShootTimerTimeout;
    }

    // Extension that uses default speed unless given
    public void Shoot(Vector2[] directions) => Shoot(directions, _speed, _bulletLifetimeSeconds, _shootTimer.WaitTime);

    public void Shoot(Vector2[] directions, float speed, float lifetimeSeconds, double waitTime)
    {
        if (!_canShoot) return;

        foreach (Vector2 direction in directions)
        {
            EventBus.EmitOnCreateBullet(GlobalPosition, direction, speed, lifetimeSeconds, _bulletScene);
        }

        _shootSound.Play();
        _canShoot = false;
        _shootTimer.WaitTime = waitTime;
        _shootTimer.Start();
    }

    private void OnShootTimerTimeout()
    {
        _canShoot = true;
    }
}
