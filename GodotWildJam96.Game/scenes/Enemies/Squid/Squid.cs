using Godot;
using GodotWildJam96.Sim;

namespace GodotWildJam96;

public sealed partial class Squid : EnemyBase
{
    private const float BiteRange = 150f;

    [Export] private AnimatedSprite2D _squidMouthSprite;
    [Export] private Timer _moveTimer;
    private double _thrustTimer = 1.8f;
    private Vector2 _direction;
    private SquidMoveState _moveState = SquidMoveState.Waiting;

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

    private void ChooseDirection(float dt)
    {
        Vector2 toPlayer = PlayerRef.GlobalPosition - GlobalPosition;
        _direction = toPlayer.Normalized();
        Rotation = Mathf.RotateToward(Rotation, _direction.Angle(), TurnSpeed * dt);

        _squidMouthSprite.Play(toPlayer.Length() <= BiteRange ? "Biting" : "Idle");
    }


    private void MoveTo(float dt)
    {
        ChooseDirection(dt);
        switch (_moveState)
        {
            case SquidMoveState.Waiting:
                Velocity = Vector2.Zero;
                break;
            case SquidMoveState.Thrusting:
                _thrustTimer -= dt;
                float t = 1f - Mathf.Max((float)_thrustTimer / 0.15f, 0f);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                Velocity = _direction * Speed * eased;
                break;
            case SquidMoveState.Coasting:
                Velocity -= Velocity * 2.5f * dt;
                break;
        }
    }

    private void MoveCalled()
    {
        switch (_moveState)
        {
            case SquidMoveState.Waiting:
                _moveState = SquidMoveState.Thrusting;
                _thrustTimer = 0.15f;
                AnimateSprite.Play();
                break;
            case SquidMoveState.Thrusting:
                _moveState = SquidMoveState.Coasting;
                AnimateSprite.Stop();
                break;
            case SquidMoveState.Coasting:
                _moveState = SquidMoveState.Waiting;
                break;
        }
    }
}
