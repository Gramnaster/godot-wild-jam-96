namespace GodotWildJam96;

// A Squid's movement cycle: idle, then a short eased burst toward the
// player, then a coast that decays back to a stop.
public enum SquidMoveState
{
    Waiting,
    Thrusting,
    Coasting
}
