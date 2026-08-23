using System;
using Godot;

namespace GodotWildJam96;

public partial class Main : Control
{

    [Export] AudioStreamPlayer2D _backgroundMusic;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _backgroundMusic.Play();
    }

    public override void _ExitTree()
    {
        _backgroundMusic.Stop();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }


}
