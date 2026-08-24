using System;
using Godot;

namespace GodotWildJam96;

/// <summary>
/// Manages lifetimes of objects that need to disappear after awhile
/// </summary>
public sealed partial class Lifetime : Node
{
    [Export] private Timer _timer;
    [Export] private float _waitTime = 0f;

    public override void _Ready()
    {
        _timer.Timeout += OnTimeout;
        _timer.Start(_waitTime);
    }

    public override void _ExitTree()
    {
        _timer.Timeout -= OnTimeout;
    }

    // Free the Parent, not this Node
    private void OnTimeout()
    {
        GetParent().QueueFree();
    }
}
