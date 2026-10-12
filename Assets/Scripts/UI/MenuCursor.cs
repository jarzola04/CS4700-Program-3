using TMPro;
using UnityEngine;

public class MenuCursor
{
    readonly RectTransform cursor;
    readonly CursorBlink blink;

    public MenuCursor(RectTransform cursor, float blinkPeriod, float reappearDelay = 0)
    {
        this.cursor = cursor;
        blink = new CursorBlink(cursor, blinkPeriod, reappearDelay);
    }

    public void PointAt(TMP_Text label, float gap)
    {
        label.ForceMeshUpdate();
        Bounds b = label.textBounds;
        Vector3 leftMid = cursor.parent.InverseTransformPoint(label.transform.TransformPoint(new Vector3(b.min.x, b.center.y, 0)));
        cursor.localPosition = new Vector3(leftMid.x - gap, leftMid.y, cursor.localPosition.z);
        blink.Restart();
    }

    public void Apply(CursorBlink.State state) => blink.Apply(state);
}
