using System;
using Godot;

namespace GodotWildJam96;

public sealed partial class RestartButton : TextureButton
{
    [Export] private PackedScene _mainScene;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Pressed += RestartGame;
    }

    private void RestartGame()
    {
        GetTree().ChangeSceneToFile($"res://scenes/main/main.tscn");
    }
}
