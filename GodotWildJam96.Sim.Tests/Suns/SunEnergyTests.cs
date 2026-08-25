using System;
using GodotWildJam96.Sim;
using Xunit;

namespace GodotWildJam96.Sim.Tests;

public class SunEnergyTests
{
    private const float PastTick = SunEnergy.SiphonTickIntervalSeconds + 0.01f;

    private static SunEnergy Regular(int max = 8, int current = 5, int minPlayerDrain = 0) =>
        new(max, current, minPlayerDrain, interactionScaleMultiplier: 1f);

    [Fact]
    public void Tick_WithNoSiphonRunning_ChangesNothing()
    {
        SunEnergy sun = Regular();

        SunTickResult result = sun.Tick(PastTick);

        Assert.Equal(5, sun.CurrentEnergy);
        Assert.False(result.SiphonTicked);
        Assert.False(result.PlayerSiphonReset);
    }

    [Fact]
    public void SiphonOut_BeforeTickInterval_DoesNotDrain()
    {
        SunEnergy sun = Regular();
        sun.StartPlayerSiphon(SiphonDirection.Out);

        SunTickResult result = sun.Tick(SunEnergy.SiphonTickIntervalSeconds);

        Assert.Equal(5, sun.CurrentEnergy);
        Assert.False(result.SiphonTicked);
    }

    [Fact]
    public void SiphonOut_PastTickInterval_DrainsOneAndCreditsPlayer()
    {
        SunEnergy sun = Regular();
        sun.StartPlayerSiphon(SiphonDirection.Out);

        SunTickResult result = sun.Tick(PastTick);

        Assert.Equal(4, sun.CurrentEnergy);
        Assert.True(result.SiphonTicked);
        Assert.Equal(1, result.EnergyCreditedToPlayer);
        Assert.Equal(1, sun.SiphonCount);
    }

    [Fact]
    public void SiphonIn_PastTickInterval_RefillsButCreditsNothing()
    {
        SunEnergy sun = Regular();
        sun.StartPlayerSiphon(SiphonDirection.In);

        SunTickResult result = sun.Tick(PastTick);

        Assert.Equal(6, sun.CurrentEnergy);
        Assert.True(result.SiphonTicked);
        Assert.Equal(0, result.EnergyCreditedToPlayer);
    }

    [Fact]
    public void EnemySiphonOut_CreditsPlayerNothing()
    {
        SunEnergy sun = Regular();
        sun.StartPlayerSiphon(SiphonDirection.Out);
        sun.AssignEnemyOwner();

        SunTickResult result = sun.Tick(PastTick);

        Assert.Equal(4, sun.CurrentEnergy);
        Assert.Equal(0, result.EnergyCreditedToPlayer);
    }

    // The drain floor is the guard that stops a player emptying MainSun and
    // triggering its instant game-over single-handedly.
    [Fact]
    public void PlayerSiphonOut_StopsAtMinPlayerDrainEnergy()
    {
        SunEnergy sun = Regular(max: 8, current: 1, minPlayerDrain: 1);
        sun.StartPlayerSiphon(SiphonDirection.Out);

        SunTickResult result = sun.Tick(PastTick);

        Assert.Equal(1, sun.CurrentEnergy);
        Assert.True(result.PlayerSiphonReset);
    }

    // An enemy is not held to the player's drain floor -- it can empty the sun.
    [Fact]
    public void EnemySiphonOut_IgnoresPlayerDrainFloor()
    {
        SunEnergy sun = Regular(max: 8, current: 1, minPlayerDrain: 1);
        sun.StartPlayerSiphon(SiphonDirection.Out);
        sun.AssignEnemyOwner();

        sun.Tick(PastTick);

        Assert.Equal(0, sun.CurrentEnergy);
        Assert.True(sun.IsDepleted);
    }

    [Fact]
    public void SiphonIn_StopsAtMaxEnergy()
    {
        SunEnergy sun = Regular(max: 8, current: 8);
        sun.StartPlayerSiphon(SiphonDirection.In);

        SunTickResult result = sun.Tick(PastTick);

        Assert.Equal(8, sun.CurrentEnergy);
        Assert.True(sun.IsFull);
        Assert.True(result.PlayerSiphonReset);
    }

    [Fact]
    public void StartPlayerSiphon_SameDirectionTwice_ReportsNoSiphonStarted()
    {
        SunEnergy sun = Regular();

        Assert.False(sun.StartPlayerSiphon(SiphonDirection.Out));
        Assert.True(sun.StartPlayerSiphon(SiphonDirection.Out));
    }

