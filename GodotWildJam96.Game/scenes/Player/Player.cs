using System;
using System.Xml;
using Godot;

namespace GodotWildJam96;

public partial class Player : CharacterBody2D
{
    private const float SHIP_MOVESPEED = 150.0f;

    [Export] private Sprite2D _playerSprite;
    [Export] private AudioStreamPlayer2D _shootSound;
    [Export] private Shooter _shooter;
    // Radians per scond. Tau is one full revolution per second.
    [Export] private float TurnSpeed { get; set; } = Mathf.Tau;
    [Export] private Label DebugLabel { get; set; }

    //Removed a boolean _canSiphon as if SunInteractionArea is null, siphon cannot be started, and if it is not null, siphon can be started. So this boolean was redundant.
    public SunInteractionArea _currentSunInteractionArea;
    public bool _siphonUnderway = false;

    // Weapons will use this to query the angle
    private Vector2 FacingDirection => Vector2.FromAngle(GlobalRotation);

    // Disabled because we have no use for it at the moment
    // private Vector2 _shipVelocity = new();
    private float _targetRotation;

    // Temporarily here until I figure out where to put this, ideally Shooter's _speed should go here instead.
    private float _primarySpeed = 500f;
    private float _secondarySpeed = 1500f;

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("shoot1"))
        {
            GD.Print("Shoot1");
            ShootFront();
        }

        if (@event.IsActionPressed("shoot2"))
        {
            GD.Print("Shoot2");
            ShootSide();
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

    public override void _Ready()
    {
        // Set initial _rotation for use in GetInput
        _targetRotation = Rotation;

        // Makes the label independent of Player transformations
        DebugLabel.TopLevel = true;

        EventBus.Instance.OnShipEntered += OnPlayerEntered;
        EventBus.Instance.OnShipExited += OnPlayerExited;
    }

    public override void _ExitTree()
    {
        EventBus.Instance.OnShipEntered -= OnPlayerEntered;
        EventBus.Instance.OnShipExited -= OnPlayerExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Converted to float since a lot of methods need deltaTime in float
        float dt = (float)delta;
        GetInput(dt);
        MoveAndSlide();
    }

    private void GetInput(float dt)
    {
        Vector2 shipVelocity = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        Velocity = shipVelocity * SHIP_MOVESPEED;

        // Gives us where ship should be pointing
        if (shipVelocity != Vector2.Zero)
        {
            _targetRotation = shipVelocity.Angle();
        }

        // Final ship rotation is what it should be pointed towards
        Rotation = Mathf.RotateToward(Rotation, _targetRotation, TurnSpeed * dt);

        // Debug
        DebugLabel.GlobalPosition = GlobalPosition + new Vector2(0, -50);
        DebugLabel.Text = $"{Velocity.ToString("F2")}-{Rotation:F2}";
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

    private void ShootFront()
    {
        _shooter.Shoot([FacingDirection], _primarySpeed);
    }

    private void ShootSide()
    {
        Vector2 fireLeft = FacingDirection.Rotated(-Mathf.Pi / 2f);
        Vector2 fireRight = FacingDirection.Rotated(Mathf.Pi / 2f);
        _shooter.Shoot([fireLeft, fireRight], _secondarySpeed);
    }
}
