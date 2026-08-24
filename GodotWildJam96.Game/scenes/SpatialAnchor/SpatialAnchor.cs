using Godot;
using System;

namespace GodotWildJam96;

public partial class SpatialAnchor : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        EventBus.Instance.OnTeleport += TeleportPlayerHome;
    }

    public override void _ExitTree()
    {
        EventBus.Instance.OnTeleport -= TeleportPlayerHome;
    }

    private void TeleportPlayerHome(Player player)
    {
        player.GlobalPosition = GlobalPosition;
    }
}
