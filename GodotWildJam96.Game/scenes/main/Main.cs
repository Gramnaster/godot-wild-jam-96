using System;
using Godot;

namespace GodotWildJam96;

public partial class Main : Control
{
    [Export] public PackedScene _sunScene;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        //Spawning a random numer of suns on game start
        int numSuns = (int)GD.RandRange(5, 20);
        for (int i = 0; i < numSuns; i++)
        {
            SpawnSuns();
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void SpawnSuns()
    {
        Sun newSun = _sunScene.Instantiate<Sun>();
        Rect2 vpr = GetViewportRect();
        //Here can substitute this code for a SpawnRangeCalculator() function that will calculate a random position
        //for the next sun based on specific inputs of current/last spawned sun
        Vector2 newPos = new Vector2((float)GD.RandRange(0, vpr.Size.X), (float)GD.RandRange(0, vpr.Size.Y));
        //Setting the instantiated sun's position to the newPos calculated above
        newSun.Position = newPos;
        //Instantiating the new sun as a child of the Main scene so it will be visible in the game
        AddChild(newSun);
    }

    public void SpawnRangeCalculator()
    {

    }
}
