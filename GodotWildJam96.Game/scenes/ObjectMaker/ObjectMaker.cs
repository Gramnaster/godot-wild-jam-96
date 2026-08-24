using Godot;

namespace GodotWildJam96;

public sealed partial class ObjectMaker : Node
{
    public override void _Ready()
    {
        EventBus.Instance.OnCreateBullet += OnCreateBulletHandler;
    }

    public override void _ExitTree()
    {
        EventBus.Instance.OnCreateBullet -= OnCreateBulletHandler;
    }

    // Instantiate bullet from PackedScene, configures in Setup(),
    // defers call so physics is clean state
    private void OnCreateBulletHandler(Vector2 position, Vector2 direction, float speed, float lifetimeSeconds, PackedScene scene)
    {
        if (scene is null) return;

        BulletBase bullet = scene.Instantiate<BulletBase>();
        bullet.Setup(position, direction, speed, lifetimeSeconds);
        CallDeferred(Node.MethodName.AddChild, bullet);
    }
}
