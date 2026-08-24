using System;
using Godot;

namespace GodotWildJam96;

public partial class MainSun : Sun
{

    [Export] private PackedScene _gameOverScene;

    protected override float InteractionAreaScaleMultiplier => 4.0f;
    protected override int MinPlayerDrainEnergy => 1;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        MaxEnergy = 15;
        CurrentEnergy = 3;
        EnergyValuebar.InitializeValues(MaxEnergy, CurrentEnergy);
        UpdateInteractionAreaScale();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (CurrentEnergy == 0)
        {
            GameOver();
        }
        else if (CurrentEnergy == MaxEnergy)
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
