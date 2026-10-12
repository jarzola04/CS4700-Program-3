using UnityEngine;
using UnityEngine.InputSystem;

// Which field UI is up, and the menu input shared by every window.
// The player can't walk while any UI is up, and the command menu waits on windows it opened.
public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [SerializeField] InputActionReference navigate, submit, cancel;

    int usedFrame = -1;

    public bool MenuOpen { get; set; }

    // A message or the full STATUS window is up
    public static bool WindowOpen => MessageWindow.Busy || StatusWindow.Busy;

    // Anything the player has to wait on
    public static bool Blocking => (Instance != null && Instance.MenuOpen) || WindowOpen || Battle.Active;

    public static Vector2 Navigate => Instance.navigate.action.ReadValue<Vector2>();

    public static bool Submit => Unused && Instance.submit.action.WasPressedThisFrame();
    public static bool Cancel => Unused && Instance.cancel.action.WasPressedThisFrame();

    public static void UsePress() => Instance.usedFrame = Time.frameCount;

    static bool Unused => Instance.usedFrame != Time.frameCount;

    void Awake()
    {
        if (Instance == null) Instance = this; // only one active at a time
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
