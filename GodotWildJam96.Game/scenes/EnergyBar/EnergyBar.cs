using Godot;
using System;

public partial class EnergyBar : ProgressBar
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void InitializeValues(float maxValue, float currentValue)
    {
        MaxValue = maxValue;
        Value = currentValue;
    }

    public void UpdateValue(float newValue)
    {
        Value = newValue;
    }
}
