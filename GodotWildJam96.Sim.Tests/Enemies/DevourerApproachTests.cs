using System.Numerics;
using GodotWildJam96.Sim;
using Xunit;

namespace GodotWildJam96.Sim.Tests;

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
