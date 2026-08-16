using Godot;
using System;
using System.Runtime.CompilerServices;


namespace GodotWildJam96;

public partial class Sun : Area2D
{

    public float _maxEnergy = 100.0f;
    public float _currentEnergy = 25.0f;
    //Light radius will be dependent on energy level of sun
    public float _lightRadius = 100.0f;
    public bool _siphonOngoing = false;
    public float _sunSiphonRate = 1.01f;

    public Sprite2D _lightRadiusSprite;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        EventBus.Instance.OnSiphonStart += StartSiphon;
        EventBus.Instance.OnSiphonEnd += StopSiphon;
        GD.Print($"[{GetPath()}] Subscribed to EventBus {EventBus.Instance.GetInstanceId()}");

        _lightRadiusSprite = this.GetChild<SunInteractionArea>(0)._lightRadiusSprite;
    }

    public override void _Draw()
    {
        EventBus.Instance.OnSiphonStart -= StartSiphon;
        EventBus.Instance.OnSiphonEnd -= StopSiphon;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
    {
        if(_siphonOngoing)
        {
            if (this._currentEnergy < 0.05f)
            {
                StopSiphon(null);
                return;
            }
            _lightRadiusSprite.Scale = _lightRadiusSprite.Scale/_sunSiphonRate;
            GD.Print("Siphoning!");
            this._currentEnergy /= _sunSiphonRate;
        }
    }


    public void StartSiphon(SunInteractionArea sunInteractionArea)
    {
        if (sunInteractionArea != null)
        {
            _siphonOngoing = true;
        }
    }

    public void StopSiphon(SunInteractionArea sunInteractionArea)
    {
        GD.Print("Siphon Stopped");
        //Can also add code to stop or 'finish' siphoning for other factors
        _siphonOngoing = false;
    }
}
