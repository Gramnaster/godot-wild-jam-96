namespace GodotWildJam96.Sim;

// Charging between shots buys range, not damage: the charge ratio interpolates
// the projectile's lifetime, and lifetime is what determines how far it flies.
public readonly record struct ShotProfile(
    float Speed,
    float MinLifetimeSeconds,
    float MaxLifetimeSeconds)
{
    public float LifetimeFor(float chargeRatio) =>
        SimMath.Lerp(MinLifetimeSeconds, MaxLifetimeSeconds, chargeRatio);
}
