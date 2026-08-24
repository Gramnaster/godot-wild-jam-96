using System;
using Godot;

namespace GodotWildJam96;

public partial class Main : Control
{

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        MusicPlayer.Instance.PlayMainTheme();
    }
}
