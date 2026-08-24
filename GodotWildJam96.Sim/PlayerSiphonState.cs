namespace GodotWildJam96.Sim;

// Whether the player currently has a siphon running, which way it is pointed,
// and whether an incoming hit is hard enough to break it off.
public sealed class PlayerSiphonState(float interruptDamage)
{
    private readonly float _interruptDamage = interruptDamage;

    public bool Underway { get; set; }

    public SiphonDirection Direction { get; private set; } = SiphonDirection.Out;

    // False when a siphon is already running, so the caller knows not to
    // re-emit the start events for one already underway.
    public bool TryStart(SiphonDirection direction)
    {
        if (Underway) return false;

        Direction = direction;
        Underway = true;
        return true;
    }

    // Any hit above the interrupt threshold breaks an active siphon. The
    // threshold ships as 0, so every nonzero hit interrupts -- see the
    // refactor roadmap's open item; that is a balance call, not a refactor.
    public bool ShouldInterrupt(int damage) => damage > _interruptDamage;
}
