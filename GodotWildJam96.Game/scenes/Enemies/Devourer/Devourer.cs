using Godot;
using System;
using System.Diagnostics;

namespace GodotWildJam96;

public partial class Devourer : EnemyBase
{
    private const float MOVE_SPEED = 50.0f;

    private Sun _currentClosestSun;

    private bool _siphoning = false;

    public override void _Ready()
    {
        base._Ready();
        EventBus.Instance.OnDevourerEntered += DevourerEntered;
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
            MoveToClosestSun((float)delta);
            MoveAndSlide();
        }
	}

    public void StartSiphoning(SunInteractionArea sunInteractionArea)
    {
        EventBus.EmitOnEnemySiphonStart(sunInteractionArea, this, 0);
    }

    public void DevourerEntered(Devourer devourer, SunInteractionArea interactionArea)
    {
        _siphoning = true;
        StartSiphoning(interactionArea);
    }
    private void FindClosestSun()
    {
        float _closestDist = float.MaxValue;
        foreach (Sun sun in SunRefs)
        {
            float _distSquared = GlobalPosition.DistanceSquaredTo(sun.GlobalPosition);
            if (_distSquared < _closestDist)
            {
                _closestDist = _distSquared;
                _currentClosestSun = sun;
            }
        }
    }

    private void MoveToClosestSun(float dt)
    {
        if (_currentClosestSun is null) return;
        LookAt(_currentClosestSun.GlobalPosition);
        Velocity = GlobalPosition.DirectionTo(_currentClosestSun.GlobalPosition) * MOVE_SPEED;
    }

    protected override void OnSunsReady()
    {
        FindClosestSun();
    }

    protected override void Die()
    {
        base.Die();
        EventBus.EmitOnEnemySiphonStop(null);
    }
}
