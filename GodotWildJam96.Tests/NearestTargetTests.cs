using Godot;
using GodotWildJam96;
using Xunit;

namespace GodotWildJam96.Tests;

public class NearestTargetTests
{
    [Fact]
    public void EmptySet_ReturnsNegativeOne()
    {
        int result = NearestTarget.IndexOfNearest(Vector2.Zero, []);

        Assert.Equal(-1, result);
    }

    [Fact]
    public void SingleTarget_ReturnsIndexZero()
    {
        Vector2[] positions = [new Vector2(100f, 0f)];

        int result = NearestTarget.IndexOfNearest(Vector2.Zero, positions);

        Assert.Equal(0, result);
    }

    [Fact]
    public void MultipleTargets_ReturnsGenuinelyNearest()
    {
        Vector2[] positions =
        [
            new Vector2(500f, 0f),
            new Vector2(10f, 0f),
            new Vector2(-200f, 0f),
        ];

        int result = NearestTarget.IndexOfNearest(Vector2.Zero, positions);

        Assert.Equal(1, result);
    }

    [Fact]
    public void TiedDistances_ResolveToFirstIndex()
    {
        Vector2[] positions =
        [
            new Vector2(50f, 0f),
            new Vector2(-50f, 0f),
        ];

        int result = NearestTarget.IndexOfNearest(Vector2.Zero, positions);

        Assert.Equal(0, result);
    }

    [Fact]
    public void NegativeCoordinates_AreHandledCorrectly()
    {
        Vector2[] positions =
        [
            new Vector2(-1000f, -1000f),
            new Vector2(-10f, -10f),
            new Vector2(-500f, -500f),
        ];

        int result = NearestTarget.IndexOfNearest(new Vector2(-5f, -5f), positions);

        Assert.Equal(1, result);
    }
}
