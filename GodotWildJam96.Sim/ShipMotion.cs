using System;
using System.Numerics;

namespace GodotWildJam96.Sim;

// One frame of player intent, already read off the input device by the bridge.
public readonly record struct ShipInput(
    float TurnAxis,
    bool ThrustForward,
    bool ThrustReverse,
    bool Brake);

// The player ship's turning, thrust and retrograde-brake rules. Owns velocity
// and rotation; the bridge hands them to MoveAndSlide and writes the
// post-collision velocity back, since collision response is the engine's job.
public sealed class ShipMotion
{
    public const float MaxLinearSpeed = 200.0f;

    // How closely the nose must point retrograde before the brake burn lights.
    private const float RetrogradeAngleTolerance = 0.05f;

    // Below this squared speed there is no meaningful retrograde to point at.
    private const float RetrogradeVelocityTolerance = 10.0f;

    // The brake burn is deliberately stronger than normal thrust.
    private const float BrakeThrustMultiplier = 1.5f;

    public ShipMotion(float turnSpeed, float thrustAcceleration, float thrustDeceleration)
    {
        TurnSpeed = turnSpeed;
        ThrustAcceleration = thrustAcceleration;
        ThrustDeceleration = thrustDeceleration;
    }

    public float TurnSpeed { get; set; }
    public float ThrustAcceleration { get; set; }
    public float ThrustDeceleration { get; set; }

    public Vector2 Velocity { get; set; }
    public float Rotation { get; set; }

    public bool IsThrusting { get; private set; }
    public bool StartsThrusting { get; private set; }
    public bool IsPowerThrusting { get; private set; }

    public Vector2 FacingDirection => SimMath.FromAngle(Rotation);

    public void Tick(ShipInput input, float deltaSeconds)
    {
        Rotation += input.TurnAxis * TurnSpeed * deltaSeconds;

        Thrust(input, deltaSeconds);
        Brake(input, deltaSeconds);

        // Limit to how fast the player goes, or they zoom off too fast.
        Velocity = Velocity.LimitLength(MaxLinearSpeed);
    }

    private void Thrust(ShipInput input, float deltaSeconds)
    {
        bool wasThrusting = IsThrusting;
        IsThrusting = input.ThrustForward;
        StartsThrusting = IsThrusting && !wasThrusting;

        if (IsThrusting)
        {
            Velocity += FacingDirection * ThrustAcceleration * deltaSeconds;
        }

        if (input.ThrustReverse)
        {
            Velocity -= FacingDirection * ThrustDeceleration * deltaSeconds;
        }
    }

    private void Brake(ShipInput input, float deltaSeconds)
    {
        IsPowerThrusting = false;

        if (!input.Brake || Velocity.LengthSquared() <= RetrogradeVelocityTolerance) return;

        // Point the nose retrograde, then burn.
        float retrogradeRotation = (-Velocity).Angle();
        Rotation = SimMath.RotateToward(Rotation, retrogradeRotation, TurnSpeed * deltaSeconds);

        // Only burn once the nose is actually pointed retrograde.
        float angleDiff = MathF.Abs(SimMath.AngleDifference(Rotation, retrogradeRotation));
        if (angleDiff >= RetrogradeAngleTolerance) return;

        IsPowerThrusting = true;
        Velocity += FacingDirection * ThrustAcceleration * deltaSeconds * BrakeThrustMultiplier;
        Velocity = Velocity.LimitLength(MaxLinearSpeed);
    }
}
