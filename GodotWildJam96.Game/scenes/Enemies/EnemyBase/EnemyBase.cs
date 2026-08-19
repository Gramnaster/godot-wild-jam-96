using System;
using System.Linq;
using Godot;
using GodotWildJam96;

namespace GodotWildJam96;

public partial class EnemyBase : CharacterBody2D
{
    [Export] private VisibleOnScreenNotifier2D _screenNotifier;
    [Export] private AnimatedSprite2D _animatedSprite2D;
    [Export] private HitBox _hitBox;
    [Export] private Timer _timer;

    // Per-enemy child tuning. Protected so sub-classes can read them.
    [Export] protected float Speed = 30f;
    [Export] protected int _sunPoints = 5;
    [Export] protected int _lives = 3;
    [Export] protected int _stolenPower = 0;

    // Encapsulation: Base class (enemyBase) owns this node,
    // Sub-classes can use it but NOT replace it
    protected AnimatedSprite2D AnimateSprite => _animatedSprite2D;
    protected Timer ActionTimer => _timer;

    // Protected so subclasses can aim at player and suns (for eating)
    protected Player PlayerRef { get; private set; }
    protected Sun[] SunRefs { get; private set; }

    private bool _isDead;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Gets the player in the group
        PlayerRef = GetTree().GetFirstNodeInGroup(GameConstants.GroupPlayer) as Player;

        if (PlayerRef is null)
        {
            GD.PrintErr($"{Name}: no Player found in group '{GameConstants.GroupPlayer}'. Queuing Free.");
            QueueFree();
            return;
        }

        EventBus.Instance.OnAllSunsSpawned += OnAllSunsSpawned;
        _screenNotifier.ScreenEntered += OnScreenEntered;
        _timer.Timeout += OnTimeout;
        _hitBox.AreaEntered += OnHitBoxAreaEntered;
    }

    public override void _ExitTree()
    {
        EventBus.Instance.OnAllSunsSpawned -= OnAllSunsSpawned;
        _timer.Timeout -= OnTimeout;
        _hitBox.AreaEntered -= OnHitBoxAreaEntered;
    }

    // Subclass decides response when ActionTimer fires
    protected virtual void OnTimeout() { }

    // Activates when it first appears on screen
    // Subclass overrides this to start animation and AI
    protected virtual void OnScreenEntered()
    {
        _timer.Start();
        // Unsubscribe immediately since we only want the first time it happens
        _screenNotifier.ScreenEntered -= OnScreenEntered;
    }

    // Area2D entering this hitbox kills the enemy
    // Subclasses can override this if they need custom hit behaviour
    protected virtual void OnHitBoxAreaEntered(Area2D area)
    {
        _lives--;

        if (_lives <= 0)
        {
            Die();
        }
    }

    // EventBus -> ObjectMaker -> Create Explosion
    private void Die()
    {
        if (_isDead) return;

        _isDead = true;
        EventBus.EmitOnCreateExplosion(GlobalPosition);
        // Need to EmitOnTransferPower too
        // But only if the enemies have power already
        QueueFree();
    }

    // Only gets the suns in the map once they've all spawned
    private void OnAllSunsSpawned()
    {
        SunRefs = GetTree().GetNodesInGroup(GameConstants.GroupSuns).OfType<Sun>().ToArray();
    }
}
