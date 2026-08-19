using System;
using Godot;
using Godot.Collections;

namespace GodotWildJam96;

[GlobalClass]
public partial class ScrollingBackgroundImages : Resource
{
    [Export] public Array<Texture2D> Images { get; set; } = new();

    public ScrollingBackgroundImages() { }
}
