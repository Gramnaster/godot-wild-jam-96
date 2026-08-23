using Godot;
using GodotWildJam96;
using System;

public partial class MainSun : Sun
{

    [Export] private PackedScene _gameOverScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        base._Ready();
        _maxEnergy = 15;
        _currentEnergy = 3;
        _energyValuebar.InitializeValues(_maxEnergy, _currentEnergy);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (_currentEnergy == 0)
        {
            GameOver();
        }
        else if (_currentEnergy == _maxEnergy)
        {
            WinTheGame();
        }
	}

    private void GameOver()
    {
        GetTree().ChangeSceneToFile($"res://scenes/GameOverScreen/GameOverScreen.tscn");
    }

    private void WinTheGame()
    {
        GetTree().ChangeSceneToFile($"res://scenes/VictoryScreen/VictoryScreen.tscn");
    }
}
