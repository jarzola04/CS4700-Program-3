using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ListMenu : MonoBehaviour
{
    [SerializeField] GameObject window;
    [SerializeField] RectTransform cursor;
    [SerializeField] Vector2 firstRow = new(-165, 220);
    [SerializeField] float rowSpacing = 80;
    [SerializeField] float cursorGap = 30;
    [SerializeField] float blinkPeriod = 0.5f; // seconds per cursor on+off cycle

    readonly List<TMP_Text> rows = new();
    readonly List<string> entries = new();
    int index;
    MenuCursor menuCursor;

    // Created on first use: Open can run before this object has ever been active
    MenuCursor Pointer => menuCursor ??= new MenuCursor(cursor, blinkPeriod);

    public int SelectedIndex => index;
    public string Selected => entries[index];

    public void Open(IReadOnlyList<string> source)
    {
        entries.Clear();
        entries.AddRange(source);

        for (int i = 0; i < entries.Count; i++) Row(i).text = entries[i];
        for (int i = 0; i < rows.Count; i++) rows[i].gameObject.SetActive(i < entries.Count);

        index = 0;
        transform.SetAsLastSibling(); // draw over the command window
        WindowAnim.Open(window);
        MoveCursor();
    }

    public void Close() => WindowAnim.Close(window);

    public void Step(int delta)
    {
        index = (index + delta + entries.Count) % entries.Count;
        MoveCursor();
    }

    // Solid while a message from the chosen entry is up, flashing otherwise
    void LateUpdate()
    {
        if (!WindowAnim.IsShown(window)) return;
        Pointer.Apply(MessageWindow.Busy ? CursorBlink.State.Solid : CursorBlink.State.Blink);
    }

    void MoveCursor() => Pointer.PointAt(rows[index], cursorGap);

    // Layout is otherwise only applied when rows are created / the cursor moves, so re-apply it for live tuning
    void OnValidate()
    {
        if (!Application.isPlaying) return;
        for (int i = 0; i < rows.Count; i++) rows[i].rectTransform.anchoredPosition = RowPosition(i);
        if (window.activeSelf && entries.Count > 0) MoveCursor();
    }

    Vector2 RowPosition(int i) => firstRow + Vector2.down * (rowSpacing * i);

    // Rows are created on demand and reused on later opens
    TMP_Text Row(int i)
    {
        TMP_Text style = GetComponent<TMP_Text>();
        while (rows.Count <= i)
        {
            var rt = new GameObject($"Row{rows.Count}", typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(window.transform, false);
            rt.pivot = new Vector2(0, 0.5f); // anchoredPosition is the row's left edge
            rt.sizeDelta = style.rectTransform.sizeDelta;
            Vector3 ws = window.transform.localScale;
            rt.localScale = new Vector3(1 / ws.x, 1 / ws.y, 1); // undo the window's squash so rows match the command labels
            rt.anchoredPosition = RowPosition(rows.Count);

            var label = rt.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = style.font;
            label.fontSize = style.fontSize;
            label.color = style.color;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            rows.Add(label);
        }
        return rows[i];
    }
}
