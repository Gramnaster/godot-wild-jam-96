using Godot;
using System;
using System.Data;
using System.Runtime.CompilerServices;


namespace GodotWildJam96;

public partial class Sun : Area2D
{
    //Sun Node Parts
    private Sprite2D _lightRadiusSprite;
    private SunInteractionArea _mySunInteractionArea;
    [Export] public EnergyBar _energyValuebar;
    [Export] public AudioStreamPlayer2D _siphonSound;

    private int _siphonCount;

    //Standard Sun Values
    private int _sunLevel = 0;
    public int _maxEnergy;
    public int _currentEnergy;

    //Light radius will be dependent on energy level of sun
    private float _lightRadius = 100.0f;

    //Checking variables
    private bool _siphonOutOngoing = false;
    private bool _siphonInOngoing = false;
    private int _sunSiphonRate = 1;
    private float _siphonTimePassed = 0.0f;
    //0 for player siphon, 1 for enemy siphon
    private int _siphonOwner = 0;
    public SunInteractionArea _currentSunInteractionArea;


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
        _lightRadiusSprite = _mySunInteractionArea._lightRadiusSprite;
        //Random range for sun level, this will be used to determine the max energy of the sun
        _sunLevel = GD.RandRange(1, 6);
        _maxEnergy = _sunLevel + 3;
        _currentEnergy = GD.RandRange(3, _maxEnergy);
        _energyValuebar.InitializeValues(_maxEnergy, _currentEnergy);
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
        if(_siphonOutOngoing)
        {
            _siphonTimePassed += dt;
            if (_currentEnergy < 1)
            {
                StopPlayerSiphon(null);
                return;
            }
            else if(_siphonTimePassed > 1.8f)
            {
                _siphonSound.Play();
                _lightRadiusSprite.Scale = new Vector2 ((_currentEnergy/_maxEnergy),(_currentEnergy/_maxEnergy));
                _currentEnergy -= _sunSiphonRate;
                EventBus.EmitOnEnergySiphoned(_sunSiphonRate);
                _energyValuebar.UpdateValue(_currentEnergy);
                _siphonTimePassed = 0.0f;
                _siphonCount++;
            }
            //GD.Print("Current Energy:" + _currentEnergy + " Max Energy:" + _maxEnergy);
        }
        else if (_siphonInOngoing)
        {
            _siphonTimePassed += dt;
            if (_currentEnergy >= _maxEnergy)
            {
                StopPlayerSiphon(null);
                return;
            }
            else if(_siphonTimePassed > 1.8f)
            {
                _siphonSound.Play();
                _lightRadiusSprite.Scale = new Vector2 ((_currentEnergy/_maxEnergy),(_currentEnergy/_maxEnergy));
                _currentEnergy += _sunSiphonRate;
                _energyValuebar.UpdateValue(_currentEnergy);
                _siphonTimePassed = 0.0f;
                _siphonCount++;
            }
            GD.Print("Current Energy:" + _currentEnergy + " Max Energy:" + _maxEnergy);
        }
        if (_siphonCount > 0)
        {
            _siphonSound.PitchScale = 1.0f + (0.15f * _siphonCount);
        }
    }

    public void OnPlayerExited(Player player, SunInteractionArea interactionArea)
    {
        if (_siphonInOngoing || _siphonOutOngoing)
        {
            GD.Print("Siphon stopped, you lost some energy!");
            EventBus.EmitOnDamageTakenPlayer(1);
        }
        player._inLightRadius = false;
        _currentSunInteractionArea = null;
        StopPlayerSiphon(interactionArea);
    }
    public void StartSiphon(SunInteractionArea sunInteractionArea, int siphonType)
    {
        if (sunInteractionArea != null && sunInteractionArea == _mySunInteractionArea)
        {
            if (siphonType == 0 && !_siphonOutOngoing)
            {
                _siphonInOngoing = false;
                _siphonOutOngoing = true;
            }
            else if (siphonType == 1 && !_siphonInOngoing )
            {
                _siphonOutOngoing = false;
                _siphonInOngoing = true;
            }
            else
            {
                _siphonInOngoing = false;
                _siphonOutOngoing = false;
            }
        }

                _siphonSound.Play();
    }

    private void EnemyStartSiphon(SunInteractionArea sunInteractionArea, Devourer devourer, int siphonType)
    {
        _siphonOwner = 1;
        StartSiphon(sunInteractionArea, 0);
    }

    public void StopPlayerSiphon(SunInteractionArea sunInteractionArea)
    {
        if ((_siphonOutOngoing || _siphonInOngoing) && _siphonOwner == 0)
        {
            GD.Print("Siphon Stopped");
            //Can also add code to stop or 'finish' siphoning for other factors
            _siphonOutOngoing = false;
            _siphonInOngoing = false;
            _siphonCount = 0;
            EventBus.Instance.EmitOnPlayerSiphonReset(false);
        }
    }

    public void EnemyStopSiphon(SunInteractionArea sunInteractionArea)
    {
            GD.Print("Siphon Stopped");
            //Can also add code to stop or 'finish' siphoning for other factors
            _siphonOutOngoing = false;
            _siphonInOngoing = false;
    }
}
