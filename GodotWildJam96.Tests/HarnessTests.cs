using System;
using Godot;
using GodotWildJam96;
using Xunit;

namespace GodotWildJam96.Tests;

public class HarnessTests
{
    // Group 1: proves Vector2/Mathf are plain managed types that need no engine boot.
    // This is the load-bearing claim of the whole refactor roadmap -- if this fails,
    // nothing downstream is testable and the approach needs rethinking before any
    // gameplay code moves.
    [Fact]
    public void Vector2FromAngle_WorksWithoutEngineBoot()
    {
        Vector2 result = Vector2.FromAngle(0f);

        Assert.True(Math.Abs(result.X - 1f) < 0.0001f);
        Assert.True(Math.Abs(result.Y - 0f) < 0.0001f);
    }

    [Fact]
    public void MathfTau_WorksWithoutEngineBoot()
    {
        float expected = (float)(2 * Math.PI);

        Assert.True(Math.Abs(Mathf.Tau - expected) < 0.0001f);
    }

    // Group 2: proves the test host can load a type out of GodotWildJam-96.dll.
    // These strings are also a real runtime contract between the AddToGroup calls in
    // Sun.cs/Player.cs and the GetNodesInGroup lookups in EnemyBase.cs/Player.cs --
    // a typo here fails silently at runtime, not at compile time.
    [Fact]
    public void GameConstants_GroupPlayer_MatchesRuntimeContract()
    {
        Assert.Equal("Player", GameConstants.GroupPlayer);
    }

    [Fact]
    public void GameConstants_GroupSuns_MatchesRuntimeContract()
    {
        Assert.Equal("Suns", GameConstants.GroupSuns);
    }

    // Group 3: documents the testable/untestable boundary. Spawner loads as a type
    // (metadata only) and is never instantiated here -- constructing a Node2D requires
    // the native Godot runtime to be booted, which this test host does not do. This is
    // exactly why the refactor roadmap extracts pure logic out of node scripts instead
    // of testing nodes in place.
    [Fact]
    public void Spawner_IsANode2D_ButCannotBeInstantiatedHere()
    {
        Type spawnerType = typeof(Spawner);

        Assert.Equal("Node2D", spawnerType.BaseType?.Name);
    }
}
