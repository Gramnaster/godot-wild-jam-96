using System;
using System.Numerics;

namespace GodotWildJam96.Sim;

// System.Numerics.Vector2 lacks the conveniences Godot's Vector2/Mathf provide,
// and two of them differ in behavior rather than just spelling:
// Vector2.Normalize returns NaN for a zero vector where Godot returns Zero, and
// Godot's angle helpers wrap at Tau. These reimplement Godot 4.7's semantics
// exactly so the migrated simulation math is bit-for-bit what shipped.
public static class SimMath
{
    public static Vector2 FromAngle(float radians) =>
        new(MathF.Cos(radians), MathF.Sin(radians));

    public static float Angle(this Vector2 v) => MathF.Atan2(v.Y, v.X);

    // Godot returns Zero for a zero-length vector; System.Numerics returns NaN.
    public static Vector2 Normalized(this Vector2 v)
    {
        float lengthSquared = v.LengthSquared();
        if (lengthSquared == 0f) return Vector2.Zero;
        return v / MathF.Sqrt(lengthSquared);
    }

    public static Vector2 DirectionTo(this Vector2 from, Vector2 to) => (to - from).Normalized();

    public static Vector2 LimitLength(this Vector2 v, float maxLength)
    {
        float length = v.Length();
        if (length > 0f && maxLength < length)
        {
            return v / length * maxLength;
        }

        return v;
    }

    public static float AngleDifference(float from, float to)
    {
        float difference = (to - from) % MathF.Tau;
        return ((2.0f * difference) % MathF.Tau) - difference;
    }

    public static float RotateToward(float from, float to, float delta)
    {
        float difference = AngleDifference(from, to);
        float absDifference = MathF.Abs(difference);
        // A negative delta moves no further than PI radians away from `to`,
        // PI being the maximum possible angular distance.
        return from + (Math.Clamp(delta, absDifference - MathF.PI, absDifference)
            * (difference >= 0.0f ? 1.0f : -1.0f));
    }

    // Godot's Mathf.Lerp is unclamped.
    public static float Lerp(float from, float to, float weight) => from + ((to - from) * weight);
}
