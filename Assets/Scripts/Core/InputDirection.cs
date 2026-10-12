using UnityEngine;

public static class InputDirection
{
    // used this a few times so I abstracted it
    public static Vector2Int Cardinal(Vector2 v)
    {
        if (v.y > 0) return Vector2Int.up;
        if (v.y < 0) return Vector2Int.down;
        if (v.x < 0) return Vector2Int.left;
        if (v.x > 0) return Vector2Int.right;
        return Vector2Int.zero;
    }
}
