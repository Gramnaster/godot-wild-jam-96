using Godot;

namespace GodotWildJam96;

public partial class Sun : Area2D
{
    //Sun Node Parts
    private SunInteractionArea _mySunInteractionArea;
    [Export] public EnergyBar EnergyValuebar { get; set; }
    [Export] public AudioStreamPlayer2D SiphonSound { get; set; }

    private int _siphonCount;

    //Standard Sun Values
    private int _sunLevel = 0;
    public int MaxEnergy { get; set; }
    public int CurrentEnergy { get; set; }

    // Interaction area never scales below this much energy's worth of size,
    // so a depleted sun stays reachable instead of shrinking to an uninteractable point.
    private const float MinInteractionEnergy = 2f;

    //Light radius will be dependent on energy level of sun
    private float _lightRadius = 100.0f;

    //Checking variables
    private bool _siphonOutOngoing = false;
    private bool _siphonInOngoing = false;
    private int _sunSiphonRate = 1;
    private float _siphonTimePassed = 0.0f;
    //0 for player siphon, 1 for enemy siphon
    private int _siphonOwner = 0;
    public SunInteractionArea CurrentSunInteractionArea { get; set; }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        EventBus.Instance.OnSiphonStart += StartSiphon;
        EventBus.Instance.OnEnemySiphonStart += EnemyStartSiphon;
        EventBus.Instance.OnPlayerSiphonEnd += StopPlayerSiphon;
        EventBus.Instance.OnEnemySiphonStop += EnemyStopSiphon;
        EventBus.Instance.OnShipExited += OnPlayerExited;

        //Help from Claude to see if EventBus is being subscribed to by this Sun instance
        //GD.Print($"[{GetPath()}] Subscribed to EventBus {EventBus.Instance.GetInstanceId()}");

