using Godot;
using GodotWildJam96;
using System;

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

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {

    }

    private void TeleportPlayerHome(Player player)
    {
        player.GlobalPosition = GlobalPosition;
    }
}
