using System;
using Godot;

namespace GodotWildJam96;

public partial class StartGameButton : TextureButton
{
    [Export] private PackedScene _levelBase;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Pressed += StartGame;
    }

    private void StartGame()
    {
        MusicPlayer.Instance.Stop();
        GetTree().ChangeSceneToFile($"res://scenes/LevelBase/LevelBase.tscn");
    }
}
