using UnityEngine;
using UnityEngine.UI;

public class WindowAnim : MonoBehaviour
{
    const float UnitsPerSecond = 800; // canvas units revealed per second
    const float MaxFrameTime = 0.05f;

    Image image;
    Mask mask;
    Image.Type imageType;
    Image.FillMethod fillMethod;
    int fillOrigin;
    float fillAmount;
    bool ready, animating;
    float height;
    float progress; // 0 = hidden, 1 = fully drawn
    int dir;        // +1 opening, -1 closing

    public bool Shown => gameObject.activeSelf && !(animating && dir < 0);

    public static bool IsShown(GameObject window) => window.TryGetComponent(out WindowAnim a) ? a.Shown : window.activeSelf;

    public static void Open(GameObject window) => Of(window).Begin(1);

    public static void Close(GameObject window)
    {
        if (window.activeSelf) Of(window).Begin(-1);
    }

    static WindowAnim Of(GameObject window) => window.TryGetComponent(out WindowAnim a) ? a : window.AddComponent<WindowAnim>();

    void Setup()
    {
        if (ready) return;
        ready = true;
        if (!TryGetComponent(out image)) return;
        imageType = image.type;
        fillMethod = image.fillMethod;
        fillOrigin = image.fillOrigin;
        fillAmount = image.fillAmount;
        mask = gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = true;
        mask.enabled = false;
    }

    void Begin(int direction)
    {
        Setup();
        height = Mathf.Max(1, ((RectTransform)transform).rect.height * transform.lossyScale.y);
        if (direction > 0)
        {
            if (Shown) return;
            if (!gameObject.activeSelf) progress = 0;
            gameObject.SetActive(true);
        }
        else if (!Shown) return;
        dir = direction;
        animating = true;
    }

    void LateUpdate()
    {
        if (!animating) return;
        progress += dir * Mathf.Min(Time.unscaledDeltaTime, MaxFrameTime) * UnitsPerSecond / height;
        if (progress <= 0)
        {
            Finish();
            if (dir < 0) gameObject.SetActive(false);
        }
        else if (progress >= 1) Finish();
        else Apply(Mathf.Round(progress * height) / height);
    }

    void Apply(float shown)
    {
        if (image == null) return;
        mask.enabled = true;
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Vertical;
        image.fillOrigin = (int)Image.OriginVertical.Top;
        image.fillAmount = shown;
    }

    void Finish()
    {
        animating = false;
        progress = Mathf.Clamp01(progress);
        if (image == null) return;
        mask.enabled = false;
        image.type = imageType;
        image.fillMethod = fillMethod;
        image.fillOrigin = fillOrigin;
        image.fillAmount = fillAmount;
    }

    void OnDisable()
    {
        bool closing = animating && dir < 0;
        if (animating) Finish();
        progress = 0;
        if (closing) gameObject.SetActive(false);
    }
}
