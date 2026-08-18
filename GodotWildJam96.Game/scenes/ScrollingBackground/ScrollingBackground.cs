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
        if (_scrollingImages is null) QueueFree();
        Scale = new Vector2(_targetScale, _targetScale);
        Position -= new Vector2(0f, _baseSize.Y * _targetScale);
    }

    private void AddLayer(float currentScrollScale, Texture2D image)
    {
        Parallax2D parallax2D = new()
        {
            RepeatSize = new(_baseSize.X, 0.0f),
            ScrollScale = new(currentScrollScale, 1.0f),
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
        float currentScrollScale = 0.0f;

        for (int i = 0; i < _scrollingImages.Images.Count; ++i)
        {
            AddLayer(currentScrollScale, _scrollingImages.Images[i]);
            currentScrollScale += scrollGap;
        }
    }
}
