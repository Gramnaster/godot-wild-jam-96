using System.Numerics;
using GodotWildJam96.Sim;
using Xunit;

namespace GodotWildJam96.Sim.Tests;

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
