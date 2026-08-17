using System;
using Godot;
using GodotWildJam96;

namespace GodotWildJam96;

public partial class Shooter : Node2D
{
    [Export] private PackedScene _bulletScene;
    [Export] private float _speed = 5.0f;
    [Export] private float _shootDelay = 0f;
    [Export] private AudioStreamPlayer2D _shootSound;
    [Export] private Timer _shootTimer;

    private bool _canShoot = true;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _shootTimer.WaitTime = _shootDelay;
        _shootTimer.Timeout += OnShootTimerTimeout;
    }

    public void Shoot(Vector2 direction)
    {
        if (!_canShoot) return;

        EventBus.EmitOnCreateBullet(GlobalPosition, direction, _speed, _bulletScene);
        _shootSound.Play();
        _canShoot = false;
        _shootTimer.Start();
    }


    private void OnShootTimerTimeout()
    {
        _canShoot = true;
    }
}
