using System.Linq;
using Godot;
using GodotWildJam96.Sim;
using SimVector2 = System.Numerics.Vector2;

namespace GodotWildJam96;

public sealed partial class Player : CharacterBody2D
{
    #region Properties
    private const float UnsafeDamageIntervalSeconds = 10.0f;

    // Ships as 0, so every nonzero hit interrupts an active siphon. Preserved
    // deliberately -- raising it is a game-feel call, not a refactor. See the
    // refactor roadmap's open items.
    private const float InterruptDamage = 0.0f;

    private const int StartingEnergyLevels = 3;

    // Animation Names
    private const string ANIM_FIRING = "Firing";

    private Vector2 _closestSunVector;
    [Export] private Sprite2D _closestSunIndicator;
    // Suns never move and are never freed after spawning, so this is safe
    // to cache once instead of re-querying the scene tree every frame.
    private SimVector2[] _sunPositions = [];

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

    // Inspector-authored tuning, handed to ShipMotion once in _Ready. These are
    // config, not runtime state -- the simulation owns the values that change.
    // Radians per second. Tau is one full revolution per second.
    [Export] private float TurnSpeed { get; set; } = Mathf.Tau / 2;
    // Acceleration towards FacingDirection while thrusting
    [Export] private float ThrustAcceleration { get; set; } = 100.0f;
    [Export] private float ThrustDecceleration { get; set; } = 50.0f;
    [Export] private float _maxChargeSeconds = 1.0f;

    public SunInteractionArea CurrentSunInteractionArea { get; set; }

    // Attack properties
    private readonly ShotProfile _primaryShot = new(
        Speed: 450f,
        MinLifetimeSeconds: 0.15f,
        MaxLifetimeSeconds: 0.8f);

    // Simulation. These own every gameplay value below; this node keeps no copy.
    private readonly ExposureTimer _exposure = new(UnsafeDamageIntervalSeconds);
    private readonly PlayerSiphonState _siphon = new(InterruptDamage);
    private ShipMotion _motion;
    private EnergyPool _energyPool;
    private ChargeMeter _chargeMeter;
    private ThrusterAnimator _thrusterAnimator;
    #endregion

    public bool InLightRadius
    {
        get => _exposure.InLightRadius;
        set => _exposure.InLightRadius = value;
    }

    public bool SiphonUnderway
    {
        get => _siphon.Underway;
        set => _siphon.Underway = value;
    }

    public bool IsThrusting => _motion.IsThrusting;
    public bool StartsThrusting => _motion.StartsThrusting;
    public bool IsPowerThrusting => _motion.IsPowerThrusting;

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
        // GD.Print("Already Siphoning!");
        if (!_siphon.TryStart(direction)) return;

        // GD.Print($"Start siphoning energy {direction}");
        EventBus.EmitOnSiphonStart(CurrentSunInteractionArea, _siphon.Direction);
        EventBus.EmitOnSpawnDevourers(CurrentSunInteractionArea);
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

        // Effect sprites start hidden. Show in ThrusterAnimator.UpdateAnimations()
        _firingSprite.Hide();
        _energySprite.Hide();

        EventBus.Instance.OnShipEntered += OnPlayerEntered;
        EventBus.Instance.OnPlayerSiphonReset += PlayerResetSiphon;
        EventBus.Instance.OnDamageTakenPlayer += TakeDamage;
        EventBus.Instance.OnEnergySiphoned += GainEnergyFromSun;
        EventBus.Instance.OnAllSunsSpawned += OnAllSunsSpawned;

        _motion = new ShipMotion(TurnSpeed, ThrustAcceleration, ThrustDecceleration);
        _thrusterAnimator = new ThrusterAnimator(_thrustMainSprite, _thrustForwardSprite, _thrustLeftSprite, _thrustRightSprite);
        _chargeMeter = new ChargeMeter(_maxChargeSeconds);

        _firingSprite.AnimationFinished += OnFiringAnimationFinished;

        // Starting energy levels
        _energyPool = new EnergyPool(StartingEnergyLevels, UpdateEnergySprite);

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

        // CharacterBody2D owns the transform -- MoveAndSlide rewrites Velocity
        // through collision response -- so the simulation is loaded from the body
        // each frame rather than keeping its own drifting copy of it.
        _motion.Velocity = Velocity.ToSim();
        _motion.Rotation = Rotation;

        _motion.Tick(ReadShipInput(), dt);

        Velocity = _motion.Velocity.ToGodot();
        Rotation = _motion.Rotation;

        _thrusterAnimator.UpdateAnimations(_motion.IsPowerThrusting);
        MoveAndSlide();

        if (_exposure.Tick(dt))
        {
            TakeDamage(1);
        }
    }

    private static ShipInput ReadShipInput() => new(
        // 'A'/'D' sets turn rate. Stops when released.
        TurnAxis: Input.GetAxis("move_left", "move_right"),
        // 'W'/'S' apply thrust where facing
        ThrustForward: Input.IsActionPressed("move_up"),
        ThrustReverse: Input.IsActionPressed("move_down"),
        Brake: Input.IsActionPressed("brake"));

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
        float adjustedLifetime = _primaryShot.LifetimeFor(chargeRatio);

        Vector2 facingDirection = _motion.FacingDirection.ToGodot();
        _shooter.Shoot(
            [(_leftMarker.GlobalPosition, facingDirection), (_rightMarker.GlobalPosition, facingDirection)],
            _primaryShot.Speed,
            adjustedLifetime,
            0.7f);

        _firingSprite.Show();
        _firingSprite.Play(ANIM_FIRING);
    }

    private void PlayerResetSiphon(bool reset)
    {
        // GD.Print("Siphon Reset!");
        _siphon.Underway = reset;
    }

    private void TakeDamage(int dmg)
    {
        _energyPool.Drain(dmg);
        if (_energyPool.IsEmpty)
        {
            GameOver();
        }

        //If the shield takes too much damage too fast, interrupt the siphoning
        if (_siphon.ShouldInterrupt(dmg))
        {
            EventBus.EmitOnPlayerSiphonEnd(CurrentSunInteractionArea);
            _siphon.Underway = false;
        }
    }

    private void OnAllSunsSpawned()
    {
        _sunPositions = GetTree().GetNodesInGroup(GameConstants.GroupSuns).OfType<Sun>()
            .Select(sun => sun.GlobalPosition.ToSim()).ToArray();
    }

    private void FindClosestSun()
    {
        int closestIndex = NearestTarget.IndexOfNearest(GlobalPosition.ToSim(), _sunPositions);
        if (closestIndex < 0) return;

        SimVector2 closestSunPosition = _sunPositions[closestIndex];
        _closestSunVector = GlobalPosition.ToSim().DirectionTo(closestSunPosition).ToGodot();

        _closestSunIndicator.LookAt(closestSunPosition.ToGodot());
        _closestSunIndicator.GlobalPosition = GlobalPosition + _closestSunVector * 50.0f;
    }

    private void GainEnergyFromSun(int energyGained)
    {
        _energyPool.Gain(energyGained);
    }

    private void GameOver()
    {
        GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://scenes/GameOverScreen/GameOverScreen.tscn");
    }
}
