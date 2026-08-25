using System;
using System.Numerics;
using GodotWildJam96.Sim;
using Xunit;

namespace GodotWildJam96.Sim.Tests;

public class SquidMotionTests
{
    private static SquidMotion Squid() => new(speed: 100f, turnSpeed: MathF.Tau);

    [Fact]
    public void Waiting_HoldsStill()
    {
        SquidMotion squid = Squid();

        squid.Tick(Vector2.Zero, new Vector2(200f, 0f), 0.016f);

        Assert.Equal(Vector2.Zero, squid.Velocity);
    }

    [Fact]
    public void MoveState_CyclesWaitingThrustingCoasting()
    {
        SquidMotion squid = Squid();

        Assert.Equal(SquidMoveState.Thrusting, squid.AdvanceMoveState());
        Assert.Equal(SquidMoveState.Coasting, squid.AdvanceMoveState());
        Assert.Equal(SquidMoveState.Waiting, squid.AdvanceMoveState());
        Assert.Equal(SquidMoveState.Thrusting, squid.AdvanceMoveState());
    }

    [Fact]
    public void Thrusting_MovesTowardThePlayer()
    {
        SquidMotion squid = Squid();
        squid.AdvanceMoveState();

        squid.Tick(Vector2.Zero, new Vector2(500f, 0f), 0.016f);

        Assert.True(squid.Velocity.X > 0f);
        Assert.Equal(0f, squid.Velocity.Y, precision: 4);
    }

    // The burst eases in: it is slower at the start of the thrust than at its end.
    [Fact]
    public void Thrusting_EasesInRatherThanStartingAtFullSpeed()
    {
        SquidMotion squid = Squid();
        squid.AdvanceMoveState();

        squid.Tick(Vector2.Zero, new Vector2(500f, 0f), 0.01f);
        float early = squid.Velocity.Length();

        squid.Tick(Vector2.Zero, new Vector2(500f, 0f), 0.1f);
        float later = squid.Velocity.Length();

        Assert.True(early < later, $"expected easing, got {early} then {later}");
        Assert.True(later <= 100f);
    }

    [Fact]
    public void Coasting_DecaysVelocityTowardZero()
    {
        SquidMotion squid = Squid();
        squid.AdvanceMoveState();
        squid.Tick(Vector2.Zero, new Vector2(500f, 0f), 0.2f);
        float thrustSpeed = squid.Velocity.Length();

        squid.AdvanceMoveState();
        squid.Tick(Vector2.Zero, new Vector2(500f, 0f), 0.1f);

        Assert.True(squid.Velocity.Length() < thrustSpeed);
    }

    [Fact]
    public void BiteRange_IsInclusiveAtTheBoundary()
    {
        SquidMotion squid = Squid();

        squid.Tick(Vector2.Zero, new Vector2(SquidMotion.BiteRange, 0f), 0.016f);
        Assert.True(squid.IsInBiteRange);

        squid.Tick(Vector2.Zero, new Vector2(SquidMotion.BiteRange + 1f, 0f), 0.016f);
        Assert.False(squid.IsInBiteRange);
    }

    [Fact]
    public void Tick_TurnsTowardThePlayer()
    {
        SquidMotion squid = Squid();
        squid.Rotation = 0f;

        squid.Tick(Vector2.Zero, new Vector2(0f, 100f), 0.1f);

        // The player is at +Y, which is PI/2; the nose must have moved that way.
        Assert.True(squid.Rotation > 0f);
    }
}
