using System;
using System.Xml;
using Godot;

namespace GodotWildJam96;

public partial class Player : CharacterBody2D
{
    //Ship Properties
    private const float MAX_LINEAR_SPEED = 300.0f;
    private const float RETROGRADE_ANGLE_TOLERANCE = 0.05f;
    private const float RETROGRADE_VELOCITY_TOLERANCE = 10.0f;

    public float _currentShieldEnergy = 0.0f;
    public float _maxShieldEnergy = 100.0f;

    [Export] private Sprite2D _playerSprite;
    [Export] private AudioStreamPlayer2D _shootSound;
    [Export] private Shooter _shooter;
    [Export] private Label DebugLabel { get; set; }

    // Radians per scond. Tau is one full revolution per second.
    [Export] private float TurnSpeed { get; set; } = Mathf.Tau / 2;
    // Acceleration towards FacingDirection while thrusting
    [Export] private float ThrustAcceleration { get; set; } = 100.0f;
    [Export] private float ThrustDecceleration { get; set; } = 50.0f;
    [Export] private float _maxChargeSeconds = 1.0f;


    public SunInteractionArea _currentSunInteractionArea;
    public bool _siphonUnderway = false;
    //If _siphonType = 0, siphoning out of sun, if _siphonType = 1, siphoning in
    private int _siphonType = 0;
    private float _interruptDamage;

    // Weapons will use this to query the angle
    private Vector2 FacingDirection => Vector2.FromAngle(GlobalRotation);
    private float _targetRotation;

    // Attack properties
    private float _primarySpeed = 450f;                     // How fast the bullet moves
    private float _primaryChargedSpeed = 900f;              // (unused) How fast the bullet moves after charging
    private float _primaryLifetimeSeconds = 0.15f;           // How long the bullet lasts (determines range)
    private float _primaryChargedLifetimeSeconds = 0.8f;    // How long the bullet lasts after charging

    private float _secondarySpeed = 750f;
    private float _secondaryChargedSpeed = 2200f;           // (unused)
    private float _secondaryLifetimeSeconds = 0.25f;
    private float _secondaryChargedLifetimeSeconds = 1.0f;

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
        if (@event.IsActionPressed("teleport_home"))
        {
            EventBus.EmitOnTeleport(this);
            TakeDamage(5);
        }
    }
    public void OnPlayerEntered(Node2D player, SunInteractionArea interactionArea)
    {
        GD.Print("Ship entered " + interactionArea.Name);
        _currentSunInteractionArea = interactionArea;
    }

    public override void _EnterTree()
    {
        AddToGroup(GameConstants.GroupPlayer);
    }

    public override void _Ready()
    {

        EventBus.Instance.OnShipEntered += OnPlayerEntered;
        EventBus.Instance.OnSiphonReset += ResetSiphon;
        EventBus.Instance.OnDamageTakenPlayer += TakeDamage;

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
        // 'A'/'D' sets turn rate. Stops when released.
        float turnInput = Input.GetAxis("move_left", "move_right");
        Rotation += turnInput * TurnSpeed * dt;

        // 'W'/'S' apply thrust where facing
        Thrust(dt);
        Break(dt);

        // Limit to how fast player goes or they'll zoom too fast
        Velocity = Velocity.LimitLength(MAX_LINEAR_SPEED);

        // Debug
        DebugLabel.GlobalPosition = GlobalPosition + new Vector2(0, -50);
        DebugLabel.Text = $"{Velocity.ToString("F2")}-{Rotation:F2}";
    }

    private void Thrust(float dt)
    {
        if (Input.IsActionPressed("move_up"))
        {
            Velocity += FacingDirection * ThrustAcceleration * dt;
        }

        if (Input.IsActionPressed("move_down"))
        {
            Velocity -= FacingDirection * ThrustDecceleration * dt;
        }
    }

    private void Break(float dt)
    {
        if (Input.IsActionPressed("brake")
            && Velocity.LengthSquared() > RETROGRADE_VELOCITY_TOLERANCE * RETROGRADE_ANGLE_TOLERANCE)
        {
            // Point nose retrograde then burn
            float retrogradeRotation = (-Velocity).Angle();
            Rotation = Mathf.RotateToward(Rotation, retrogradeRotation, TurnSpeed * dt);

            // Only burn once nose is actually pointed retrograde
            float angleDiff = Mathf.Abs(Mathf.AngleDifference(Rotation, retrogradeRotation));
            if (angleDiff < RETROGRADE_ANGLE_TOLERANCE)
            {
                Velocity += FacingDirection * ThrustAcceleration * dt * 1.5f;
                Velocity = Velocity.LimitLength(MAX_LINEAR_SPEED);
            }

            return;
        }
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
