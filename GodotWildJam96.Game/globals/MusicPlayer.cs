using Godot;

namespace GodotWildJam96;

public partial class MusicPlayer : AudioStreamPlayer
{

    public static MusicPlayer Instance { get; private set; }

    public override void _Ready()
    {
        if (Instance is not null)
        {
            QueueFree();
            return;
        }

        Instance = this;
        Stream = GD.Load<AudioStream>("res://assets/backgrounds/MainBGM.mp3");
    }

    public void PlayMainTheme()
    {
        if (!Playing)
        {
            Play();
        }
    }
}
