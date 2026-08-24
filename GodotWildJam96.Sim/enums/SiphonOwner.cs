namespace GodotWildJam96.Sim;

// Who is currently draining a sun's siphon — determines drain-floor rules
// and who OnEnergySiphoned should credit.
public enum SiphonOwner
{
    Player,
    Enemy
}
