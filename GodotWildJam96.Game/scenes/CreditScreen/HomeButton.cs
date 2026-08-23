using System;
using Godot;

public partial class HomeButton : TextureButton
{
    [Export] private PackedScene _mainScene;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Pressed += GoHome;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void GoHome()
    {
        GetTree().ChangeSceneToFile($"res://scenes/main/main.tscn");
    }
}
