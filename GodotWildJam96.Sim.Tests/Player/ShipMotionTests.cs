using System;
using System.Numerics;
using GodotWildJam96.Sim;
using Xunit;

namespace GodotWildJam96.Sim.Tests;

public class ShipMotionTests
{
    private static ShipMotion Ship() => new(turnSpeed: MathF.Tau / 2, thrustAcceleration: 100f, thrustDeceleration: 50f);

    private static readonly ShipInput Idle = new(TurnAxis: 0f, ThrustForward: false, ThrustReverse: false, Brake: false);

    [Fact]
    public void TurnAxis_RotatesAtTurnSpeed()
    {
        ShipMotion ship = Ship();

        ship.Tick(Idle with { TurnAxis = 1f }, 1f);

        Assert.Equal(MathF.Tau / 2, ship.Rotation, precision: 4);
    }

    [Fact]
    public void NoInput_LeavesVelocityUntouched()
    {
        ShipMotion ship = Ship();
        ship.Velocity = new Vector2(30f, 40f);

        ship.Tick(Idle, 1f);

        Assert.Equal(new Vector2(30f, 40f), ship.Velocity);
    }

    [Fact]
    public void ThrustForward_AcceleratesAlongFacing()
    {
        ShipMotion ship = Ship();

        ship.Tick(Idle with { ThrustForward = true }, 1f);

        // Rotation 0 faces +X.
        Assert.Equal(100f, ship.Velocity.X, precision: 3);
        Assert.Equal(0f, ship.Velocity.Y, precision: 3);
    }

    [Fact]
    public void ThrustReverse_DeceleratesAgainstFacing()
    {
        ShipMotion ship = Ship();

        ship.Tick(Idle with { ThrustReverse = true }, 1f);

        Assert.Equal(-50f, ship.Velocity.X, precision: 3);
    }

    // Forward and reverse held together net out to the difference, not a cancel.
    [Fact]
    public void ThrustForwardAndReverse_ApplyBothAccelerations()
    {
        ShipMotion ship = Ship();

        ship.Tick(Idle with { ThrustForward = true, ThrustReverse = true }, 1f);

        Assert.Equal(50f, ship.Velocity.X, precision: 3);
    }

    [Fact]
    public void Velocity_IsClampedToMaxLinearSpeed()
    {
        ShipMotion ship = Ship();

        for (int i = 0; i < 100; i++)
        {
            ship.Tick(Idle with { ThrustForward = true }, 1f);
        }

        Assert.Equal(ShipMotion.MaxLinearSpeed, ship.Velocity.Length(), precision: 3);
    }

    [Fact]
    public void StartsThrusting_IsTrueOnlyOnTheFirstFrameOfAHold()
    {
        ShipMotion ship = Ship();
        ShipInput thrusting = Idle with { ThrustForward = true };

        ship.Tick(thrusting, 0.016f);
        Assert.True(ship.StartsThrusting);

        ship.Tick(thrusting, 0.016f);
        Assert.False(ship.StartsThrusting);
        Assert.True(ship.IsThrusting);
    }

    [Fact]
    public void StartsThrusting_RearmsAfterReleasing()
    {
        ShipMotion ship = Ship();
        ShipInput thrusting = Idle with { ThrustForward = true };

        ship.Tick(thrusting, 0.016f);
        ship.Tick(Idle, 0.016f);
        Assert.False(ship.IsThrusting);

        ship.Tick(thrusting, 0.016f);
        Assert.True(ship.StartsThrusting);
    }

    // Below the velocity tolerance there is no meaningful retrograde heading,
    // so the brake must not grab the nose and spin it.
    [Fact]
    public void Brake_BelowVelocityTolerance_DoesNothing()
    {
        ShipMotion ship = Ship();
        ship.Velocity = new Vector2(1f, 0f);
        ship.Rotation = 0f;

        ship.Tick(Idle with { Brake = true }, 0.016f);

        Assert.Equal(0f, ship.Rotation, precision: 5);
        Assert.False(ship.IsPowerThrusting);
    }

    [Fact]
    public void Brake_TurnsTheNoseTowardRetrograde()
    {
        ShipMotion ship = Ship();
        ship.Velocity = new Vector2(100f, 0f);
        ship.Rotation = 0f;

        ship.Tick(Idle with { Brake = true }, 0.1f);

        // Retrograde of +X is PI; the nose must have moved off zero toward it.
        Assert.True(MathF.Abs(ship.Rotation) > 0f);
        Assert.False(ship.IsPowerThrusting);
    }

    [Fact]
    public void Brake_OnceAlignedRetrograde_LightsThePowerBurn()
    {
        ShipMotion ship = Ship();
        ship.Velocity = new Vector2(100f, 0f);
        ship.Rotation = MathF.PI;

        ship.Tick(Idle with { Brake = true }, 0.016f);

        Assert.True(ship.IsPowerThrusting);
        // Burning retrograde reduces the forward component.
        Assert.True(ship.Velocity.X < 100f);
    }

    [Fact]
    public void Brake_PowerBurnStaysWithinMaxLinearSpeed()
    {
        ShipMotion ship = Ship();
        ship.Velocity = new Vector2(ShipMotion.MaxLinearSpeed, 0f);
        ship.Rotation = 0f;

        for (int i = 0; i < 20; i++)
        {
            ship.Tick(Idle with { ThrustForward = true, Brake = true }, 0.1f);
        }

        Assert.True(ship.Velocity.Length() <= ShipMotion.MaxLinearSpeed + 0.001f);
    }

    [Fact]
    public void IsPowerThrusting_ClearsWhenBrakeIsReleased()
    {
        ShipMotion ship = Ship();
        ship.Velocity = new Vector2(100f, 0f);
        ship.Rotation = MathF.PI;

        ship.Tick(Idle with { Brake = true }, 0.016f);
        Assert.True(ship.IsPowerThrusting);

        ship.Tick(Idle, 0.016f);
        Assert.False(ship.IsPowerThrusting);
    }

    [Fact]
    public void FacingDirection_TracksRotation()
    {
        ShipMotion ship = Ship();
        ship.Rotation = MathF.PI / 2f;

        Assert.Equal(0f, ship.FacingDirection.X, precision: 4);
        Assert.Equal(1f, ship.FacingDirection.Y, precision: 4);
    }
}
