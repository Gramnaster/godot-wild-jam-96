using System;
using Godot;

namespace GodotWildJam96;

public partial class EventBus : Node
{

    public static EventBus Instance { get; private set; }

    // Sun events
    public event Action<Node2D, SunInteractionArea> OnShipEntered;
    public event Action<Node2D, SunInteractionArea> OnShipExited;
    public event Action<SunInteractionArea, int> OnSiphonStart;
    public event Action<SunInteractionArea> OnSiphonEnd;
    public event Action<Boolean> OnSiphonReset;
    public event Action<float> OnDamageTakenPlayer;
    public event Action<Player> OnTeleport;

    // Shooting events
    public event Action<Vector2, Vector2, float, float, PackedScene> OnCreateBullet;
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

    public void EmitOnSiphonStart(SunInteractionArea interactionArea, int siphonType)
    {
        //Code to see who is subscribed to this event
        //GD.Print($"Siphon Start Event Emitted {interactionArea.Name} on EventBus {GetInstanceId()}, subscribers: {OnSiphonStart?.GetInvocationList().Length ?? 0}");
        OnSiphonStart?.Invoke(interactionArea, siphonType);
    }

    public void EmitOnSiphonEnd(SunInteractionArea interactionArea)
    {
        //Code to see who is subscribed to this event
        //GD.Print($"Siphon End Event Emitted {interactionArea.Name} on EventBus {GetInstanceId()}, subscribers: {OnSiphonEnd?.GetInvocationList().Length ?? 0}");
        GD.Print("Siphon End Event Emitted");
        OnSiphonEnd?.Invoke(interactionArea);
    }

    public void EmitOnSiphonReset(Boolean reset)
    {
        GD.Print("Siphon Reset!");
        OnSiphonReset?.Invoke(reset);
    }
    public static void EmitOnOnDamageTakenPlayer(float dmg)
    {
        GD.Print("Player taking damage!");
        Instance.OnDamageTakenPlayer?.Invoke(dmg);
    }

    public static void EmitOnTeleport(Player player)
    {
        Instance.OnTeleport?.Invoke(player);
    }

    public static void EmitOnCreateBullet(Vector2 position, Vector2 direction, float speed, float lifetimeSeconds, PackedScene scene)
    {
        GD.Print("Im firing ma lazors");
        Instance.OnCreateBullet?.Invoke(position, direction, speed, lifetimeSeconds, scene);
    }

    public static void EmitOnCreateExplosion(Vector2 position)
    {
        GD.Print("Explosion triggered");
        Instance.OnCreateExplosion?.Invoke(position);
    }


}
