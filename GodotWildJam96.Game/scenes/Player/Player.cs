using System;
using System.Xml;
using Godot;

namespace GodotWildJam96;

public partial class Player : CharacterBody2D
{
    //Ship Properties
    private const float SHIP_MOVESPEED = 150.0f;
    public float _currentShieldEnergy = 0.0f;
    public float _maxShieldEnergy = 100.0f;

    [Export] private Sprite2D _playerSprite;
    [Export] private AudioStreamPlayer2D _shootSound;
    [Export] private Shooter _shooter;
    [Export] private Label DebugLabel { get; set; }

    // Radians per scond. Tau is one full revolution per second.
    [Export] private float TurnSpeed { get; set; } = Mathf.Tau;
    [Export] private float _maxChargeSeconds = 1.0f;


    public SunInteractionArea _currentSunInteractionArea;
    public bool _siphonUnderway = false;
    //If _siphonType = 0, siphoning out of sun, if _siphonType = 1, siphoning in
    private int _siphonType = 0;
    private float _interruptDamage;


    // Weapons will use this to query the angle
    private Vector2 FacingDirection => Vector2.FromAngle(GlobalRotation);

    // Disabled because we have no use for it at the moment
    // private Vector2 _shipVelocity = new();
    private float _targetRotation;

    // Attack properties
    private float _primarySpeed = 500f;                     // How fast the bullet moves
    private float _primaryChargedSpeed = 900f;              // (unused) How fast the bullet moves after charging
    private float _primaryLifetimeSeconds = 1.5f;           // How long the bullet lasts (determines range)
    private float _primaryChargedLifetimeSeconds = 3.0f;    // How long the bullet lasts after charging

    private float _secondarySpeed = 1500f;
    private float _secondaryChargedSpeed = 2200f;           // (unused)
    private float _secondaryLifetimeSeconds = 1.0f;
    private float _secondaryChargedLifetimeSeconds = 2.0f;

    // Measure of time for the charge attack
    private ulong _shoot1PressedAtMsec;
    private ulong _shoot2PressedAtMsec;


    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("shoot1"))
        {
            _shoot1PressedAtMsec = Time.GetTicksMsec();
        }
        if (@event.IsActionReleased("shoot1"))
        {
            ShootFront(ChargeRatio(_shoot1PressedAtMsec));
        }

        if (@event.IsActionPressed("shoot2"))
        {
            _shoot2PressedAtMsec = Time.GetTicksMsec();
        }

        if (@event.IsActionReleased("shoot2"))
        {
            ShootSide(ChargeRatio(_shoot2PressedAtMsec));
        }

        if (@event.IsActionPressed("siphon_out") && _currentSunInteractionArea != null)
        {
            _siphonType = 0;
            GD.Print("Start siphoning energy out of Sun");
            //0 For siphon out, 1 for siphon in. This is to differentiate between the two siphon events.
            EventBus.Instance.EmitOnSiphonStart(_currentSunInteractionArea, _siphonType);
        }
        else if (@event.IsActionPressed("siphon_in") && _currentSunInteractionArea != null)
        {
            _siphonType = 1;
            GD.Print("Start siphoning energy into Sun");
            //0 For siphon out, 1 for siphon in. This is to differentiate between the two siphon events.
            EventBus.Instance.EmitOnSiphonStart(_currentSunInteractionArea, _siphonType);
        }
    }
    public void OnPlayerEntered(Node2D player, SunInteractionArea interactionArea)
    {
        GD.Print("Ship entered " + interactionArea.Name);
        _currentSunInteractionArea = interactionArea;
    }
    public override void _Ready()
    {

        EventBus.Instance.OnShipEntered += OnPlayerEntered;
        EventBus.Instance.OnSiphonReset += ResetSiphon;
        EventBus.Instance.OnDamageTakenPlayer += TakeDamage;
        // Set initial _rotation for use in GetInput
        _targetRotation = Rotation;

        // Makes the label independent of Player transformations
        DebugLabel.TopLevel = true;
    }

    public override void _ExitTree()
    {
        EventBus.Instance.OnSiphonReset -= ResetSiphon;
        EventBus.Instance.OnShipEntered -= OnPlayerEntered;
        EventBus.Instance.OnDamageTakenPlayer -= TakeDamage;
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


    private void ShootFront(float chargeRatio)
    {
        // Charging time between the attacks determines how far the projectile goes
        float adjustedLifetime = Mathf.Lerp(_primaryLifetimeSeconds, _primaryChargedLifetimeSeconds, chargeRatio);

        GD.Print($"Adjusted Lifetime: {adjustedLifetime}");
        _shooter.Shoot([FacingDirection], _primarySpeed, adjustedLifetime);
    }

    private void ShootSide(float chargeRatio)
    {
        float adjustedLifetime = Mathf.Lerp(_secondaryLifetimeSeconds, _secondaryChargedLifetimeSeconds, chargeRatio);

        Vector2 fireLeft = FacingDirection.Rotated(-Mathf.Pi / 2f);
        Vector2 fireRight = FacingDirection.Rotated(Mathf.Pi / 2f);

        GD.Print($"Adjusted Lifetime: {adjustedLifetime}");
        _shooter.Shoot([fireLeft, fireRight], _secondarySpeed, adjustedLifetime);
    }

    // Determines how much charging you can pull off in the listed charging time
    private float ChargeRatio(ulong pressedAtMsec)
    {
        float heldSeconds = (Time.GetTicksMsec() - pressedAtMsec) / 1000f;
        return Mathf.Clamp(heldSeconds / _maxChargeSeconds, 0f, 1f);
    }

    private void ResetSiphon(bool reset)
    {
        GD.Print("Siphon Reset!");
        _siphonUnderway = reset;
    }

    private void TakeDamage(float dmg)
    {
        GD.Print(dmg + " damage taken!");
        GD.Print("Only " + _currentShieldEnergy + " shield energy left!");
        _currentShieldEnergy -= dmg;
        //If the shield takes too much damage too fast, interrupt the siphoning
        if (dmg > _interruptDamage)
        {
            EventBus.Instance.EmitOnSiphonEnd(_currentSunInteractionArea);
        }
    }
}
