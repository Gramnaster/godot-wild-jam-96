using System.Numerics;

namespace GodotWildJam96.Sim;

public enum EnemyHitResult
{
    Survived,
    Killed,

    // A hit that lands after the enemy is already dying. Lives still decrements
    // -- matching the shipped code, where only Die() was guarded, not the hit --
    // but the death effects must not fire a second time.
    AlreadyDead
}

// An enemy's remaining lives and the knockback it takes from a surviving hit.
public sealed class EnemyVitals(int lives)
{
    // Enemies stay hittable while flashing -- there are no invulnerability frames.
    private const float KnockbackDistance = 6.0f;

    public int Lives { get; private set; } = lives;

    public bool IsDead { get; private set; }

    public EnemyHitResult TakeHit()
    {
        Lives--;

        if (Lives > 0) return EnemyHitResult.Survived;
        if (IsDead) return EnemyHitResult.AlreadyDead;

        IsDead = true;
        return EnemyHitResult.Killed;
    }

    // A surviving hit nudges the enemy directly away from the origin.
    public static Vector2 KnockbackOffset(Vector2 position) =>
        position.Normalized() * KnockbackDistance;
}
