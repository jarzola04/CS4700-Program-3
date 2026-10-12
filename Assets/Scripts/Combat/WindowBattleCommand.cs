using TMPro;
using UnityEngine;

// FIGHT / RUN / SPELL / ITEM grid, drawn like the overworld CommandMenu
public class WindowBattleCommand : WindowBase
{
    [SerializeField] private GameObject window;
    [SerializeField] private RectTransform cursor;
    [SerializeField] private RectTransform[] slots; // column-major: FIGHT, RUN, SPELL, ITEM
    [SerializeField] private int rows = 2;
    [SerializeField] private float cursorGap = 4;
    [SerializeField] private float blinkPeriod = 0.5f;

    private int row, col;
    private Vector2Int lastDir;
    private MenuCursor menuCursor;

    private MenuCursor Pointer => menuCursor ??= new MenuCursor(cursor, blinkPeriod);

    public int current => col * rows + row;

    private void Awake()
    {
        window.SetActive(false);
    }

    public override void Initialize()
    {
        base.Initialize();
        row = col = 0;
        lastDir = InputDirection.Cardinal(UIController.Navigate);
    }

    public override void Open()
    {
        Initialize();
        window.transform.SetAsLastSibling();
        WindowAnim.Open(window);
        MoveCursor();
    }

    public override void Close()
    {
        WindowAnim.Close(window);
    }

    public void ManualUpdate()
    {
        Vector2Int dir = InputDirection.Cardinal(UIController.Navigate);
        if (dir != lastDir && dir != Vector2Int.zero)
        {
            int cols = (slots.Length + rows - 1) / rows;
            if (dir.y != 0) row = (row - dir.y + rows) % rows; // up = -row
            else col = (col + dir.x + cols) % cols;
            MoveCursor();
        }
        lastDir = dir;
    }

    private void LateUpdate()
    {
        if (!WindowAnim.IsShown(window)) return;
        Pointer.Apply(MessageWindow.Typing ? CursorBlink.State.Solid : CursorBlink.State.Blink);
    }

    private void MoveCursor() => Pointer.PointAt(slots[current].GetComponent<TMP_Text>(), cursorGap);
}
