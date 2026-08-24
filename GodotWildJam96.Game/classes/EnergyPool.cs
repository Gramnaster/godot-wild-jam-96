using System;

namespace GodotWildJam96;

// 0 to 6 (7 levels total) -- matches Player's pre-extraction clamp range.
public sealed class EnergyPool
{
    private const int MinLevels = 0;
    private const int MaxLevels = 6;

    private readonly Action<int> _onChanged;
    private int _levels;

    public EnergyPool(int initialLevels, Action<int> onChanged)
    {
        _onChanged = onChanged;
        Levels = initialLevels;
    }

    public int Levels
    {
        get => _levels;
        private set
        {
            _levels = Math.Clamp(value, MinLevels, MaxLevels);
            _onChanged?.Invoke(_levels);
        }
    }

    public bool IsEmpty => _levels <= 0;

    public void Drain(int amount) => Levels -= amount;
    public void Gain(int amount) => Levels += amount;
}
