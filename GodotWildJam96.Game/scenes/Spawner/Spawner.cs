using System;
using System.Threading.Tasks;
using Godot;

namespace GodotWildJam96;

public partial class Spawner : Node2D
{

    [Export] public PackedScene SunScene { get; set; }
    [Export] public PackedScene MainSunScene { get; set; }
    [Export] public PackedScene DevourerScene { get; set; }
    [Export] public PackedScene SquidScene { get; set; }
    [Export] public Shape2D SpawnCheckShape { get; set; }
    //It's main for now, change to level base later
    [Export] public Node LevelBase { get; set; }
    [Export] private Timer _spawnSquidTimer;
    [Export] private Player _player;
    private readonly Random _rng = new();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        EventBus.Instance.OnSpawnDevourers += SpawnDevourers;
        MainSun newMainSun = MainSunScene.Instantiate<MainSun>();
        CallDeferred(Node.MethodName.AddChild, newMainSun);
        CallDeferred(MethodName.Trial);
        _spawnSquidTimer.Timeout += SpawnSquid;
    }

    public override void _ExitTree()
    {
        EventBus.Instance.OnSpawnDevourers -= SpawnDevourers;
        _spawnSquidTimer.Timeout -= SpawnSquid;
    }

    private void Trial()
    {
        _ = SpawnSuns(25);
    }
    //Logic for spawning suns
    private async Task SpawnSuns(int spawnCount)
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 sunPos = Vector2.Zero;
            bool positionFound = false;
            for (int attempt = 0; attempt < 25 && !positionFound; attempt++)
            {
                sunPos = SpawnPlacement.RandomSunPosition(_rng);
                positionFound = EnsurePositionValid(sunPos);
            }

            if (!positionFound)
            {
                //GD.Print("Abort spawning this sun! No suitable place found!");
                continue;
            }

            Sun newSun = SunScene.Instantiate<Sun>();
            //Instantiating the new sun as a child of the Main scene so it will be visible in the game.
            //The sun group itself is assigned in Sun._Ready, not here.
            LevelBase.AddChild(newSun);
            newSun.GlobalPosition = sunPos;

            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        }

        EventBus.EmitOnAllSunsSpawn();
    }
    private bool EnsurePositionValid(Vector2 position)
    {
        PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;
        var query = new PhysicsShapeQueryParameters2D
        {
            Shape = SpawnCheckShape,
            Transform = new Transform2D(0, position),
            CollisionMask = 1,
            CollideWithBodies = false,
            CollideWithAreas = true
        };
        var result = spaceState.IntersectShape(query);
        //GD.Print(result.Count == 0);
        return result.Count == 0; // If the result is empty, the position is valid
    }

    private void SpawnDevourers(SunInteractionArea interactionArea)
    {
        Devourer _newDevourer = DevourerScene.Instantiate<Devourer>();
        _newDevourer.GlobalPosition = interactionArea.GlobalPosition + new Vector2 (300.0f, 300.0f);
        AddChild(_newDevourer);

    }

    private void SpawnSquid()
    {
        Squid _newSquid = SquidScene.Instantiate<Squid>();
        _newSquid.GlobalPosition = _player.GlobalPosition + OffscreenSpawnOffset();
        AddChild(_newSquid);
        // GD.Print("Spawning Squid!");
        _spawnSquidTimer.Start();
    }

    // Picks a point just outside the camera's visible rectangle, on a random edge,
    // so squids spawn out of sight instead of popping in mid-screen.
    private Vector2 OffscreenSpawnOffset()
    {
        Camera2D camera = GetViewport().GetCamera2D();
        Vector2 halfExtent = camera is null
            ? new Vector2(480f, 360f)
            : GetViewport().GetVisibleRect().Size / 2f / camera.Zoom;

        return SpawnPlacement.OffscreenOffset(_rng, halfExtent);
    }
}
