using Godot;
using GodotWildJam96;
using System;

public partial class MainSun : Sun
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        base._Ready();
        _maxEnergy = 15;
        _currentEnergy = 3;
        _energyValuebar.InitializeValues(_maxEnergy, _currentEnergy);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
