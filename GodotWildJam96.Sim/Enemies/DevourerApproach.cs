using System.Numerics;

namespace GodotWildJam96.Sim;

// A devourer cruises to the nearest sun and starts feeding once it is close
// enough. The interaction area's own radius is the shared player light radius,
// far wider than the devourer's actual reach, so range is checked here rather
// than being taken from the physics overlap. Facing turns at TurnSpeed instead
// of snapping -- the same turn-rate-limited approach SquidMotion uses -- while
// movement still points straight at the target.
public sealed class DevourerApproach(float turnSpeed = 0f)
{
    public const float MoveSpeed = 50.0f;
    public const float SiphonRange = 60.0f;

    public bool IsSiphoning { get; private set; }
    public float TurnSpeed { get; set; } = turnSpeed;
    public float Rotation { get; set; }

    public static bool IsWithinSiphonRange(Vector2 selfPosition, Vector2 sunPosition) =>
        Vector2.Distance(selfPosition, sunPosition) <= SiphonRange;

    public static Vector2 VelocityToward(Vector2 selfPosition, Vector2 sunPosition) =>
        selfPosition.DirectionTo(sunPosition) * MoveSpeed;

    public void TurnToward(Vector2 selfPosition, Vector2 sunPosition, float deltaSeconds)
    {
        float targetAngle = selfPosition.DirectionTo(sunPosition).Angle();
        Rotation = SimMath.RotateToward(Rotation, targetAngle, TurnSpeed * deltaSeconds);
    }

    public void BeginSiphon() => IsSiphoning = true;
}
