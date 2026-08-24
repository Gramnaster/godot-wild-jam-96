using System;
using Godot;
using GodotWildJam96;
using Xunit;

namespace GodotWildJam96.Tests;

public class SpawnPlacementTests
{
    [Fact]
    public void RandomSunPosition_SameSeed_IsDeterministic()
    {
        Vector2 first = SpawnPlacement.RandomSunPosition(new Random(42));
        Vector2 second = SpawnPlacement.RandomSunPosition(new Random(42));

        Assert.Equal(first, second);
    }

    // Pins the current quirk: radius is drawn from [-5000, 5000], not [0, 5000],
    // so a negative draw mirrors the angle instead of pushing the point outward.
    // This is not area-uniform (suns cluster toward the origin). Not a bug fix.
    [Fact]
    public void RandomSunPosition_MagnitudeNeverExceeds5000()
    {
        for (int seed = 0; seed < 200; seed++)
        {
            Vector2 result = SpawnPlacement.RandomSunPosition(new Random(seed));

            Assert.True(result.Length() <= 5000f, $"seed {seed} produced length {result.Length()}");
        }
    }

    [Fact]
    public void OffscreenOffset_SameSeed_IsDeterministic()
    {
        Vector2 halfExtent = new(480f, 360f);

        Vector2 first = SpawnPlacement.OffscreenOffset(new Random(7), halfExtent);
        Vector2 second = SpawnPlacement.OffscreenOffset(new Random(7), halfExtent);

        Assert.Equal(first, second);
    }

    // The result must sit outside the halfExtent rectangle on whichever axis it
    // was pushed along, by at least the minimum buffer (50).
    [Fact]
    public void OffscreenOffset_AlwaysClearsHalfExtentByMinimumBuffer()
    {
        Vector2 halfExtent = new(480f, 360f);

        for (int seed = 0; seed < 200; seed++)
        {
            Vector2 result = SpawnPlacement.OffscreenOffset(new Random(seed), halfExtent);

            bool clearsX = Mathf.Abs(result.X) >= halfExtent.X + 50f;
            bool clearsY = Mathf.Abs(result.Y) >= halfExtent.Y + 50f;
            Assert.True(clearsX || clearsY, $"seed {seed} produced {result} inside the buffer");
        }
    }
}
