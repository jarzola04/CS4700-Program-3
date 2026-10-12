using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageWindow : MonoBehaviour
{
    public static MessageWindow Instance { get; private set; }

    [SerializeField] GameObject window;
    [SerializeField] TMP_Text text;
    [SerializeField] float charsPerSecond = 30;
    [SerializeField] RectTransform moreCursor; // optional; visual
    [SerializeField] float moreCursorGap = 20;
    [SerializeField] float blinkPeriod = 0.5f;  // seconds per cursor on/off cycle

    readonly Queue<string> pending = new(); // messages waiting for a press
    float shown; // characters revealed so far
    int total;   // characters in the window's text
    CursorBlink moreBlink;

    // True while the window is up, includes close animation
    public static bool Busy => Instance != null && Instance.window.activeSelf;

    void Awake()
    {
        Instance = this;
        window.SetActive(false);
        if (moreCursor != null) moreBlink = new CursorBlink(moreCursor, blinkPeriod);
        UpdateMoreCursor(false);
    }

    public static void Show(string message)
    {
        if (Instance == null)
        {
            Debug.Log(message);
            return;
        }
        Instance.Open(message);
    }

    void Open(string message)
    {
        if (WindowAnim.IsShown(window))
        {
            pending.Enqueue(message);
            return;
        }
        text.text = "";
        shown = 0;
        window.transform.SetAsLastSibling();
        WindowAnim.Open(window);
        Append(message);
    }

    void Append(string message)
    {
        text.text = text.text.Length == 0 ? message : text.text + "\n" + message;
        text.ForceMeshUpdate();
        total = text.textInfo.characterCount;
        text.maxVisibleCharacters = (int)shown;
    }

    void Update()
    {
        if (!WindowAnim.IsShown(window)) return;

        bool typing = shown < total;
        if (typing)
        {
            shown = Mathf.Min(total, shown + charsPerSecond * Time.deltaTime);
            text.maxVisibleCharacters = (int)shown;
        }

        UpdateMoreCursor(shown >= total && pending.Count > 0);

        if (!UIController.Submit && !UIController.Cancel) return;
        UIController.UsePress();

        if (typing)
        {
            shown = total;
            text.maxVisibleCharacters = total;
        }
        else if (pending.Count > 0)
        {
            Append(pending.Dequeue());
            UpdateMoreCursor(false);
        }
        else WindowAnim.Close(window);
    }

    void UpdateMoreCursor(bool waiting)
    {
        if (moreBlink == null) return;
        moreBlink.Apply(waiting ? CursorBlink.State.Blink : CursorBlink.State.Hidden);
        if (!waiting) return;

        TMP_TextInfo info = text.textInfo;
        float bottom = info.lineCount > 0 ? info.lineInfo[info.lineCount - 1].descender : text.rectTransform.rect.yMin;
        Vector3 under = moreCursor.parent.InverseTransformPoint(text.transform.TransformPoint(new Vector3(0, bottom, 0)));
        RectTransform box = (RectTransform)window.transform;
        Vector3 center = moreCursor.parent.InverseTransformPoint(box.TransformPoint(box.rect.center));
        moreCursor.localPosition = new Vector3(center.x, under.y - moreCursorGap, moreCursor.localPosition.z);
    }
}
