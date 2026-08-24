using Godot;
using SimVector2 = System.Numerics.Vector2;

namespace GodotWildJam96;

// The one place Godot.Vector2 becomes System.Numerics.Vector2 and back.
// The simulation assembly carries no Godot reference at all (see
// GodotWildJam96.Sim.csproj), so every vector crossing the bridge converts here
// rather than each call site inventing its own struct copy.
public static class SimVec
{
    public static SimVector2 ToSim(this Vector2 v) => new(v.X, v.Y);

    public static SimVector2[] ToSim(this Vector2[] source)
    {
        var converted = new SimVector2[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            converted[i] = source[i].ToSim();
        }

        return converted;
    }

    public static Vector2 ToGodot(this SimVector2 v) => new(v.X, v.Y);
}