        _mySunInteractionArea = GetChild<SunInteractionArea>(0);
        //Random range for sun level, this will be used to determine the max energy of the sun
        _sunLevel = GD.RandRange(1, 6);
        MaxEnergy = _sunLevel + 3;
        CurrentEnergy = GD.RandRange(3, MaxEnergy);
        EnergyValuebar.InitializeValues(MaxEnergy, CurrentEnergy);
        UpdateInteractionAreaScale();
        AddToGroup(GameConstants.GroupSuns);
    }

    public override void _ExitTree()
    {
        EventBus.Instance.OnSiphonStart -= StartSiphon;
        EventBus.Instance.OnEnemySiphonStart -= EnemyStartSiphon;
        EventBus.Instance.OnPlayerSiphonEnd -= StopPlayerSiphon;
        EventBus.Instance.OnEnemySiphonStop -= EnemyStopSiphon;
        EventBus.Instance.OnShipExited -= OnPlayerExited;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        UpdateEnergy((float)delta);
    }

    public void UpdateEnergy(float dt)
    {
        if (_siphonOutOngoing)
        {
            _siphonTimePassed += dt;
            // Enemy drains can still empty the sun; a player drain stops at MinPlayerDrainEnergy
            // so the player can't solo-trigger MainSun's instant game-over by over-absorbing.
            bool drainedToFloor = _siphonOwner == 0
                ? CurrentEnergy <= MinPlayerDrainEnergy
                : CurrentEnergy < 1;
            if (drainedToFloor)
            {
                StopPlayerSiphon(null);
                return;
            }
            else if (_siphonTimePassed > 1.8f)
            {
                SiphonSound.Play();
                CurrentEnergy -= _sunSiphonRate;
                UpdateInteractionAreaScale();
                if (_siphonOwner == 0)
                {
                    EventBus.EmitOnEnergySiphoned(_sunSiphonRate);
                }
                EnergyValuebar.UpdateValue(CurrentEnergy);
                _siphonTimePassed = 0.0f;
                _siphonCount++;
            }
            //GD.Print("Current Energy:" + CurrentEnergy + " Max Energy:" + MaxEnergy);
        }
        else if (_siphonInOngoing)
        {
            _siphonTimePassed += dt;
            if (CurrentEnergy >= MaxEnergy)
            {
                StopPlayerSiphon(null);
                return;
            }
            else if (_siphonTimePassed > 1.8f)
            {
                SiphonSound.Play();
                CurrentEnergy += _sunSiphonRate;
                UpdateInteractionAreaScale();
                EnergyValuebar.UpdateValue(CurrentEnergy);
                _siphonTimePassed = 0.0f;
                _siphonCount++;
            }
            //GD.Print("Current Energy:" + CurrentEnergy + " Max Energy:" + MaxEnergy);
        }
        if (_siphonCount > 0)
        {
            SiphonSound.PitchScale = 1.0f + (0.15f * _siphonCount);
        }
    }

    // Subclasses (MainSun) can scale their interaction area up independently of the energy ratio.
    protected virtual float InteractionAreaScaleMultiplier => 1f;

    // Lowest energy a player siphon may drain this sun to. Regular suns can be fully
    // depleted; MainSun overrides this so it can't be player-drained to 0.
    protected virtual int MinPlayerDrainEnergy => 0;

    // Scales the whole interaction area (its collision radius and its light-glow sprite
    // together, since both are children of it) to match the sun's current energy ratio.
    protected void UpdateInteractionAreaScale()
    {
        float energyRatio = Mathf.Max(CurrentEnergy, MinInteractionEnergy) / MaxEnergy;
        float scale = energyRatio * InteractionAreaScaleMultiplier;
        _mySunInteractionArea.Scale = new Vector2(scale, scale);
    }

    public void OnPlayerExited(Player player, SunInteractionArea interactionArea)
    {
        if (_siphonInOngoing || _siphonOutOngoing)
        {
            // GD.Print("Siphon stopped, you lost some energy!");
        }
        player.InLightRadius = false;
        CurrentSunInteractionArea = null;
        StopPlayerSiphon(interactionArea);
    }
    public void StartSiphon(SunInteractionArea sunInteractionArea, int siphonType)
    {
        if (sunInteractionArea != null && sunInteractionArea == _mySunInteractionArea)
        {
            // any direct call to StartSiphon is the player's path
            _siphonOwner = 0;

            if (siphonType == 0 && !_siphonOutOngoing)
            {
                _siphonInOngoing = false;
                _siphonOutOngoing = true;
            }
            else if (siphonType == 1 && !_siphonInOngoing)
            {
                _siphonOutOngoing = false;
                _siphonInOngoing = true;
            }
            else
            {
                _siphonInOngoing = false;
                _siphonOutOngoing = false;
                // No siphon actually starts/continues here (e.g. an enemy siphon
                // collided with the player's in-progress one),
                // so tell the player it's not underway
                // otherwise SiphonUnderway sticks true forever
                EventBus.EmitOnPlayerSiphonReset(false);
            }
        }

        SiphonSound.Play();
    }

    private void EnemyStartSiphon(SunInteractionArea sunInteractionArea, Devourer devourer, int siphonType)
    {
        if (sunInteractionArea is null || sunInteractionArea != _mySunInteractionArea) return;

        StartSiphon(sunInteractionArea, 0);
        _siphonOwner = 1;
    }

    public void StopPlayerSiphon(SunInteractionArea sunInteractionArea)
    {
        if ((_siphonOutOngoing || _siphonInOngoing) && _siphonOwner == 0)
        {
            // GD.Print("Siphon Stopped");
            //Can also add code to stop or 'finish' siphoning for other factors
            _siphonOutOngoing = false;
            _siphonInOngoing = false;
            _siphonCount = 0;
            EventBus.EmitOnPlayerSiphonReset(false);
        }
    }

    public void EnemyStopSiphon(SunInteractionArea sunInteractionArea)
    {
        // GD.Print("Siphon Stopped");
        //Can also add code to stop or 'finish' siphoning for other factors
        _siphonOutOngoing = false;
        _siphonInOngoing = false;
        _siphonOwner = 0;
    }
}
