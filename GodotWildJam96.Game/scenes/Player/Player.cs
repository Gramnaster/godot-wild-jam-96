using System;
using System.Linq;
using Godot;

namespace GodotWildJam96;

public partial class Player : CharacterBody2D
{

    //Tutorial Flags

    //Tutorial Labels

    #region Properties
    //Ship Properties
    private const float MAX_LINEAR_SPEED = 200.0f;
    private const float RETROGRADE_ANGLE_TOLERANCE = 0.05f;
    private const float RETROGRADE_VELOCITY_TOLERANCE = 10.0f;
    private const float UNSAFE_DAMAGE_INTERVAL = 10.0f;

    // Animation Names
    private const string ANIM_MAIN_THRUST_START = "MainThrustStart";
    private const string ANIM_MAIN_THRUST_CONTINUOUS = "MainThrustContinuous";
    private const string ANIM_MAIN_THRUST_POWER = "MainThrustPower";

    private const string ANIM_THRUST_FORWARD_START = "ThrustForward";
    private const string ANIM_THRUST_FORWARD_CONTINUOUS = "ThrustForwardContinuous";

    private const string ANIM_THRUST_LEFT_START = "ThrustLeft";
    private const string ANIM_THRUST_LEFT_CONTINUOUS = "ThrustLeftContinuous";

    private const string ANIM_THRUST_RIGHT_START = "ThrustRight";
    private const string ANIM_THRUST_RIGHT_CONTINUOUS = "ThrustRightContinuous";

    private const string ANIM_FIRING = "Firing";

    [Export] private int _energyLevels = 0;
    private int EnergyLevels
    {
        get => _energyLevels;
        set
        {
            // 0 to 6 (7 levels total)
            _energyLevels = Mathf.Clamp(value, 0, 6);
            UpdateEnergySprite();
        }
    }

    public bool InLightRadius { get; set; } = true;
    public float SafetyTimer { get; set; } = 0.0f;

    private Vector2 _closestSunVector;
    [Export] private Sprite2D _closestSunIndicator;

    [Export] private Sprite2D _playerSprite;
    [Export] private AnimatedSprite2D _firingSprite;
    [Export] private AnimatedSprite2D _thrustMainSprite;
    [Export] private AnimatedSprite2D _thrustForwardSprite;
    [Export] private AnimatedSprite2D _thrustLeftSprite;
    [Export] private AnimatedSprite2D _thrustRightSprite;
    [Export] private AnimatedSprite2D _energySprite;
    [Export] private Shooter _shooter;
    // [Export] private Label DebugLabel { get; set; }
    [Export] private Marker2D _leftMarker;
    [Export] private Marker2D _rightMarker;


    // Radians per scond. Tau is one full revolution per second.
    [Export] private float TurnSpeed { get; set; } = Mathf.Tau / 2;
    // Acceleration towards FacingDirection while thrusting
    [Export] private float ThrustAcceleration { get; set; } = 100.0f;
    [Export] private float ThrustDecceleration { get; set; } = 50.0f;
    [Export] private float _maxChargeSeconds = 1.0f;

    // State Properties
    public bool IsThrusting { get; private set; }
    public bool StartsThrusting { get; private set; }
    public bool IsPowerThrusting { get; private set; }

    public SunInteractionArea CurrentSunInteractionArea { get; set; }
    public bool SiphonUnderway { get; set; } = false;
    //If _siphonType = 0, siphoning out of sun, if _siphonType = 1, siphoning in
    private int _siphonType = 0;
    private float _interruptDamage;

    // Weapons will use this to query the angle
    private Vector2 FacingDirection => Vector2.FromAngle(GlobalRotation);

    // Attack properties
    private float _primarySpeed = 450f;                     // How fast the bullet moves
    private float _primaryLifetimeSeconds = 0.15f;           // How long the bullet lasts (determines range)
    private float _primaryChargedLifetimeSeconds = 0.8f;    // How long the bullet lasts after charging

    // Measure of time for the charge attack
    private ulong _shoot1PressedAtMsec;

    // All thruster effects with one shared state machine
    private Thruster[] _thrusters;

