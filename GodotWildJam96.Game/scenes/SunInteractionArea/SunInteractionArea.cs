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

        this._lightRadiusSprite = this.GetChild<Sprite2D>(0);
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
        if (body is CharacterBody2D characterBody)
        {
            EventBus.Instance.EmitOnShipEntered(characterBody, this);
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is CharacterBody2D characterBody)
        {
            EventBus.Instance.EmitOnShipExited(characterBody, this);
        }
    }

}
