using System;
using Godot;

public partial class StartGameButton : TextureButton
{
    [Export] private PackedScene _levelBase;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Pressed += StartGame;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void StartGame()
    {
        GetTree().ChangeSceneToFile($"res://scenes/LevelBase/LevelBase.tscn");
    }
}
