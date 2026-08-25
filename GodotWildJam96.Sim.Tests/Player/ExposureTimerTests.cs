using GodotWildJam96.Sim;
using Xunit;

namespace GodotWildJam96.Sim.Tests;

public class ExposureTimerTests
{
    [Fact]
    public void InLightRadius_NeverDealsDamage()
    {
        var timer = new ExposureTimer(10f);

        for (int i = 0; i < 100; i++)
        {
            Assert.False(timer.Tick(1f));
        }
    }

    [Fact]
    public void OutsideLightRadius_DealsDamageOnceTheIntervalElapses()
    {
        var timer = new ExposureTimer(10f) { InLightRadius = false };

        Assert.False(timer.Tick(9f));
        Assert.True(timer.Tick(2f));
    }

    // Strictly greater than, not >=: landing exactly on the interval is not due yet.
    [Fact]
    public void ExactlyAtTheInterval_IsNotYetDue()
    {
        var timer = new ExposureTimer(10f) { InLightRadius = false };

        Assert.False(timer.Tick(10f));
    }

    [Fact]
    public void AfterDamage_TheClockRestarts()
    {
        var timer = new ExposureTimer(10f) { InLightRadius = false };

        Assert.True(timer.Tick(11f));
        Assert.Equal(0f, timer.Elapsed);
        Assert.False(timer.Tick(5f));
    }

    // The clock accumulates while sheltered and is only cleared by actual damage,
    // so re-entering the dark can bill for time spent in the light. Pinned as shipped.
    [Fact]
    public void ClockKeepsRunningWhileSheltered()
    {
        var timer = new ExposureTimer(10f);

        timer.Tick(20f);
        timer.InLightRadius = false;

        Assert.True(timer.Tick(0.1f));
    }
}
