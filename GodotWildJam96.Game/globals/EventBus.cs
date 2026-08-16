using System;
using Godot;

namespace GodotWildJam96;

public partial class EventBus : Node
{

    public static EventBus Instance { get; private set; }
    public event Action<Node2D, SunInteractionArea> OnShipEntered;
    public event Action<Node2D, SunInteractionArea> OnShipExited;
    public event Action<SunInteractionArea> OnSiphonStart;
    public event Action<SunInteractionArea> OnSiphonEnd;
    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            QueueFree();
        }
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
}
