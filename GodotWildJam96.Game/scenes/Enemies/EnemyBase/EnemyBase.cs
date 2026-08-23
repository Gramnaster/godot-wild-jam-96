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
    [Export] protected int SunPoints = 5;
    [Export] protected int Lives = 3;
    [Export] protected int Stolenpower = 0;
    [Export] protected float TargetRotation;
    [Export] protected float TurnSpeed = Mathf.Tau; // Radians/sec

    // Encapsulation: Base class (enemyBase) owns this node,
    // Sub-classes can use it but NOT replace it
    protected AnimatedSprite2D AnimateSprite => _animatedSprite2D;
    protected Timer ActionTimer => _timer;

    // Protected so subclasses can aim at player and suns (for eating)
    protected Player PlayerRef { get; private set; }
    protected Sun[] SunRefs { get; private set; }

    // Sprites flashed on hit. Subclasses with extra sprites (e.g. a mouth) override this.
    protected virtual AnimatedSprite2D[] FlashSprites => [_animatedSprite2D];

    private bool _isDead;
    private Tween[] _hitFlashTweens = [];

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
        _hitBox.BodyEntered += OnHitBoxBodyEntered;
    }

    public override void _ExitTree()
    {
        EventBus.Instance.OnAllSunsSpawned -= OnAllSunsSpawned;
        _timer.Timeout -= OnTimeout;
        _hitBox.AreaEntered -= OnHitBoxAreaEntered;
        _hitBox.BodyEntered -= OnHitBoxBodyEntered;
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
    protected virtual void OnHitBoxBodyEntered(Node2D body)
    {
        GD.Print("Body Entered is: " + body);
        if (body is Player)
        {
            EventBus.EmitOnDamageTakenPlayer(1);
        }
    }

    protected virtual void OnHitBoxAreaEntered(Area2D body)
    {
        GD.Print("Body Entered is: " + body);
        if (body is BulletBase)
        {
            GD.Print("You're hitting me: ", nameof(EnemyBase));
            Lives--;
            if (Lives <= 0)
            {
                Die();
            }
            else
            {
                GlobalPosition += (GlobalPosition.Normalized() * 6.0f);
                FlashOnHit();
            }
        }
        else
        {
            GD.Print("Entered a random body!");
        }
    }

    // Purely cosmetic hit feedback. Enemies stay hittable while flashing (no i-frames).
    // Each sprite gets its own tween so they blink in sync instead of one after another.
    private void FlashOnHit()
    {
        foreach (Tween tween in _hitFlashTweens)
        {
            tween?.Kill();
        }

        AnimatedSprite2D[] sprites = FlashSprites;
        _hitFlashTweens = new Tween[sprites.Length];
        for (int s = 0; s < sprites.Length; s++)
        {
            Tween tween = CreateTween();
            for (int i = 0; i < 13; i++)
            {
                tween.TweenProperty(sprites[s], "modulate:a", 0.2f, 0.05f);
                tween.TweenProperty(sprites[s], "modulate:a", 1.0f, 0.05f);
            }
            _hitFlashTweens[s] = tween;
        }
    }

    // EventBus -> ObjectMaker -> Create Explosion
    protected virtual void Die()
    {
        if (_isDead) return;

        _isDead = true;
        EventBus.EmitOnCreateExplosion(GlobalPosition);
        // Need to EmitOnTransferPower too
        // But only if the enemies have power already
        QueueFree();
    }

    // Only gets the suns in the map once they've all spawned
    protected void OnAllSunsSpawned()
    {
        SunRefs = GetTree().GetNodesInGroup(GameConstants.GroupSuns).OfType<Sun>().ToArray();
        OnSunsReady();
    }

    //Here so that enemies can do something with SunRefs when its done
    protected virtual void OnSunsReady() { }
}
