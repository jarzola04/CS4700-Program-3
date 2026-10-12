using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// In a dark place, lifts the darkness tilemap in a square around the player (TORCH and RADIANT widen it)
public class DarknessManager : MonoBehaviour
{
    [SerializeField] Tilemap darkness;
    [SerializeField] Transform player;
    [SerializeField, Min(0)] int radius = 0;
    [SerializeField, Min(0)] int torchRadius = 1;   // 3x3
    [SerializeField, Min(0)] int radiantRadius = 3; // 7x7
    [SerializeField, Min(1)] int radiantStepsPerShrink = 60;
    [SerializeField, Min(0f)] float closeInStepTime = 0.08f; // for battle transition
    [SerializeField, Min(0f)] float blackoutHold = 0.3f;

    readonly Dictionary<Vector3Int, TileBase> cleared = new();
    Vector3Int lastCell;
    int lastRadius = -1;
    int radiantSteps; // steps until the RADIANT light shrinks; 0 = not active
                      // technically in Dragon Warriors I think the first ring
                      // has longer lifespan but who cares
    PlayerMovement mover;
    bool closedIn;   // battle transition finished, everything is dark
    int savedRadius;

    public int Radius
    {
        get => radius;
        set => radius = Mathf.Max(0, value);
    }

    public void UseTorch()
    {
        Radius = torchRadius;
        radiantSteps = 0;
    }

    public void CastRadiant()
    {
        Radius = radiantRadius;
        radiantSteps = radiantStepsPerShrink;
    }

    public IEnumerator CloseIn() // battle transition
    {
        savedRadius = radius;
        for (int r = radius - 1; r >= 0; r--)
        {
            radius = r;
            yield return new WaitForSeconds(closeInStepTime);
        }
        closedIn = true;
        Restore();
        yield return new WaitForSeconds(blackoutHold);
    }

    // Undoes CloseIn once the battle is over
    public void Reopen()
    {
        if (!closedIn) return;
        closedIn = false;
        radius = savedRadius;
        lastRadius = -1;
    }

    void OnEnable()
    {
        DarkPlace.Changed += OnDarkChanged;
        OnDarkChanged(DarkPlace.IsDark);
        mover = player.GetComponent<PlayerMovement>();
        if (mover != null) mover.Stepped += OnStepped;
    }

    void OnDisable()
    {
        DarkPlace.Changed -= OnDarkChanged;
        if (mover != null) mover.Stepped -= OnStepped;
    }

    void OnDarkChanged(bool dark) // this function is never gonna be used lol
    {
        darkness.gameObject.SetActive(dark);
        Restore();
        lastRadius = -1; // force a refresh on the next frame
        if (!dark)
        {
            // Light doesn't carry over between dark places
            Radius = 0;
            radiantSteps = 0;
        }
    }

    // RADIANT fades one ring every radiantStepsPerShrink steps, down to torch size
    void OnStepped()
    {
        if (radiantSteps == 0 || --radiantSteps > 0) return;
        Radius--;
        radiantSteps = Radius > torchRadius ? radiantStepsPerShrink : 0;
    }

    void LateUpdate()
    {
        if (!DarkPlace.IsDark || closedIn) return;

        Vector3Int cell = darkness.WorldToCell(player.position);
        if (cell == lastCell && radius == lastRadius) return;

        Restore();
        for (int x = -radius; x <= radius; x++)
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int c = cell + new Vector3Int(x, y, 0);
                TileBase t = darkness.GetTile(c);
                if (t == null) continue;
                cleared[c] = t;
                darkness.SetTile(c, null);
            }

        lastCell = cell;
        lastRadius = radius;
    }

    void Restore()
    {
        foreach (var kv in cleared) darkness.SetTile(kv.Key, kv.Value);
        cleared.Clear();
    }
}
