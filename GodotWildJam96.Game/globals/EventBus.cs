using System;
using Godot;

namespace GodotWildJam96;

public partial class EventBus : Node
{

    public static EventBus Instance { get; private set; }

    // Sun events
    public event Action<Node2D, SunInteractionArea> OnShipEntered;
    public event Action<Node2D, SunInteractionArea> OnShipExited;
    public event Action<SunInteractionArea> OnSiphonStart;
    public event Action<SunInteractionArea> OnSiphonEnd;

    // Shooting events
    public event Action<Vector2, Vector2, float, PackedScene> OnCreateBullet;
    public event Action<Vector2> OnCreateExplosion; // Usually for enemy death

    public override void _Ready()
    {
        if (Instance is not null)
        {
            QueueFree();
            return;
        }

        Instance = this;
    }

    public void EmitOnShipEntered(Node2D ship, SunInteractionArea interactionArea)
    {
        OnShipEntered?.Invoke(ship, interactionArea);
    }


    public void EmitOnShipExited(Node2D ship, SunInteractionArea interactionArea)
    {
        OnShipExited?.Invoke(ship, interactionArea);
    }

    public void EmitOnSiphonStart(SunInteractionArea interactionArea)
    {
        GD.Print($"Siphon Start Event Emitted {interactionArea.Name} on EventBus {GetInstanceId()}, subscribers: {OnSiphonStart?.GetInvocationList().Length ?? 0}");
        OnSiphonStart?.Invoke(interactionArea);
    }

    public void EmitOnSiphonEnd(SunInteractionArea interactionArea)
    {
        GD.Print($"Siphon End Event Emitted {interactionArea.Name} on EventBus {GetInstanceId()}, subscribers: {OnSiphonEnd?.GetInvocationList().Length ?? 0}");
        OnSiphonEnd?.Invoke(interactionArea);
    }

    public static void EmitOnCreateBullet(Vector2 position, Vector2 direction, float speed, PackedScene scene)
    {
        GD.Print("Im firing ma lazors");
        Instance.OnCreateBullet?.Invoke(position, direction, speed, scene);
    }

    public static void EmitOnCreateExplosion(Vector2 position)
    {
        GD.Print("Explosion triggered");
        Instance.OnCreateExplosion?.Invoke(position);
    }
}
