using System;
using Godot;

namespace GodotWildJam96;

/// <summary>
/// HitBox is a reusable collision component.
/// Wraps CollisionShape2D and exposes the shape via [Export]
/// so it can be ediated in the Godot IDE w/o exposing the node
/// </summary>
[Tool]
public sealed partial class HitBox : Area2D
{
    [Export] private CollisionShape2D _collisionShape;

    private Shape2D _shape;

    // Exposed to inspector. Setter updates CollisionShape2D immediately in editor
    [Export]
    public Shape2D Shape
    {
        get => _shape;
        set
        {
            _shape = value;
            // IsEditorHint() returns true inside Godot editor, not runtime
            // Guard prevents editor-only update code from running in-game
            if (Engine.IsEditorHint() && _collisionShape is not null)
            {
                _collisionShape.Shape = _shape;
                _collisionShape.QueueRedraw();
            }
        }
    }

    public override void _Ready()
    {
        // Applie the shape at runtime, in case assigned before _Ready
        _collisionShape.Shape = _shape;
    }

    public void Activate(bool isOn)
    {
        // Keeps track at the end of the current frame
        SetDeferred(Area2D.PropertyName.Monitoring, isOn);
        SetDeferred(Area2D.PropertyName.Monitorable, isOn);
    }
}
