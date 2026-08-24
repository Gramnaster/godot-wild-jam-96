using System;
using System.Linq;
using System.Reflection;
using GodotWildJam96.Sim;
using Xunit;

namespace GodotWildJam96.Tests;

// The reference graph is the real enforcement -- this test project has no
// ProjectReference to the Game assembly, so a `using Godot;` in Sim fails the
// build before it ever gets here. These assert the same invariant at runtime so
// a transitive reference sneaking in via a future package is caught too.
public class SimBoundaryTests
{
    [Fact]
    public void SimAssembly_ReferencesNoGodotAssembly()
    {
        Assembly sim = typeof(EnergyPool).Assembly;

        string[] godotReferences = sim.GetReferencedAssemblies()
            .Select(name => name.Name ?? string.Empty)
            .Where(name => name.Contains("Godot", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.Empty(godotReferences);
    }

    [Fact]
    public void SimAssembly_LoadsAndConstructsWithoutEngineBoot()
    {
        // No GodotSharp.dll on disk next to the test host, no SceneTree, no
        // native runtime -- the simulation still runs its rules.
        var pool = new EnergyPool(3, onChanged: null);

        pool.Drain(1);

        Assert.Equal(2, pool.Levels);
    }
}