    // Re-requesting the same direction cancels both flags, so a following tick
    // must not drain. This is the path that used to leave SiphonUnderway stuck.
    [Fact]
    public void StartPlayerSiphon_SameDirectionTwice_LeavesNoSiphonRunning()
    {
        SunEnergy sun = Regular();
        sun.StartPlayerSiphon(SiphonDirection.Out);
        sun.StartPlayerSiphon(SiphonDirection.Out);

        sun.Tick(PastTick);

        Assert.Equal(5, sun.CurrentEnergy);
    }

    [Fact]
    public void StartPlayerSiphon_OppositeDirection_SwitchesInsteadOfCancelling()
    {
        SunEnergy sun = Regular();
        sun.StartPlayerSiphon(SiphonDirection.Out);

        Assert.False(sun.StartPlayerSiphon(SiphonDirection.In));

        sun.Tick(PastTick);
        Assert.Equal(6, sun.CurrentEnergy);
    }

    [Fact]
    public void StopPlayerSiphon_WithNothingRunning_ReportsNoReset()
    {
        SunEnergy sun = Regular();

        Assert.False(sun.StopPlayerSiphon());
    }

    // An enemy-owned siphon is not the player's to stop.
    [Fact]
    public void StopPlayerSiphon_DoesNotStopAnEnemySiphon()
    {
        SunEnergy sun = Regular();
        sun.StartPlayerSiphon(SiphonDirection.Out);
        sun.AssignEnemyOwner();

        Assert.False(sun.StopPlayerSiphon());

        sun.Tick(PastTick);
        Assert.Equal(4, sun.CurrentEnergy);
    }

    [Fact]
    public void StopEnemySiphon_HaltsDrainAndReturnsOwnershipToPlayer()
    {
        SunEnergy sun = Regular();
        sun.StartPlayerSiphon(SiphonDirection.Out);
        sun.AssignEnemyOwner();

        sun.StopEnemySiphon();
        sun.Tick(PastTick);

        Assert.Equal(5, sun.CurrentEnergy);
    }

    [Fact]
    public void StopPlayerSiphon_ResetsSiphonCount()
    {
        SunEnergy sun = Regular();
        sun.StartPlayerSiphon(SiphonDirection.Out);
        sun.Tick(PastTick);
        Assert.Equal(1, sun.SiphonCount);

        sun.StopPlayerSiphon();

        Assert.Equal(0, sun.SiphonCount);
    }

    // Time accumulates across sub-interval ticks rather than resetting each frame.
    [Fact]
    public void SiphonOut_AccumulatesPartialDeltas_UntilIntervalElapses()
    {
        SunEnergy sun = Regular();
        sun.StartPlayerSiphon(SiphonDirection.Out);

        for (int i = 0; i < 10; i++)
        {
            sun.Tick(0.1f);
        }

        Assert.Equal(5, sun.CurrentEnergy);

        sun.Tick(0.9f);

        Assert.Equal(4, sun.CurrentEnergy);
    }

    [Fact]
    public void InteractionAreaScale_NeverDropsBelowTheTwoEnergyFloor()
    {
        SunEnergy sun = Regular(max: 8, current: 0);

        Assert.Equal(2f / 8f, sun.InteractionAreaScale, precision: 5);
    }

    [Fact]
    public void InteractionAreaScale_AppliesTheMultiplier()
    {
        var sun = new SunEnergy(15, 15, minPlayerDrainEnergy: 1, interactionScaleMultiplier: 4f);

        Assert.Equal(4f, sun.InteractionAreaScale, precision: 5);
    }

    [Fact]
    public void RollRegular_StaysWithinTheShippedLevelAndChargeRanges()
    {
        for (int seed = 0; seed < 200; seed++)
        {
            SunEnergy sun = SunEnergy.RollRegular(new Random(seed), minPlayerDrainEnergy: 0, interactionScaleMultiplier: 1f);

            Assert.InRange(sun.MaxEnergy, 4, 9);
            Assert.InRange(sun.CurrentEnergy, 3, sun.MaxEnergy);
        }
    }

    [Fact]
    public void RollRegular_SameSeed_IsDeterministic()
    {
        SunEnergy first = SunEnergy.RollRegular(new Random(11), 0, 1f);
        SunEnergy second = SunEnergy.RollRegular(new Random(11), 0, 1f);

        Assert.Equal(first.MaxEnergy, second.MaxEnergy);
        Assert.Equal(first.CurrentEnergy, second.CurrentEnergy);
    }

    [Fact]
    public void OverrideEnergy_ReplacesTheRolledValues()
    {
        SunEnergy sun = SunEnergy.RollRegular(new Random(3), 1, 4f);

        sun.OverrideEnergy(15, 3);

        Assert.Equal(15, sun.MaxEnergy);
        Assert.Equal(3, sun.CurrentEnergy);
    }
}
