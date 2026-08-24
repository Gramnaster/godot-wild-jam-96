using Godot;

namespace GodotWildJam96;

public static class NearestTarget
{
    // Returns -1 for an empty set, so "no targets" is a value the caller
    // must handle rather than a null waiting to be dereferenced.
    public static int IndexOfNearest(Vector2 from, Vector2[] positions)
    {
        int closestIndex = -1;
        float closestDistSquared = float.MaxValue;

        for (int i = 0; i < positions.Length; i++)
        {
            float distSquared = from.DistanceSquaredTo(positions[i]);
            if (distSquared < closestDistSquared)
            {
                closestDistSquared = distSquared;
                closestIndex = i;
            }
        }

        return closestIndex;
    }
}
