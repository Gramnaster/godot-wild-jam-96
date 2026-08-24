using Godot;
using System;

namespace GodotWildJam96;

public partial class MainMenuButton : TextureButton
{
    [Export] private PackedScene _mainScene;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Pressed += BackToMainMenu;
	}

    private void BackToMainMenu()
    {
        GetTree().ChangeSceneToFile($"res://scenes/main/main.tscn");
    }
}
