using System;
using Godot;
using GodotWildJam96.Sim;

namespace GodotWildJam96;

public partial class Sun : Area2D
{
    //Sun Node Parts
    [Export] private SunInteractionArea _mySunInteractionArea;
    [Export] public EnergyBar EnergyValuebar { get; set; }
    [Export] public AudioStreamPlayer2D SiphonSound { get; set; }

    public SunInteractionArea CurrentSunInteractionArea { get; set; }

    // SunEnergy owns every energy and siphon value. This node holds no copy of
    // any of it -- it reads the simulation fresh each frame and renders it.
    protected SunEnergy Energy { get; private set; }

    // Subclasses (MainSun) can scale their interaction area up independently of the energy ratio.
    protected virtual float InteractionAreaScaleMultiplier => 1f;

    // Lowest energy a player siphon may drain this sun to. Regular suns can be fully
    // depleted; MainSun overrides this so it can't be player-drained to 0.
    protected virtual int MinPlayerDrainEnergy => 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        EventBus.Instance.OnSiphonStart += StartSiphon;
        EventBus.Instance.OnEnemySiphonStart += EnemyStartSiphon;
        EventBus.Instance.OnPlayerSiphonEnd += StopPlayerSiphon;
        EventBus.Instance.OnEnemySiphonStop += EnemyStopSiphon;
        EventBus.Instance.OnShipExited += OnPlayerExited;

        Energy = SunEnergy.RollRegular(new Random(), MinPlayerDrainEnergy, InteractionAreaScaleMultiplier);
        EnergyValuebar.InitializeValues(Energy.MaxEnergy, Energy.CurrentEnergy);
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
        SunTickResult result = Energy.Tick((float)delta);

        if (result.SiphonTicked)
        {
            SiphonSound.Play();
            UpdateInteractionAreaScale();

            if (result.EnergyCreditedToPlayer > 0)
            {
                EventBus.EmitOnEnergySiphoned(result.EnergyCreditedToPlayer);
            }

            EnergyValuebar.UpdateValue(Energy.CurrentEnergy);
        }

        if (result.PlayerSiphonReset)
        {
            EventBus.EmitOnPlayerSiphonReset(false);
        }

        if (Energy.SiphonCount > 0)
        {
            SiphonSound.PitchScale = 1.0f + (0.15f * Energy.SiphonCount);
        }
    }

    // Scales the whole interaction area (its collision radius and its light-glow sprite
    // together, since both are children of it) to match the sun's current energy ratio.
    protected void UpdateInteractionAreaScale()
    {
        float scale = Energy.InteractionAreaScale;
        _mySunInteractionArea.Scale = new Vector2(scale, scale);
    }

    public void OnPlayerExited(Player player, SunInteractionArea interactionArea)
    {
        player.InLightRadius = false;
        CurrentSunInteractionArea = null;
        StopPlayerSiphon(interactionArea);
    }

    public void StartSiphon(SunInteractionArea sunInteractionArea, SiphonDirection siphonDirection)
    {
        // Any direct call to StartSiphon is the player's path. A true return means
        // no siphon actually started or continued (e.g. an enemy siphon collided
        // with the player's in-progress one), so the player has to be told it's not
        // underway, otherwise SiphonUnderway sticks true forever.
        if (sunInteractionArea != null
            && sunInteractionArea == _mySunInteractionArea
            && Energy.StartPlayerSiphon(siphonDirection))
        {
            EventBus.EmitOnPlayerSiphonReset(false);
        }

        SiphonSound.Play();
    }

    private void EnemyStartSiphon(SunInteractionArea sunInteractionArea, Devourer devourer, SiphonDirection siphonDirection)
    {
        if (sunInteractionArea is null || sunInteractionArea != _mySunInteractionArea) return;

        StartSiphon(sunInteractionArea, SiphonDirection.Out);
        Energy.AssignEnemyOwner();
    }

    public void StopPlayerSiphon(SunInteractionArea sunInteractionArea)
    {
        if (Energy.StopPlayerSiphon())
        {
            EventBus.EmitOnPlayerSiphonReset(false);
        }
    }

    public void EnemyStopSiphon(SunInteractionArea sunInteractionArea)
    {
        Energy.StopEnemySiphon();
    }
}
