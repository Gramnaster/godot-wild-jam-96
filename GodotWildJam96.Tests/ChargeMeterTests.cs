using GodotWildJam96.Sim;
using Xunit;

namespace GodotWildJam96.Tests;

public class ChargeMeterTests
{
    [Fact]
    public void ZeroHold_ReturnsZero()
    {
        var meter = new ChargeMeter(1.0f);
        meter.Press(1000);

        float ratio = meter.Release(1000);

        Assert.Equal(0f, ratio);
    }

    [Fact]
    public void FullHold_ReturnsOne()
    {
        var meter = new ChargeMeter(1.0f);
        meter.Press(0);

        float ratio = meter.Release(1000);

        Assert.Equal(1f, ratio);
    }

    [Fact]
    public void OverHold_ClampsToOne()
    {
        var meter = new ChargeMeter(1.0f);
        meter.Press(0);

        float ratio = meter.Release(5000);

        Assert.Equal(1f, ratio);
    }
}
