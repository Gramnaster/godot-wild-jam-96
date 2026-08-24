using System.Numerics;

namespace GodotWildJam96.Sim;

// A projectile flies straight at a fixed velocity and expires on a timer, so
// lifetime is what actually sets its range.
public sealed class ProjectileMotion(Vector2 velocity, float maxLifetimeSeconds)
{
    private readonly float _maxLifetimeSeconds = maxLifetimeSeconds;

    public Vector2 Velocity { get; } = velocity;

    public float Elapsed { get; private set; }

    public bool HasExpired => Elapsed >= _maxLifetimeSeconds;

    // Returns the projectile's new position and advances its lifetime.
    public Vector2 Advance(Vector2 position, float deltaSeconds)
    {
        Elapsed += deltaSeconds;
        return position + (Velocity * deltaSeconds);
    }
}
