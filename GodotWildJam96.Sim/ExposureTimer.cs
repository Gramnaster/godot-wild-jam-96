namespace GodotWildJam96.Sim;

// Outside a sun's light radius the ship takes a tick of damage every interval.
// The clock runs whether or not the player is exposed, matching the shipped
// behaviour: it is only reset when damage actually lands.
public sealed class ExposureTimer(float unsafeDamageIntervalSeconds)
{
    private readonly float _unsafeDamageIntervalSeconds = unsafeDamageIntervalSeconds;

    public bool InLightRadius { get; set; } = true;

    public float Elapsed { get; private set; }

    // True when an unsafe interval has just elapsed and a point of damage is due.
    public bool Tick(float deltaSeconds)
    {
        Elapsed += deltaSeconds;

        if (!InLightRadius && Elapsed > _unsafeDamageIntervalSeconds)
        {
            Elapsed = 0.0f;
            return true;
        }

        return false;
    }
}
