using GodotWildJam96.Sim;
using Xunit;

namespace GodotWildJam96.Tests;

public class EnergyPoolTests
{
    [Fact]
    public void Drain_ClampsAtZero()
    {
        var pool = new EnergyPool(2, _ => { });

        pool.Drain(5);

        Assert.Equal(0, pool.Levels);
        Assert.True(pool.IsEmpty);
    }

    [Fact]
    public void Gain_ClampsAtSix()
    {
        var pool = new EnergyPool(5, _ => { });

        pool.Gain(10);

        Assert.Equal(6, pool.Levels);
    }

    [Fact]
    public void Drain_ToExactlyZero_IsEmpty()
    {
        var pool = new EnergyPool(3, _ => { });

        pool.Drain(3);

        Assert.True(pool.IsEmpty);
    }

    [Fact]
    public void OnChanged_FiresWithClampedValue()
    {
        int? received = null;
        var pool = new EnergyPool(0, v => received = v);

        pool.Gain(10);

        Assert.Equal(6, received);
    }
}
