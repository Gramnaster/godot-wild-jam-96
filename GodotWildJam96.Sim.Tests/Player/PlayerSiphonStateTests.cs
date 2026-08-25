using GodotWildJam96.Sim;
using Xunit;

namespace GodotWildJam96.Sim.Tests;

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
