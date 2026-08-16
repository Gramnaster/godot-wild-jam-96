using Godot;
using System;

namespace GodotWildJam96;

public partial class Player : CharacterBody2D
{
	public const float SHIP_MOVESPEED = 300.0f;
    public Vector2 shipVelocity = new Vector2();
    //Removed a boolean _canSiphon as if SunInteractionArea is null, siphon cannot be started, and if it is not null, siphon can be started. So this boolean was redundant.
    public SunInteractionArea _currentSunInteractionArea;
    public bool _siphonUnderway = false;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("shoot"))
        {
            GD.Print("Shoot");
        }

        if (@event.IsActionPressed("switch_weapon_left"))
        {
            GD.Print("Switch Weapon Left");
        }

        if (@event.IsActionPressed("switch_weapon_right"))
        {
            GD.Print("Switch Weapon Right");
        }

        if (@event.IsActionPressed("siphon") && _currentSunInteractionArea != null)
        {
            GD.Print("Start Siphoning");
            _siphonUnderway = true;
            EventBus.Instance.EmitOnSiphonStart(_currentSunInteractionArea);
        }
    }

	public override void _PhysicsProcess(double delta)
	{
        GetInput();
		MoveAndSlide();
	}

    public void GetInput()
    {
        Vector2 shipVelocity = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        if (shipVelocity != Vector2.Zero)
        {
            shipVelocity = shipVelocity.Normalized();
        }
        Velocity = shipVelocity * SHIP_MOVESPEED;
    }

    public override void _Ready()
    {
        EventBus.Instance.OnShipEntered += OnPlayerEntered;
        EventBus.Instance.OnShipExited += OnPlayerExited;
    }

    public override void _ExitTree()
    {
        EventBus.Instance.OnShipEntered -= OnPlayerEntered;
        EventBus.Instance.OnShipExited -= OnPlayerExited;
    }

    public void OnPlayerEntered(Node2D player, SunInteractionArea interactionArea)
    {
        GD.Print("Ship entered " + interactionArea.Name);
        _currentSunInteractionArea = interactionArea;
    }


    public void OnPlayerExited(Node2D player, SunInteractionArea interactionArea)
    {
        GD.Print("Ship exited " + interactionArea.Name);
        if (_siphonUnderway == true)
        {
            GD.Print("Siphon stopped, you lost some energy!");
            _siphonUnderway = false;
        }
        _currentSunInteractionArea = null;
        EventBus.Instance.EmitOnSiphonEnd(interactionArea);
    }
}
