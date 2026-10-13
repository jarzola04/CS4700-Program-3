using TMPro;
using UnityEngine;

// The small LV/HP/MP/G/E window that appears while the player stands still or a menu is up,
// plus full STATUS window from COMMAND
public class StatusWindow : MonoBehaviour
{
    public static StatusWindow Instance { get; private set; }

    [SerializeField] PlayerMovement player;
    [SerializeField] float idleDelay = 1;

    [SerializeField] GameObject mini, full;
    [SerializeField] TMP_Text miniName, miniLabels, miniValues, fullLabels, fullValues;

    float still; // seconds since the player last moved

    // True while the window is up, includes close animation
    public static bool Busy => Instance != null && Instance.full.activeSelf;

    void Awake()
    {
        Instance = this;
        mini.SetActive(false);
        full.SetActive(false);
    }

    public static void ShowFull()
    {
        if (Instance == null) return;
        Instance.fullLabels.text = "NAME:\nSTRENGTH:\nAGILITY:\nMAXIMUM HP:\nMAXIMUM MP:\nATTACK POWER:\nDEFENSE POWER:\nWEAPON:\nARMOR:\nSHIELD:";
        Instance.fullValues.text =
            $"{PlayerStats.Name}\n{PlayerStats.Strength}\n{PlayerStats.Agility}\n{PlayerStats.MaxHP}\n{PlayerStats.MaxMP}\n" +
            $"{PlayerStats.Attack}\n{PlayerStats.Defense}\n{PlayerStats.Weapon}\n{PlayerStats.Armor}\n{PlayerStats.Shield}";
        Instance.full.transform.SetAsLastSibling(); // draw over the command windows
        WindowAnim.Open(Instance.full);
    }

    void Update()
    {
        still = player.IsMoving ? 0 : still + Time.deltaTime;
        bool showMini = still >= idleDelay || UIController.Blocking;
        if (showMini) RefreshMini(); // values can change while it's up (e.g. MP after a spell)
        if (WindowAnim.IsShown(mini) != showMini)
        {
            if (showMini) WindowAnim.Open(mini);
            else WindowAnim.Close(mini);
        }

        if (!WindowAnim.IsShown(full)) return;
        if (UIController.Submit || UIController.Cancel)
        {
            UIController.UsePress();
            WindowAnim.Close(full);
        }
    }

    void RefreshMini()
    {
        miniName.text = PlayerStats.Name;
        miniLabels.text = "\nLV\nHP\nMP\nG\nE";
        miniValues.text = $"\n{PlayerStats.Level}\n{PlayerStats.HP}\n{PlayerStats.MP}\n{PlayerStats.Gold}\n{PlayerStats.Exp}";
    }
}
