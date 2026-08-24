using GodotWildJam96.Sim;
using Xunit;

namespace GodotWildJam96.Tests;

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

public class PlayerSiphonStateTests
{
    [Fact]
    public void TryStart_FromIdle_Starts()
    {
        var siphon = new PlayerSiphonState(0f);

        Assert.True(siphon.TryStart(SiphonDirection.In));
        Assert.True(siphon.Underway);
        Assert.Equal(SiphonDirection.In, siphon.Direction);
    }

    [Fact]
    public void TryStart_WhileAlreadyUnderway_IsRefusedAndKeepsDirection()
    {
        var siphon = new PlayerSiphonState(0f);
        siphon.TryStart(SiphonDirection.Out);

        Assert.False(siphon.TryStart(SiphonDirection.In));
        Assert.Equal(SiphonDirection.Out, siphon.Direction);
    }

    // The shipped interrupt threshold is 0, so every nonzero hit breaks a siphon.
    [Fact]
    public void ShouldInterrupt_AtTheShippedZeroThreshold_AnyHitInterrupts()
    {
        var siphon = new PlayerSiphonState(0f);

        Assert.True(siphon.ShouldInterrupt(1));
        Assert.False(siphon.ShouldInterrupt(0));
    }

    [Fact]
    public void ShouldInterrupt_WithARealThreshold_IgnoresLightHits()
    {
        var siphon = new PlayerSiphonState(2f);

        Assert.False(siphon.ShouldInterrupt(2));
        Assert.True(siphon.ShouldInterrupt(3));
    }
}

public class ShotProfileTests
{
    private static readonly ShotProfile Primary = new(Speed: 450f, MinLifetimeSeconds: 0.15f, MaxLifetimeSeconds: 0.8f);

    [Fact]
    public void NoCharge_GivesTheMinimumLifetime()
    {
        Assert.Equal(0.15f, Primary.LifetimeFor(0f), precision: 5);
    }

    [Fact]
    public void FullCharge_GivesTheMaximumLifetime()
    {
        Assert.Equal(0.8f, Primary.LifetimeFor(1f), precision: 5);
    }

    [Fact]
    public void HalfCharge_InterpolatesBetweenThem()
    {
        Assert.Equal(0.475f, Primary.LifetimeFor(0.5f), precision: 5);
    }
}
