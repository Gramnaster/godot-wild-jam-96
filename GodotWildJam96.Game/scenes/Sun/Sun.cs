using Godot;
using System;
using System.Runtime.CompilerServices;


namespace GodotWildJam96;

public partial class Sun : Area2D
{


    //Sun Node Parts
    private Sprite2D _lightRadiusSprite;
    private SunInteractionArea _mySunInteractionArea;
    [Export] public EnergyBar _energyValuebar;
    [Export] public AudioStreamPlayer2D _siphonSound;

    //Standard Sun Values
    private int _sunLevel = 0;
    public float _maxEnergy = 0.0f;
    private float _currentEnergy = 0.0f;

    //Light radius will be dependent on energy level of sun
    private float _lightRadius = 100.0f;

    //Checking variables
    private bool _siphonOutOngoing = false;
    private bool _siphonInOngoing = false;
    private float _sunSiphonRate = 5.2f;

    public SunInteractionArea _currentSunInteractionArea;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        EventBus.Instance.OnSiphonStart += StartSiphon;
        EventBus.Instance.OnSiphonEnd += StopSiphon;

        EventBus.Instance.OnShipExited += OnPlayerExited;

        //Help from Claude to see if EventBus is being subscribed to by this Sun instance
        //GD.Print($"[{GetPath()}] Subscribed to EventBus {EventBus.Instance.GetInstanceId()}");

        _mySunInteractionArea = GetChild<SunInteractionArea>(0);
        _lightRadiusSprite = _mySunInteractionArea._lightRadiusSprite;
        //Random range for sun level, this will be used to determine the max energy of the sun
        _sunLevel = GD.RandRange(0, 12);
        _maxEnergy = 100.0f + (_sunLevel * 10.0f);
        _currentEnergy = _maxEnergy;
        _energyValuebar.InitializeValues(_maxEnergy, _maxEnergy);
    }

    public override void _ExitTree()
    {
        EventBus.Instance.OnSiphonStart -= StartSiphon;
        EventBus.Instance.OnSiphonEnd -= StopSiphon;
        EventBus.Instance.OnShipExited -= OnPlayerExited;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
    {
        if(_siphonOutOngoing)
        {
            if (_currentEnergy < 0.05f)
            {
                StopSiphon(null);
                return;
            }
            _lightRadiusSprite.Scale = new Vector2 ((_currentEnergy/_maxEnergy),(_currentEnergy/_maxEnergy));
            GD.Print("Siphoning!");
            _currentEnergy -= _sunSiphonRate;
            _energyValuebar.UpdateValue(_currentEnergy);
        }
        else if (_siphonInOngoing)
        {
            if (_currentEnergy >= _maxEnergy)
            {
                StopSiphon(null);
                return;
            }
            _lightRadiusSprite.Scale = new Vector2 ((_currentEnergy/_maxEnergy),(_currentEnergy/_maxEnergy));
            GD.Print("Siphoning!");
            _currentEnergy += _sunSiphonRate;
            _energyValuebar.UpdateValue(_currentEnergy);
        }
    }




    public void OnPlayerExited(Node2D player, SunInteractionArea interactionArea)
    {
        GD.Print("Ship exited " + interactionArea.Name);
        if (_siphonInOngoing || _siphonOutOngoing)
        {
            GD.Print("Siphon stopped, you lost some energy!");
        }
        _currentSunInteractionArea = null;
        StopSiphon(interactionArea);
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

    public void StopSiphon(SunInteractionArea sunInteractionArea)
    {
        if (_siphonOutOngoing || _siphonInOngoing)
        {
            GD.Print("Siphon Stopped");
            //Can also add code to stop or 'finish' siphoning for other factors
            _siphonOutOngoing = false;
            _siphonInOngoing = false;
            _siphonSound.Playing = false;
            EventBus.Instance.EmitOnSiphonReset(false);
        }
    }
}
