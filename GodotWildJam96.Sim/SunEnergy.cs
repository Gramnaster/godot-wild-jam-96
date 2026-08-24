using System;

namespace GodotWildJam96.Sim;

// What one SunEnergy.Tick decided. The bridge renders this -- it never decides
// any of it: no sound, no bar update, and no event emission implies a rule.
public readonly record struct SunTickResult(
    bool SiphonTicked,
    bool PlayerSiphonReset,
    int EnergyCreditedToPlayer);

// A sun's energy and the whole siphon state machine: who is draining it, which
// direction, how far it may be drained, and when a tick lands.
public sealed class SunEnergy
{
    public const float SiphonTickIntervalSeconds = 1.8f;

    // The interaction area never scales below this much energy's worth of size,
    // so a depleted sun stays reachable instead of shrinking to an
    // uninteractable point.
    private const float MinInteractionEnergy = 2f;

    private const int SiphonRate = 1;

    private readonly int _minPlayerDrainEnergy;
    private readonly float _interactionScaleMultiplier;

    private bool _siphonOutOngoing;
    private bool _siphonInOngoing;
    private float _siphonTimePassed;
    private SiphonOwner _owner = SiphonOwner.Player;

    public SunEnergy(int maxEnergy, int currentEnergy, int minPlayerDrainEnergy, float interactionScaleMultiplier)
    {
        MaxEnergy = maxEnergy;
        CurrentEnergy = currentEnergy;
        _minPlayerDrainEnergy = minPlayerDrainEnergy;
        _interactionScaleMultiplier = interactionScaleMultiplier;
    }

    public int MaxEnergy { get; private set; }
    public int CurrentEnergy { get; private set; }

    // Consecutive ticks in the current siphon. Drives the siphon sound's rising
    // pitch in the bridge; reset to 0 when a player siphon stops.
    public int SiphonCount { get; private set; }

    public bool IsDepleted => CurrentEnergy == 0;
    public bool IsFull => CurrentEnergy == MaxEnergy;

    public float InteractionAreaScale =>
        MathF.Max(CurrentEnergy, MinInteractionEnergy) / MaxEnergy * _interactionScaleMultiplier;

    // A regular sun rolls a level of 1-6, which sets its capacity, then starts
    // somewhere between 3 and that capacity.
    public static SunEnergy RollRegular(Random rng, int minPlayerDrainEnergy, float interactionScaleMultiplier)
    {
        int sunLevel = rng.Next(1, 7);
        int maxEnergy = sunLevel + 3;
        int currentEnergy = rng.Next(3, maxEnergy + 1);
        return new SunEnergy(maxEnergy, currentEnergy, minPlayerDrainEnergy, interactionScaleMultiplier);
    }

    // MainSun replaces its rolled values with fixed ones after construction,
    // matching the original base._Ready()-then-overwrite order.
    public void OverrideEnergy(int maxEnergy, int currentEnergy)
    {
        MaxEnergy = maxEnergy;
        CurrentEnergy = currentEnergy;
    }

    public SunTickResult Tick(float deltaSeconds)
    {
        if (_siphonOutOngoing) return UpdateSiphon(deltaSeconds, SiphonDirection.Out);
        if (_siphonInOngoing) return UpdateSiphon(deltaSeconds, SiphonDirection.In);
        return default;
    }

    // Returns true when no siphon actually started or continued -- e.g. an enemy
    // siphon collided with the player's in-progress one. The bridge must then
    // tell the player it isn't underway, or SiphonUnderway sticks true forever.
    public bool StartPlayerSiphon(SiphonDirection direction)
    {
        _owner = SiphonOwner.Player;

        if (direction == SiphonDirection.Out && !_siphonOutOngoing)
        {
            _siphonInOngoing = false;
            _siphonOutOngoing = true;
            return false;
        }

        if (direction == SiphonDirection.In && !_siphonInOngoing)
        {
            _siphonOutOngoing = false;
            _siphonInOngoing = true;
            return false;
        }

        _siphonInOngoing = false;
        _siphonOutOngoing = false;
        return true;
    }

    public void AssignEnemyOwner() => _owner = SiphonOwner.Enemy;

    // Returns true when a player-owned siphon was actually stopped, so the
    // bridge knows whether to emit the reset.
    public bool StopPlayerSiphon()
    {
        if ((_siphonOutOngoing || _siphonInOngoing) && _owner == SiphonOwner.Player)
        {
            _siphonOutOngoing = false;
            _siphonInOngoing = false;
            SiphonCount = 0;
            return true;
        }

        return false;
    }

    public void StopEnemySiphon()
    {
        _siphonOutOngoing = false;
        _siphonInOngoing = false;
        _owner = SiphonOwner.Player;
    }

    private SunTickResult UpdateSiphon(float deltaSeconds, SiphonDirection direction)
    {
        _siphonTimePassed += deltaSeconds;

        // Enemy drains can empty the sun entirely; a player drain stops at
        // _minPlayerDrainEnergy so the player can't solo-trigger MainSun's
        // instant game-over by over-absorbing. Siphon-in has no owner
        // distinction -- it always stops at MaxEnergy.
        bool reachedStop = direction == SiphonDirection.Out
            ? (_owner == SiphonOwner.Player ? CurrentEnergy <= _minPlayerDrainEnergy : CurrentEnergy < 1)
            : CurrentEnergy >= MaxEnergy;

        if (reachedStop)
        {
            return new SunTickResult(
                SiphonTicked: false,
                PlayerSiphonReset: StopPlayerSiphon(),
                EnergyCreditedToPlayer: 0);
        }

        if (_siphonTimePassed <= SiphonTickIntervalSeconds) return default;

        CurrentEnergy += direction == SiphonDirection.Out ? -SiphonRate : SiphonRate;
        _siphonTimePassed = 0.0f;
        SiphonCount++;

        // Only a player-owned siphon-out credits the player; siphon-in credits nothing.
        int credited = direction == SiphonDirection.Out && _owner == SiphonOwner.Player ? SiphonRate : 0;

        return new SunTickResult(
            SiphonTicked: true,
            PlayerSiphonReset: false,
            EnergyCreditedToPlayer: credited);
    }
}
