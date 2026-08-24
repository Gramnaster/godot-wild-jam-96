using System;
using Godot;

namespace GodotWildJam96;

public static class SpawnPlacement
{
    // Radius drawn from [-5000, 5000] rather than [0, 5000], so a negative draw
    // mirrors the angle instead of pushing the point outward -- distribution is
    // uniform in radius, not area (suns cluster toward the origin). Both quirks
    // are pinned by SpawnPlacementTests, not corrected here.
    public static Vector2 RandomSunPosition(Random rng)
    {
        float angle = (float)(rng.NextDouble() * Mathf.Tau);
        float radius = rng.Next(-5000, 5001);
        return Vector2.FromAngle(angle) * radius;
    }

    // Picks a point just outside a halfExtent-sized rectangle, on a random edge,
    // offset by a random buffer so the point spawns out of sight instead of
    // popping in right at the rectangle's border.
    public static Vector2 OffscreenOffset(Random rng, Vector2 halfExtent)
    {
        float buffer = rng.Next(50, 201);

        if (rng.NextSingle() < 0.5f)
        {
            float x = NextRange(rng, -halfExtent.X, halfExtent.X);
            float y = halfExtent.Y + buffer;
            if (rng.NextSingle() < 0.5f) y = -y;
            return new Vector2(x, y);
        }

        float axisY = NextRange(rng, -halfExtent.Y, halfExtent.Y);
        float axisX = halfExtent.X + buffer;
        if (rng.NextSingle() < 0.5f) axisX = -axisX;
        return new Vector2(axisX, axisY);
    }

    private static float NextRange(Random rng, float min, float max)
    {
        return (float)(rng.NextDouble() * (max - min) + min);
    }
}
