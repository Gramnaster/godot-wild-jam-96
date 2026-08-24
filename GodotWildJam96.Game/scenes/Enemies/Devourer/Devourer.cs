using System;
using System.Diagnostics;
using Godot;
using GodotWildJam96.Sim;
using SimVector2 = System.Numerics.Vector2;

namespace GodotWildJam96;

public sealed partial class Devourer : EnemyBase
{
    [Export] private AnimatedSprite2D _mouthSprite;
    private const float MOVE_SPEED = 50.0f;
    // SunInteractionArea's own radius (~127) is the shared player light radius, too wide
    // for the Devourer's actual draining range, so it must close to within this first.
    private const float SIPHON_RANGE = 60.0f;

    protected override AnimatedSprite2D[] FlashSprites => [AnimateSprite, _mouthSprite];

    private Sun _currentClosestSun;
    private SunInteractionArea _pendingInteractionArea;
    // Parallel to SunRefs; suns never move, so this is safe to build once.
    private SimVector2[] _sunPositions = [];

    private bool _siphoning = false;

    public override void _Ready()
    {
        base._Ready();
        EventBus.Instance.OnDevourerEntered += DevourerEntered;
        base.OnAllSunsSpawned();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        EventBus.Instance.OnDevourerEntered -= DevourerEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_siphoning)
        {
            MoveToClosestSun();
            MoveAndSlide();
            TryStartSiphoning();
        }
        UpdateMouthAnimation();
    }

    private void TryStartSiphoning()
    {
        if (_pendingInteractionArea is null || _currentClosestSun is null) return;
        if (GlobalPosition.DistanceTo(_currentClosestSun.GlobalPosition) > SIPHON_RANGE) return;

        _siphoning = true;
        StartSiphoning(_pendingInteractionArea);
        _pendingInteractionArea = null;
    }

    // Body pulses while cruising toward a sun; mouth takes over once it's actually feeding.
    private void UpdateMouthAnimation()
    {
        if (_siphoning)
        {
            AnimateSprite.Stop();
            _mouthSprite.Play();
        }
        else
        {
            AnimateSprite.Play();
            _mouthSprite.Stop();
        }
    }

    public void StartSiphoning(SunInteractionArea sunInteractionArea)
    {
        EventBus.EmitOnEnemySiphonStart(sunInteractionArea, this, SiphonDirection.Out);
    }

    public void DevourerEntered(Devourer devourer, SunInteractionArea interactionArea)
    {
        if (devourer != this) return;
        _pendingInteractionArea = interactionArea;
    }
    private void FindClosestSun()
    {
        int closestIndex = NearestTarget.IndexOfNearest(GlobalPosition.ToSim(), _sunPositions);
        if (closestIndex < 0) return;

        _currentClosestSun = SunRefs[closestIndex];
    }

    private void MoveToClosestSun()
    {
        if (_currentClosestSun is null) return;
        LookAt(_currentClosestSun.GlobalPosition);
        Velocity = GlobalPosition.DirectionTo(_currentClosestSun.GlobalPosition) * MOVE_SPEED;
    }

    protected override void OnSunsReady()
    {
        _sunPositions = new SimVector2[SunRefs.Length];
        for (int i = 0; i < SunRefs.Length; i++)
        {
            _sunPositions[i] = SunRefs[i].GlobalPosition.ToSim();
        }
        FindClosestSun();
    }

    protected override void Die()
    {
        base.Die();
        EventBus.EmitOnEnemySiphonStop(null);
    }
}