    private sealed class Thruster(
        AnimatedSprite2D sprite,
        string actionName,
        string startAnimation,
        string continuousAnimation
        )
    {
        public readonly AnimatedSprite2D Sprite = sprite;
        public readonly string ActionName = actionName;
        public readonly string StartAnimation = startAnimation;
        public readonly string ContinuousAnimation = continuousAnimation;
        public bool WasActive;
        public Action AnimationFinishedHandler;
    }
    #endregion

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

        if (@event.IsActionPressed("siphon_out") && CurrentSunInteractionArea != null)
        {
            if (!SiphonUnderway)
            {
                _siphonType = 0;
                // GD.Print("Start siphoning energy out of Sun");
                //0 For siphon out, 1 for siphon in. This is to differentiate between the two siphon events.
                EventBus.EmitOnSiphonStart(CurrentSunInteractionArea, _siphonType);
                EventBus.EmitOnSpawnDevourers(CurrentSunInteractionArea);
                SiphonUnderway = true;
            }
            else
            {
                // GD.Print("Already Siphoning!");
            }
        }
        else if (@event.IsActionPressed("siphon_in") && CurrentSunInteractionArea != null)
        {
            if (!SiphonUnderway)
            {
                _siphonType = 1;
                // GD.Print("Start siphoning energy into Sun");
                //0 For siphon out, 1 for siphon in. This is to differentiate between the two siphon events.
                EventBus.EmitOnSiphonStart(CurrentSunInteractionArea, _siphonType);
                EventBus.EmitOnSpawnDevourers(CurrentSunInteractionArea);
                SiphonUnderway = true;
            }
            else
            {
                // GD.Print("Already Siphoning!");
            }
        }
        if (@event.IsActionPressed("teleport_home"))
        {
            EventBus.EmitOnTeleport(this);
            TakeDamage(1);
        }
    }

    public override void _EnterTree()
    {
        AddToGroup(GameConstants.GroupPlayer);
    }

    public override void _Ready()
    {
        _playerSprite.RotationDegrees = 90f;
        _firingSprite.RotationDegrees = 90f;
        _thrustMainSprite.RotationDegrees = 90f;
        _thrustForwardSprite.RotationDegrees = 90f;
        _thrustLeftSprite.RotationDegrees = 90f;
        _thrustRightSprite.RotationDegrees = 90f;
        _energySprite.RotationDegrees = 90f;
        // _energySprite.

        // Effect sprites starts hidden. Show in UpdateThrusterAnimations()
        _firingSprite.Hide();
        _thrustMainSprite.Hide();
        _thrustForwardSprite.Hide();
        _thrustLeftSprite.Hide();
        _thrustRightSprite.Hide();
        _energySprite.Hide();

        EventBus.Instance.OnShipEntered += OnPlayerEntered;
        EventBus.Instance.OnPlayerSiphonReset += PlayerResetSiphon;
        EventBus.Instance.OnDamageTakenPlayer += TakeDamage;
        EventBus.Instance.OnEnergySiphoned += GainEnergyFromSun;

        // Animation events
        _thrusters =
        [
            new Thruster(_thrustMainSprite, "move_up", ANIM_MAIN_THRUST_START, ANIM_MAIN_THRUST_CONTINUOUS),
            new Thruster(_thrustForwardSprite, "move_down", ANIM_THRUST_FORWARD_START, ANIM_THRUST_FORWARD_CONTINUOUS),
            new Thruster(_thrustLeftSprite, "move_left", ANIM_THRUST_LEFT_START, ANIM_THRUST_LEFT_CONTINUOUS),
            new Thruster(_thrustRightSprite, "move_right", ANIM_THRUST_RIGHT_START, ANIM_THRUST_RIGHT_CONTINUOUS),
            // new Thruster(_thrustMainSprite, "brake", ANIM_MAIN_THRUST_START, ANIM_MAIN_THRUST_POWER),
        ];

        // Each thruster has its own AnimationFinished subscription
        // so it can pass itself from start clip to continuous
        foreach (Thruster thruster in _thrusters)
        {
            Thruster eventThruster = thruster;
            eventThruster.AnimationFinishedHandler = () => OnThrusterAnimationFinished(eventThruster);
            eventThruster.Sprite.AnimationFinished += eventThruster.AnimationFinishedHandler;
        }

        _firingSprite.AnimationFinished += OnFiringAnimationFinished;

        // Starting energy levels
        EnergyLevels = 3;

        // Makes the label independent of Player transformations
        // DebugLabel.TopLevel = true;
    }

    public override void _ExitTree()
    {
        EventBus.Instance.OnPlayerSiphonReset -= PlayerResetSiphon;
        EventBus.Instance.OnShipEntered -= OnPlayerEntered;
        EventBus.Instance.OnDamageTakenPlayer -= TakeDamage;
        EventBus.Instance.OnEnergySiphoned -= GainEnergyFromSun;

        if (_thrusters != null)
        {
            foreach (Thruster thruster in _thrusters)
            {
                thruster.Sprite.AnimationFinished -= thruster.AnimationFinishedHandler;
            }
        }

        _firingSprite.AnimationFinished -= OnFiringAnimationFinished;
    }

    // "Firing" loops, so treat one loop as a single muzzle-flash flash
    private void OnFiringAnimationFinished()
    {
        _firingSprite.Hide();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Converted to float since a lot of methods need deltaTime in float
        float dt = (float)delta;

        GetInput(dt);
        UpdateThrusterAnimations();
        // RapidShoot();
        MoveAndSlide();
        CheckIfSafe((float)delta);
    }

    public override void _Process(double delta)
    {
        FindClosestSun();
    }

    public void OnPlayerEntered(Player player, SunInteractionArea interactionArea)
    {
        // GD.Print(player.Name + " entered " + interactionArea.Name);
        CurrentSunInteractionArea = interactionArea;
        player.InLightRadius = true;
    }
    private void GetInput(float dt)
    {
        // 'A'/'D' sets turn rate. Stops when released.
        float turnInput = Input.GetAxis("move_left", "move_right");
        Rotation += turnInput * TurnSpeed * dt;

        // 'W'/'S' apply thrust where facing
        Thrust(dt);
        Brake(dt);

        // Limit to how fast player goes or they'll zoom too fast
        Velocity = Velocity.LimitLength(MAX_LINEAR_SPEED);

        // Debug
        // DebugLabel.GlobalPosition = GlobalPosition + new Vector2(0, -50);
        // DebugLabel.Text = $"{Velocity.ToString("F2")}-{Rotation:F2}";
    }

    private void Thrust(float dt)
    {
        bool wasThrusting = IsThrusting;
        IsThrusting = Input.IsActionPressed("move_up");
        StartsThrusting = IsThrusting && !wasThrusting;

        if (IsThrusting)
        {
            Velocity += FacingDirection * ThrustAcceleration * dt;
        }

        if (Input.IsActionPressed("move_down"))
        {
            Velocity -= FacingDirection * ThrustDecceleration * dt;
        }
    }

    private void Brake(float dt)
    {
        IsPowerThrusting = false;

        if (Input.IsActionPressed("brake")
            && Velocity.LengthSquared() > RETROGRADE_VELOCITY_TOLERANCE)
        {
            // Point nose retrograde then burn
            float retrogradeRotation = (-Velocity).Angle();
            Rotation = Mathf.RotateToward(Rotation, retrogradeRotation, TurnSpeed * dt);

            // Only burn once nose is actually pointed retrograde
            float angleDiff = Mathf.Abs(Mathf.AngleDifference(Rotation, retrogradeRotation));
            if (angleDiff < RETROGRADE_ANGLE_TOLERANCE)
            {
                IsPowerThrusting = true;

                Velocity += FacingDirection * ThrustAcceleration * dt * 1.5f;
                Velocity = Velocity.LimitLength(MAX_LINEAR_SPEED);
            }
        }
    }

    private void UpdateThrusterAnimations()
    {
        foreach (Thruster thruster in _thrusters)
        {
            bool active = Input.IsActionPressed(thruster.ActionName);

            // Std Main thruster gets a higher-priority over Power Main thruster
            if (thruster.Sprite == _thrustMainSprite && IsPowerThrusting)
            {
                thruster.Sprite.Show();

                if (thruster.Sprite.Animation != ANIM_MAIN_THRUST_POWER)
                {
                    thruster.Sprite.Play(ANIM_MAIN_THRUST_POWER);
                }

                thruster.WasActive = true;
                continue;
            }

            // Standard thruster handling
            if (active)
            {
                // Because they're all hidden at _Ready
                thruster.Sprite.Show();

                if (!thruster.WasActive || !thruster.Sprite.IsPlaying())
                {
                    thruster.Sprite.Play(thruster.StartAnimation);
                }
            }
            else
            {
                thruster.Sprite.Stop();
                thruster.Sprite.Hide();
            }

            thruster.WasActive = active;
        }
    }

    private static void OnThrusterAnimationFinished(Thruster thruster)
    {
        bool active = Input.IsActionPressed(thruster.ActionName);

        if (active && thruster.Sprite.Animation == thruster.StartAnimation)
        {
            thruster.Sprite.Play(thruster.ContinuousAnimation);
        }
    }

    private void UpdateEnergySprite()
    {
        if (_energyLevels == 0)
        {
            _energySprite.Hide();
            return;
        }

        _energySprite.Show();
        _energySprite.Frame = _energyLevels - 1;
    }

    private void ShootFront(float chargeRatio)
    {
        // Charging time between the attacks determines how far the projectile goes
        float adjustedLifetime = Mathf.Lerp(_primaryLifetimeSeconds, _primaryChargedLifetimeSeconds, chargeRatio);

        // GD.Print($"Adjusted Lifetime: {adjustedLifetime}");
        _shooter.Shoot([(_leftMarker.GlobalPosition, FacingDirection), (_rightMarker.GlobalPosition, FacingDirection)], _primarySpeed, adjustedLifetime, 0.7f);

        _firingSprite.Show();
        _firingSprite.Play(ANIM_FIRING);
    }

    // Determines how much charging you can pull off in the listed charging time
    private float ChargeRatio(ulong pressedAtMsec)
    {
        float heldSeconds = (Time.GetTicksMsec() - pressedAtMsec) / 1000f;
        return Mathf.Clamp(heldSeconds / _maxChargeSeconds, 0f, 1f);
    }

    private void PlayerResetSiphon(bool reset)
    {
        // GD.Print("Siphon Reset!");
        SiphonUnderway = reset;
    }

    private void TakeDamage(int dmg)
    {
        EnergyLevels -= dmg;
        // GD.Print(dmg + " damage taken!");
        // GD.Print("Only " + _energyLevels + " energy levels left!");
        if (EnergyLevels <= 0)
        {
            GameOver();
        }
        //If the shield takes too much damage too fast, interrupt the siphoning
        if (dmg > _interruptDamage)
        {
            EventBus.EmitOnPlayerSiphonEnd(CurrentSunInteractionArea);
            SiphonUnderway = false;
        }
    }

    private void FindClosestSun()
    {

        Sun _closestSun = null;
        float _closestDist = float.MaxValue;

        foreach (Sun sun in GetTree().GetNodesInGroup(GameConstants.GroupSuns).OfType<Sun>())
        {
            float _distSquared = GlobalPosition.DistanceSquaredTo(sun.GlobalPosition);
            if (_distSquared < _closestDist)
            {
                _closestDist = _distSquared;
                _closestSun = sun;
            }
        }
        _closestSunVector = GlobalPosition.DirectionTo(_closestSun.GlobalPosition);
        _closestSunIndicator.LookAt(_closestSun.GlobalPosition);
        _closestSunIndicator.GlobalPosition = GlobalPosition + _closestSunVector * 50.0f;
    }

    private void GainEnergyFromSun(int energyGained)
    {
        EnergyLevels += energyGained;
        // GD.Print(energyGained);
        // GD.Print("Current Energy Levels: " + _energyLevels);
    }

    private void GameOver()
    {
        GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://scenes/GameOverScreen/GameOverScreen.tscn");
    }

    private void CheckIfSafe(float dt)
    {
        SafetyTimer += dt;
        if (!InLightRadius && SafetyTimer > UNSAFE_DAMAGE_INTERVAL)
        {
            TakeDamage(1);
            SafetyTimer = 0.0f;
        }
    }
}
