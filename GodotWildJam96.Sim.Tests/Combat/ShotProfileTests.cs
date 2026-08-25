using GodotWildJam96.Sim;
using Xunit;

namespace GodotWildJam96.Sim.Tests;

public class ShotProfileTests
{
    private static readonly ShotProfile Primary = new(Speed: 450f, MinLifetimeSeconds: 0.15f, MaxLifetimeSeconds: 0.8f);

    [Fact]
    public void NoCharge_GivesTheMinimumLifetime()
    {
        Assert.Equal(0.15f, Primary.LifetimeFor(0f), precision: 5);
    }

    [Fact]
    public void FullCharge_GivesTheMaximumLifetime()
    {
        Assert.Equal(0.8f, Primary.LifetimeFor(1f), precision: 5);
    }

    [Fact]
    public void HalfCharge_InterpolatesBetweenThem()
    {
        Assert.Equal(0.475f, Primary.LifetimeFor(0.5f), precision: 5);
    }
}
