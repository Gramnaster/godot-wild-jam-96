using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Godot;

namespace GodotWildJam96;

public partial class Spawner : Node2D
{

    [Export] public PackedScene _sunScene;
    [Export] public PackedScene _mainSunScene;

    [Export] public PackedScene _devourerScene;
    [Export] public Shape2D SpawnCheckShape;
    //It's main for now, change to level base later
    [Export] public Node _levelBase;

    private Vector2 _sunPos;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        MainSun newMainSun = _mainSunScene.Instantiate<MainSun>();
        CallDeferred("add_child", newMainSun);
        CallDeferred("Trial");
        //Devourer newDevourer = _devourerScene.Instantiate<Devourer>();
        //newDevourer.GlobalPosition = new Vector2 (100.0f, 100.0f);
        //AddChild(newDevourer);
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
            int SPAWN_ATTEMPTS = 0;
            do
            {
                SunSpawnCalculator();
                SPAWN_ATTEMPTS++;
                //GD.Print(_sunPos + " " + SPAWN_ATTEMPTS + " " + i);
            } while (!EnsurePositionValid(_sunPos) && SPAWN_ATTEMPTS < 25);

            if (SPAWN_ATTEMPTS == 25)
            {
                //GD.Print("Abort spawning this sun! No suitable place found!");
                continue;
            }

            Sun newSun = _sunScene.Instantiate<Sun>();
            //Adding to the Sun group so enemies can see it
            //Instantiating the new sun as a child of the Main scene so it will be visible in the game
            _levelBase.AddChild(newSun);
            newSun.GlobalPosition = _sunPos;

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

    private void SpawnDevourers(int amount)
    {

    }

    private void SunSpawnCalculator()
    {
        _sunPos = Vector2.FromAngle((float)GD.RandRange(0, Mathf.Tau)) * GD.RandRange(-5000, 5000);
        _sunPos = new Vector2(_sunPos.X, _sunPos.Y);
    }
}
