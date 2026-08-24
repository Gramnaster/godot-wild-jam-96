using System;
using Godot;

namespace GodotWildJam96;

// Owns the four directional thruster effect sprites: which animation each
// plays, the start-clip-to-continuous-clip handoff, and the rule that the
// main thruster's power-burn animation takes priority over its own standard
// clip while power-thrusting. Godot-coupled (drives AnimatedSprite2D), so
// this collaborator is not unit-tested -- see NearestTarget/SpawnPlacement
// for the GD.*-free collaborators that are.
public sealed class ThrusterAnimator
{
    private const string AnimMainThrustStart = "MainThrustStart";
    private const string AnimMainThrustContinuous = "MainThrustContinuous";
    private const string AnimMainThrustPower = "MainThrustPower";

    private const string AnimThrustForwardStart = "ThrustForward";
    private const string AnimThrustForwardContinuous = "ThrustForwardContinuous";

    private const string AnimThrustLeftStart = "ThrustLeft";
    private const string AnimThrustLeftContinuous = "ThrustLeftContinuous";

    private const string AnimThrustRightStart = "ThrustRight";
    private const string AnimThrustRightContinuous = "ThrustRightContinuous";

    private readonly AnimatedSprite2D _mainSprite;
    private readonly Thruster[] _thrusters;

    private sealed class Thruster(
        AnimatedSprite2D sprite,
        string actionName,
        string startAnimation,
        string continuousAnimation)
    {
        public readonly AnimatedSprite2D Sprite = sprite;
        public readonly string ActionName = actionName;
        public readonly string StartAnimation = startAnimation;
        public readonly string ContinuousAnimation = continuousAnimation;
        public bool WasActive;
        public Action AnimationFinishedHandler;
    }

    public ThrusterAnimator(
        AnimatedSprite2D mainSprite,
        AnimatedSprite2D forwardSprite,
        AnimatedSprite2D leftSprite,
        AnimatedSprite2D rightSprite)
    {
        _mainSprite = mainSprite;
        _thrusters =
        [
            new Thruster(mainSprite, "move_up", AnimMainThrustStart, AnimMainThrustContinuous),
            new Thruster(forwardSprite, "move_down", AnimThrustForwardStart, AnimThrustForwardContinuous),
            new Thruster(leftSprite, "move_left", AnimThrustLeftStart, AnimThrustLeftContinuous),
            new Thruster(rightSprite, "move_right", AnimThrustRightStart, AnimThrustRightContinuous),
        ];

        // Each thruster has its own AnimationFinished subscription so it
        // can pass itself from start clip to continuous.
        foreach (Thruster thruster in _thrusters)
        {
            thruster.Sprite.Hide();
            Thruster eventThruster = thruster;
            eventThruster.AnimationFinishedHandler = () => OnThrusterAnimationFinished(eventThruster);
            eventThruster.Sprite.AnimationFinished += eventThruster.AnimationFinishedHandler;
        }
    }

    public void Unsubscribe()
    {
        foreach (Thruster thruster in _thrusters)
        {
            thruster.Sprite.AnimationFinished -= thruster.AnimationFinishedHandler;
        }
    }

    public void UpdateAnimations(bool isPowerThrusting)
    {
        foreach (Thruster thruster in _thrusters)
        {
            bool active = Input.IsActionPressed(thruster.ActionName);

            // Std Main thruster gets a higher-priority over Power Main thruster
            if (thruster.Sprite == _mainSprite && isPowerThrusting)
            {
                thruster.Sprite.Show();

                if (thruster.Sprite.Animation != AnimMainThrustPower)
                {
                    thruster.Sprite.Play(AnimMainThrustPower);
                }

                thruster.WasActive = true;
                continue;
            }

            // Standard thruster handling
            if (active)
            {
                // Because they're all hidden at construction
                thruster.Sprite.Show();

                if (!thruster.WasActive || !thruster.Sprite.IsPlaying())
                {
                    thruster.Sprite.Play(thruster.StartAnimation);
                }
            }
            else
            {
                thruster.Sprite.Stop();
                thruster.Sprite.Hide();
            }

            thruster.WasActive = active;
        }
    }

    private static void OnThrusterAnimationFinished(Thruster thruster)
    {
        bool active = Input.IsActionPressed(thruster.ActionName);

        if (active && thruster.Sprite.Animation == thruster.StartAnimation)
        {
            thruster.Sprite.Play(thruster.ContinuousAnimation);
        }
    }
}
