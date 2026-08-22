using Godot;
using System;

namespace GodotWildJam96;

public partial class SunInteractionArea : Area2D
{
    public Sprite2D _lightRadiusSprite;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        _lightRadiusSprite = GetChild<Sprite2D>(0);
    }

    public override void _ExitTree()
    {
        BodyEntered -= OnBodyEntered;
        BodyExited -= OnBodyExited;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {

    }

    public override void _PhysicsProcess(double delta)
    {
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            EventBus.Instance.EmitOnShipEntered(player, this);
        }
        if (body is Squid squid)
        {
            GD.Print(squid.Name + " entered " + this.Name);
        }
        if (body is Devourer devourer)
        {
            EventBus.EmitOnDevourerEntered(devourer, this);
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Player player)
        {
            EventBus.Instance.EmitOnShipExited(player, this);
        }
    }

}
