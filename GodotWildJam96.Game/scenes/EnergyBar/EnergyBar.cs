using Godot;
using System;

namespace GodotWildJam96;

public sealed partial class EnergyBar : ProgressBar
{
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
