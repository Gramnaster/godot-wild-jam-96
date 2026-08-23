using System;
using Godot;

public partial class CreditsButton : TextureButton
{
    [Export] private PackedScene _creditScreen;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Pressed += ShowCredits;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void ShowCredits()
    {
        GetTree().ChangeSceneToFile($"res://scenes/CreditScreen/CreditScreen.tscn");
    }
}
