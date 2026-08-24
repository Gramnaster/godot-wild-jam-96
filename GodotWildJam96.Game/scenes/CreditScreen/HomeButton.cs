using System;
using Godot;

namespace GodotWildJam96;

public sealed partial class HomeButton : TextureButton
{
    [Export] private PackedScene _mainScene;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Pressed += GoHome;
    }

    private void GoHome()
    {
        GetTree().ChangeSceneToFile($"res://scenes/main/main.tscn");
    }
}
