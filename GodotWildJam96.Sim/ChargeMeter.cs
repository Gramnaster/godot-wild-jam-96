using System;

namespace GodotWildJam96.Sim;

// Held-milliseconds -> clamped 0-1 charge ratio. Caller supplies both
// timestamps (Time.GetTicksMsec()) so this stays Godot-engine-free.
public sealed class ChargeMeter(float maxChargeSeconds)
{
    private readonly float _maxChargeSeconds = maxChargeSeconds;
    private ulong _pressedAtMsec;

    public void Press(ulong nowMsec)
    {
        _pressedAtMsec = nowMsec;
    }

    public float Release(ulong nowMsec)
    {
        float heldSeconds = (nowMsec - _pressedAtMsec) / 1000f;
        return Math.Clamp(heldSeconds / _maxChargeSeconds, 0f, 1f);
    }
}
