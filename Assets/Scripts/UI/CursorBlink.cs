using UnityEngine;
using UnityEngine.UI;

public class CursorBlink
{
    public enum State { Blink, Solid, Hidden }

    readonly Graphic graphic;
    readonly float period;         // seconds for one on/off cycle
    readonly float reappearDelay;  // seconds kept off when coming back from Hidden
    float since;                   // start of the current cycle; reset so the cursor always comes back lit
    State last = State.Blink;

    public CursorBlink(Component cursor, float period, float reappearDelay = 0)
    {
        graphic = cursor.GetComponent<Graphic>();
        this.period = period;
        this.reappearDelay = reappearDelay;
    }

    // Restart the cycle lit right away, e.g. after the cursor moves or the menu opens
    public void Restart()
    {
        since = Time.unscaledTime;
        last = State.Blink;
    }

    public void Apply(State state)
    {
        if (graphic == null) return;
        if (state == State.Blink && last != State.Blink)
            since = Time.unscaledTime + (last == State.Hidden ? reappearDelay : 0);
        last = state;
        float t = Time.unscaledTime - since;
        graphic.enabled = state switch
        {
            State.Blink => t >= 0 && t % period < period / 2,
            State.Solid => true,
            _ => false,
        };
    }
}
