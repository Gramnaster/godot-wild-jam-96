using System.Linq;
using Godot;

namespace GodotWildJam96;

public sealed partial class Player : CharacterBody2D
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
    private const string ANIM_FIRING = "Firing";

    public bool InLightRadius { get; set; } = true;
    public float SafetyTimer { get; set; } = 0.0f;

    private Vector2 _closestSunVector;
    [Export] private Sprite2D _closestSunIndicator;
    // Suns never move and are never freed after spawning, so this is safe
    // to cache once instead of re-querying the scene tree every frame.
    private Vector2[] _sunPositions = [];

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
    private SiphonDirection _siphonType = SiphonDirection.Out;
    private float _interruptDamage;

    // Weapons will use this to query the angle
    private Vector2 FacingDirection => Vector2.FromAngle(GlobalRotation);

    // Attack properties
    private readonly float _primarySpeed = 450f;                     // How fast the bullet moves
    private readonly float _primaryLifetimeSeconds = 0.15f;           // How long the bullet lasts (determines range)
    private readonly float _primaryChargedLifetimeSeconds = 0.8f;    // How long the bullet lasts after charging

    private EnergyPool _energyPool;
    private ChargeMeter _chargeMeter;
    private ThrusterAnimator _thrusterAnimator;
    #endregion

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("shoot1"))
        {
            _chargeMeter.Press(Time.GetTicksMsec());
        }

        if (@event.IsActionReleased("shoot1"))
        {
            ShootFront(_chargeMeter.Release(Time.GetTicksMsec()));
        }

        if (@event.IsActionPressed("siphon_out") && CurrentSunInteractionArea != null)
        {
            TryStartSiphon(SiphonDirection.Out);
        }
        else if (@event.IsActionPressed("siphon_in") && CurrentSunInteractionArea != null)
        {
            TryStartSiphon(SiphonDirection.In);
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

    private void TryStartSiphon(SiphonDirection direction)
    {
        if (SiphonUnderway)
        {
            // GD.Print("Already Siphoning!");
            return;
        }

        _siphonType = direction;
        // GD.Print($"Start siphoning energy {direction}");
        EventBus.EmitOnSiphonStart(CurrentSunInteractionArea, _siphonType);
        EventBus.EmitOnSpawnDevourers(CurrentSunInteractionArea);
        SiphonUnderway = true;
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

        // Effect sprites start hidden. Show in ThrusterAnimator.UpdateAnimations()
        _firingSprite.Hide();
        _energySprite.Hide();

        EventBus.Instance.OnShipEntered += OnPlayerEntered;
        EventBus.Instance.OnPlayerSiphonReset += PlayerResetSiphon;
        EventBus.Instance.OnDamageTakenPlayer += TakeDamage;
        EventBus.Instance.OnEnergySiphoned += GainEnergyFromSun;
        EventBus.Instance.OnAllSunsSpawned += OnAllSunsSpawned;

        _thrusterAnimator = new ThrusterAnimator(_thrustMainSprite, _thrustForwardSprite, _thrustLeftSprite, _thrustRightSprite);
        _chargeMeter = new ChargeMeter(_maxChargeSeconds);

        _firingSprite.AnimationFinished += OnFiringAnimationFinished;

        // Starting energy levels
        _energyPool = new EnergyPool(3, UpdateEnergySprite);

        // Makes the label independent of Player transformations
        // DebugLabel.TopLevel = true;
    }

    public override void _ExitTree()
    {
        EventBus.Instance.OnPlayerSiphonReset -= PlayerResetSiphon;
        EventBus.Instance.OnShipEntered -= OnPlayerEntered;
        EventBus.Instance.OnDamageTakenPlayer -= TakeDamage;
        EventBus.Instance.OnEnergySiphoned -= GainEnergyFromSun;
        EventBus.Instance.OnAllSunsSpawned -= OnAllSunsSpawned;

        _thrusterAnimator?.Unsubscribe();

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
        _thrusterAnimator.UpdateAnimations(IsPowerThrusting);
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

    private void UpdateEnergySprite(int levels)
    {
        if (levels == 0)
        {
            _energySprite.Hide();
            return;
        }

        _energySprite.Show();
        _energySprite.Frame = levels - 1;
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

    private void PlayerResetSiphon(bool reset)
    {
        // GD.Print("Siphon Reset!");
        SiphonUnderway = reset;
    }

    private void TakeDamage(int dmg)
    {
        _energyPool.Drain(dmg);
        // GD.Print(dmg + " damage taken!");
        // GD.Print("Only " + _energyPool.Levels + " energy levels left!");
        if (_energyPool.IsEmpty)
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

    private void OnAllSunsSpawned()
    {
        _sunPositions = GetTree().GetNodesInGroup(GameConstants.GroupSuns).OfType<Sun>()
            .Select(sun => sun.GlobalPosition).ToArray();
    }

    private void FindClosestSun()
    {
        int closestIndex = NearestTarget.IndexOfNearest(GlobalPosition, _sunPositions);
        if (closestIndex < 0) return;

        Vector2 closestSunPosition = _sunPositions[closestIndex];
        _closestSunVector = GlobalPosition.DirectionTo(closestSunPosition);
        _closestSunIndicator.LookAt(closestSunPosition);
        _closestSunIndicator.GlobalPosition = GlobalPosition + _closestSunVector * 50.0f;
    }

    private void GainEnergyFromSun(int energyGained)
    {
        _energyPool.Gain(energyGained);
        // GD.Print(energyGained);
        // GD.Print("Current Energy Levels: " + _energyPool.Levels);
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
