using System;
using Godot;

namespace GodotWildJam96;

public partial class EventBus : Node
{

    public static EventBus Instance { get; private set; }

    // Sun events
    public event Action<Player, SunInteractionArea> OnShipEntered;
    public event Action<Player, SunInteractionArea> OnShipExited;
    public event Action<Devourer, SunInteractionArea> OnDevourerEntered;
    public event Action<SunInteractionArea, int> OnSiphonStart;
    public event Action<SunInteractionArea, Devourer, int> OnEnemySiphonStart;
    public event Action<SunInteractionArea> OnEnemySiphonStop;
    public event Action<SunInteractionArea> OnPlayerSiphonEnd;
    public event Action<Boolean> OnPlayerSiphonReset;
    public event Action<float> OnDamageTakenPlayer;
    public event Action<Player> OnTeleport;
    public event Action OnAllSunsSpawned;

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

    public void EmitOnShipEntered(Player ship, SunInteractionArea interactionArea)
    {
        OnShipEntered?.Invoke(ship, interactionArea);
    }


    public void EmitOnShipExited(Player ship, SunInteractionArea interactionArea)
    {
        OnShipExited?.Invoke(ship, interactionArea);
    }

    public void EmitOnSiphonStart(SunInteractionArea interactionArea, int siphonType)
    {
        //Code to see who is subscribed to this event
        //GD.Print($"Siphon Start Event Emitted {interactionArea.Name} on EventBus {GetInstanceId()}, subscribers: {OnSiphonStart?.GetInvocationList().Length ?? 0}");
        OnSiphonStart?.Invoke(interactionArea, siphonType);
    }

    public static void EmitOnEnemySiphonStart(SunInteractionArea interactionArea, Devourer devourer, int siphonType)
    {
        Instance.OnEnemySiphonStart?.Invoke(interactionArea, devourer, siphonType);
    }
    public static void EmitOnEnemySiphonStop(SunInteractionArea interactionArea)
    {
        Instance.OnEnemySiphonStop?.Invoke(interactionArea);
    }

    public void EmitOnPlayerSiphonEnd(SunInteractionArea interactionArea)
    {
        //Code to see who is subscribed to this event
        //GD.Print($"Siphon End Event Emitted {interactionArea.Name} on EventBus {GetInstanceId()}, subscribers: {OnSiphonEnd?.GetInvocationList().Length ?? 0}");
        GD.Print("Siphon End Event Emitted");
        OnPlayerSiphonEnd?.Invoke(interactionArea);
    }

    public void EmitOnPlayerSiphonReset(Boolean reset)
    {
        GD.Print("Siphon Reset!");
        OnPlayerSiphonReset?.Invoke(reset);
    }
    public static void EmitOnDamageTakenPlayer(float dmg)
    {
        GD.Print("Player taking damage!");
        Instance.OnDamageTakenPlayer?.Invoke(dmg);
    }

    public static void EmitOnTeleport(Player player)
    {
        Instance.OnTeleport?.Invoke(player);
    }

    public static void EmitOnAllSunsSpawn()
    {
        Instance.OnAllSunsSpawned?.Invoke();
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

    public static void EmitOnDevourerEntered(Devourer devourer, SunInteractionArea interactionArea)
    {
        Instance.OnDevourerEntered?.Invoke(devourer, interactionArea);
    }


}
