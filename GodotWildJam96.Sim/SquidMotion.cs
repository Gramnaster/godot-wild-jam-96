using System;
using System.Numerics;

namespace GodotWildJam96.Sim;

// A squid's chase: it always turns to face the player, but only closes distance
// in short eased bursts separated by coasts, driven by an external move timer.
public sealed class SquidMotion(float speed, float turnSpeed)
{
    public const float BiteRange = 150f;

    // How long one thrust burst eases in over.
    private const float ThrustDurationSeconds = 0.15f;

    // Per-second fraction of velocity shed while coasting.
    private const float CoastDamping = 2.5f;

    private double _thrustTimer = 1.8f;

    public float Speed { get; set; } = speed;
    public float TurnSpeed { get; set; } = turnSpeed;

    public SquidMoveState State { get; private set; } = SquidMoveState.Waiting;
    // Settable so the bridge can reload the body's post-collision velocity each
    // frame; coasting decays whatever the physics engine last left behind.
    public Vector2 Velocity { get; set; }
    public Vector2 Direction { get; private set; }
    public float Rotation { get; set; }

    // Drives the mouth animation in the bridge.
    public bool IsInBiteRange { get; private set; }

    public void Tick(Vector2 selfPosition, Vector2 playerPosition, float deltaSeconds)
    {
        Vector2 toPlayer = playerPosition - selfPosition;
        Direction = toPlayer.Normalized();
        Rotation = SimMath.RotateToward(Rotation, Direction.Angle(), TurnSpeed * deltaSeconds);
        IsInBiteRange = toPlayer.Length() <= BiteRange;

        switch (State)
        {
            case SquidMoveState.Waiting:
                Velocity = Vector2.Zero;
                break;
            case SquidMoveState.Thrusting:
                _thrustTimer -= deltaSeconds;
                float remaining = 1f - MathF.Max((float)_thrustTimer / ThrustDurationSeconds, 0f);
                float eased = 1f - MathF.Pow(1f - remaining, 3f);
                Velocity = Direction * Speed * eased;
                break;
            case SquidMoveState.Coasting:
                Velocity -= Velocity * CoastDamping * deltaSeconds;
                break;
        }
    }

    // Waiting -> Thrusting -> Coasting -> Waiting, one step per move-timer tick.
    public SquidMoveState AdvanceMoveState()
    {
        State = State switch
        {
            SquidMoveState.Waiting => SquidMoveState.Thrusting,
            SquidMoveState.Thrusting => SquidMoveState.Coasting,
            _ => SquidMoveState.Waiting,
        };

        if (State == SquidMoveState.Thrusting)
        {
            _thrustTimer = ThrustDurationSeconds;
        }

        return State;
    }
}
