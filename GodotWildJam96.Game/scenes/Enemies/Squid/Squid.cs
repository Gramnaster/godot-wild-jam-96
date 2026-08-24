using Godot;
using GodotWildJam96.Sim;

namespace GodotWildJam96;

public sealed partial class Squid : EnemyBase
{
    [Export] private AnimatedSprite2D _squidMouthSprite;
    [Export] private Timer _moveTimer;

    private SquidMotion _motion;

    protected override AnimatedSprite2D[] FlashSprites => [AnimateSprite, _squidMouthSprite];

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        ActionTimer.OneShot = true;
        ActionTimer.WaitTime = GD.RandRange(2.0, 4.0);
        Speed = 100.0f;
        _motion = new SquidMotion(Speed, TurnSpeed);
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
        // The body owns the transform, so load it into the simulation each frame
        // rather than letting the simulation keep a copy that drifts from it.
        _motion.Rotation = Rotation;
        _motion.Velocity = Velocity.ToSim();

        _motion.Tick(GlobalPosition.ToSim(), PlayerRef.GlobalPosition.ToSim(), (float)delta);

        Rotation = _motion.Rotation;
        Velocity = _motion.Velocity.ToGodot();
        _squidMouthSprite.Play(_motion.IsInBiteRange ? "Biting" : "Idle");

        MoveAndSlide();
    }

    private void MoveCalled()
    {
        switch (_motion.AdvanceMoveState())
        {
            case SquidMoveState.Thrusting:
                AnimateSprite.Play();
                break;
            case SquidMoveState.Coasting:
                AnimateSprite.Stop();
                break;
            case SquidMoveState.Waiting:
                break;
        }
    }
}
