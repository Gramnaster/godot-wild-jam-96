using System;
using Godot;

namespace GodotWildJam96;

public partial class MainSun : Sun
{

    [Export] private PackedScene _gameOverScene;

    protected override float InteractionAreaScaleMultiplier => 4.0f;
    protected override int MinPlayerDrainEnergy => 1;

    // Latches once the game-over/win scene change fires, so it fires exactly
    // once instead of every frame the end-state condition remains true.
    private bool _gameEndTriggered;

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
        if (_gameEndTriggered) return;

        if (CurrentEnergy == 0)
        {
            _gameEndTriggered = true;
            GameOver();
        }
        else if (CurrentEnergy == MaxEnergy)
        {
            _gameEndTriggered = true;
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
