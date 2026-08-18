using System;
using Godot;

namespace GodotWildJam96;

public partial class ScrollingBackground : Node2D
{
    [Export] private ScrollingBackgroundImages _scrollingImages;
    [Export] private Vector2 _baseSize = new(1024f, 768f);
    [Export] private float _targetScale = 1.0f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (_scrollingImages is null) QueueFree();
        Setup();
        Scale = new Vector2(_targetScale, _targetScale);
        Position -= new Vector2(_baseSize.X * _targetScale, _baseSize.Y * _targetScale);
    }

    private void AddLayer(float currentScrollScale, Texture2D image)
    {
        Parallax2D parallax2D = new()
        {
            RepeatSize = _baseSize,
            RepeatTimes = 3,
            ScrollScale = new(currentScrollScale, currentScrollScale),
        };

        Sprite2D sprite = new()
        {
            Texture = image,
            Centered = false,
        };

        parallax2D.AddChild(sprite);
        AddChild(parallax2D);
    }

    private void Setup()
    {
        float scrollGap = 1.0f / _scrollingImages.Images.Count;
        float currentScrollScale = scrollGap;

        for (int i = 0; i < _scrollingImages.Images.Count; ++i)
        {
            AddLayer(currentScrollScale, _scrollingImages.Images[i]);
            currentScrollScale += scrollGap;
        }
    }
}
