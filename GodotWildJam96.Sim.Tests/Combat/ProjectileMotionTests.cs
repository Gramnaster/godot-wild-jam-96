using System.Numerics;
using GodotWildJam96.Sim;
using Xunit;

namespace GodotWildJam96.Sim.Tests;

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
