using System;
using Godot;

namespace GodotWildJam96;

public partial class Squid : EnemyBase
{
    private const float BiteRange = 150f;

    [Export] AnimatedSprite2D _squidMouthSprite;
    [Export] Timer _moveTimer;
    private double _thrustTimer = 1.8f;
    private Vector2 _direction;
    private int _moveState = 0;

    protected override AnimatedSprite2D[] FlashSprites => [AnimateSprite, _squidMouthSprite];

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        ActionTimer.OneShot = true;
        ActionTimer.WaitTime = GD.RandRange(2.0, 4.0);
        Speed = 100.0f;
        _moveTimer.Timeout += MoveCalled;
    }


    public override void _ExitTree()
    {
        base._ExitTree();
        _moveTimer.Timeout -= MoveCalled;
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        MoveTo((float)delta);
        MoveAndSlide();
    }

    private void ChooseDiretcion(float dt)
    {
        Vector2 toPlayer = PlayerRef.GlobalPosition - GlobalPosition;
        _direction = toPlayer.Normalized();
        Rotation = Mathf.RotateToward(Rotation, _direction.Angle(), TurnSpeed * dt);

        _squidMouthSprite.Play(toPlayer.Length() <= BiteRange ? "Biting" : "Idle");
    }


    private void MoveTo(float dt)
    {
        ChooseDiretcion(dt);
        //0 means Squid is waiting for the ability to move
        if (_moveState == 0)
        {
            Velocity = Vector2.Zero;
        }
        //1 means squid can move
        else if (_moveState == 1)
        {
            _thrustTimer -= dt;
            float t = 1f - Mathf.Max((float)_thrustTimer / 0.15f, 0f);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            Velocity = _direction * Speed * eased;

        }
        //1 means squid can move
        else if (_moveState == 2)
        {
            Velocity -= Velocity * 2.5f * dt;
        }
    }

    private void MoveCalled()
    {
        //can probably do switch case here
        if (_moveState == 0)
        {
            _moveState = 1;
            _thrustTimer = 0.15f;
            AnimateSprite.Play();
        }
        else if (_moveState == 1)
        {
            _moveState = 2;
            AnimateSprite.Stop();
        }
        else if (_moveState == 2)
        {
            _moveState = 0;
        }
    }
}
