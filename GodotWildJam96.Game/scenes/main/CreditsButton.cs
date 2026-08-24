using System;
using Godot;

namespace GodotWildJam96;

public sealed partial class CreditsButton : TextureButton
{
    [Export] private PackedScene _creditScreen;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Pressed += ShowCredits;
    }

    private void ShowCredits()
    {
        GetTree().ChangeSceneToFile($"res://scenes/CreditScreen/CreditScreen.tscn");
    }
}
