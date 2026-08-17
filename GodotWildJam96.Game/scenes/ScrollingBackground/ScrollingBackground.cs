using System;
using Godot;

namespace GodotWildJam96;

public partial class ScrollingBackground : Node2D
{
    [Export] private ScrollingBackgroundImages _scrollingImages;
    [Export] private Vector2 _baseSize = new(1920f, 1080f);
    [Export] private float _targetScale = 1.0f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }
}
