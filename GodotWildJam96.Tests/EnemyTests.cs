using System;
using System.Numerics;
using GodotWildJam96.Sim;
using Xunit;

namespace GodotWildJam96.Tests;

public class EnemyVitalsTests
{
    [Fact]
    public void FirstHitOfThree_Survives()
    {
        var vitals = new EnemyVitals(3);

        Assert.Equal(EnemyHitResult.Survived, vitals.TakeHit());
        Assert.Equal(2, vitals.Lives);
        Assert.False(vitals.IsDead);
    }

    [Fact]
    public void HitThatEmptiesLives_Kills()
    {
        var vitals = new EnemyVitals(1);

        Assert.Equal(EnemyHitResult.Killed, vitals.TakeHit());
        Assert.True(vitals.IsDead);
    }

    // Death effects fire exactly once even though a bullet already in flight can
    // still land. Lives keeps decrementing, matching the shipped code where only
    // Die() was guarded.
    [Fact]
    public void HitAfterDeath_ReportsAlreadyDead()
    {
        var vitals = new EnemyVitals(1);
        vitals.TakeHit();

        Assert.Equal(EnemyHitResult.AlreadyDead, vitals.TakeHit());
        Assert.Equal(-1, vitals.Lives);
    }

    [Fact]
    public void EnemyStartingWithNoLives_DiesOnItsFirstHit()
    {
        var vitals = new EnemyVitals(0);

        Assert.Equal(EnemyHitResult.Killed, vitals.TakeHit());
    }

    [Fact]
    public void KnockbackOffset_PushesDirectlyAwayFromTheOrigin()
    {
        Vector2 offset = EnemyVitals.KnockbackOffset(new Vector2(100f, 0f));

        Assert.Equal(6f, offset.X, precision: 4);
        Assert.Equal(0f, offset.Y, precision: 4);
    }

    // An enemy sitting exactly on the origin has no direction to be pushed in.
    // Godot's Normalized returns Zero here; System.Numerics would return NaN.
    [Fact]
    public void KnockbackOffset_AtTheOrigin_IsZeroNotNaN()
    {
        Vector2 offset = EnemyVitals.KnockbackOffset(Vector2.Zero);

        Assert.Equal(Vector2.Zero, offset);
    }
}

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

public class DevourerApproachTests
{
    [Fact]
    public void WithinSiphonRange_IsInclusiveAtTheBoundary()
    {
        Assert.True(DevourerApproach.IsWithinSiphonRange(Vector2.Zero, new Vector2(DevourerApproach.SiphonRange, 0f)));
        Assert.False(DevourerApproach.IsWithinSiphonRange(Vector2.Zero, new Vector2(DevourerApproach.SiphonRange + 1f, 0f)));
    }

    [Fact]
    public void VelocityToward_PointsAtTheSunAtMoveSpeed()
    {
        Vector2 velocity = DevourerApproach.VelocityToward(Vector2.Zero, new Vector2(0f, 400f));

        Assert.Equal(0f, velocity.X, precision: 4);
        Assert.Equal(DevourerApproach.MoveSpeed, velocity.Y, precision: 4);
    }

    [Fact]
    public void VelocityToward_ASunItIsSittingOn_IsZeroNotNaN()
    {
        Vector2 velocity = DevourerApproach.VelocityToward(Vector2.Zero, Vector2.Zero);

        Assert.Equal(Vector2.Zero, velocity);
    }

    [Fact]
    public void BeginSiphon_LatchesTheSiphoningFlag()
    {
        var approach = new DevourerApproach();
        Assert.False(approach.IsSiphoning);

        approach.BeginSiphon();

        Assert.True(approach.IsSiphoning);
    }
}

public class ProjectileMotionTests
{
    [Fact]
    public void Advance_MovesAlongVelocity()
    {
        var bullet = new ProjectileMotion(new Vector2(100f, 0f), 1f);

        Vector2 position = bullet.Advance(Vector2.Zero, 0.5f);

        Assert.Equal(50f, position.X, precision: 4);
    }

    [Fact]
    public void NewProjectile_HasNotExpired()
    {
        var bullet = new ProjectileMotion(new Vector2(100f, 0f), 1f);

        Assert.False(bullet.HasExpired);
    }

    [Fact]
    public void Expires_OnceLifetimeIsReached()
    {
        var bullet = new ProjectileMotion(new Vector2(100f, 0f), 0.5f);

        bullet.Advance(Vector2.Zero, 0.5f);

        Assert.True(bullet.HasExpired);
    }

    // Lifetime, not distance, is what bounds a shot -- so a faster bullet with the
    // same lifetime simply travels further.
    [Fact]
    public void Range_IsSpeedTimesLifetime()
    {
        var slow = new ProjectileMotion(new Vector2(100f, 0f), 1f);
        var fast = new ProjectileMotion(new Vector2(400f, 0f), 1f);

        Vector2 slowEnd = slow.Advance(Vector2.Zero, 1f);
        Vector2 fastEnd = fast.Advance(Vector2.Zero, 1f);

        Assert.Equal(100f, slowEnd.X, precision: 4);
        Assert.Equal(400f, fastEnd.X, precision: 4);
    }
}
