using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CommandMenu : MonoBehaviour
{
    [SerializeField] GameObject window;
    [SerializeField] RectTransform cursor;
    [SerializeField] RectTransform[] slots; // TALK, STATUS, STAIRS, SEARCH, SPELL, ITEM, DOOR, TAKE
    [SerializeField] PlayerMovement player;
    [SerializeField] Tilemap floor;
    [SerializeField] ListMenu itemMenu, spellMenu;
    [SerializeField] DarknessManager darkness;
    [SerializeField] ItemData magicKey; // given by TAKE on a chest tile
    [SerializeField] ItemData torch;
    [SerializeField] TileBase openChestTile; // replaces the chest tile once taken
    [SerializeField] int radiantCost = 3;
    [SerializeField] float cursorGap = 4;
    [SerializeField] float blinkPeriod = 0.5f; // seconds per cursor on/off cycle
    [SerializeField] float reappearDelay = 0.3f; // seconds the cursor stays off after STATUS / SPELL / ITEM closes
    [SerializeField] AudioSource sfx;
    [SerializeField] AudioClip menuSfx;

    const int Rows = 4;
    const int Talk = 0, Status = 1, Stairs = 2, Search = 3, Spell = 4, Item = 5, Door = 6, Take = 7;
    const float DeadZone = 0.5f;

    int row, col;
    Vector2Int lastDir;
    ListMenu sub; // current submenu
    Action pendingEffect;
    bool closingDue;
    MenuCursor menuCursor;
    FieldCommands commands;
    FieldEffects effects;

    void Awake()
    {
        menuCursor = new MenuCursor(cursor, blinkPeriod, reappearDelay);
        commands = new FieldCommands(player, floor, openChestTile, magicKey);
        effects = new FieldEffects(commands, darkness, torch, magicKey, radiantCost);
    }

    void Update()
    {
        if (UIController.WindowOpen) return;
        if (closingDue)
        {
            closingDue = false;
            var effect = pendingEffect;
            pendingEffect = null;
            Close();
            effect?.Invoke();
            return;
        }
        if (UIController.Instance.MenuOpen) HandleMenu();
        else if (UIController.Submit && !player.IsMoving)
        {
            UIController.UsePress();
            Open();
        }
    }

    // After Update so it's after window opening
    void LateUpdate()
    {
        if (!UIController.Instance.MenuOpen) return;
        menuCursor.Apply(sub != null || StatusWindow.Busy ? CursorBlink.State.Hidden
            : MessageWindow.Busy ? CursorBlink.State.Solid
            : CursorBlink.State.Blink);
    }

    void Open()
    {
        sfx.PlayOneShot(menuSfx);
        row = col = 0;
        lastDir = InputDirection.Cardinal(UIController.Navigate);;
        WindowAnim.Open(window);
        UIController.Instance.MenuOpen = true;
        MoveCursor();
    }

    void Close()
    {
        CloseSub();
        WindowAnim.Close(window);
        UIController.Instance.MenuOpen = false;
    }

    void HandleMenu()
    {
        if (sub != null)
        {
            HandleSub();
            return;
        }

        Vector2Int dir = InputDirection.Cardinal(UIController.Navigate);
        if (dir != lastDir && dir != Vector2Int.zero)
        {
            if (dir.y != 0) row = (row - dir.y + Rows) % Rows; // up = -row
            else col = 1 - col;
            MoveCursor();
        }
        lastDir = dir;

        if (UIController.Cancel)
        {
            UIController.UsePress();
            Close();
        }
        else if (UIController.Submit)
        {
            UIController.UsePress();
            sfx.PlayOneShot(menuSfx);
            Execute(col * Rows + row);
            closingDue = sub == null && UIController.Instance.MenuOpen && UIController.WindowOpen;
        }
    }

    void HandleSub()
    {
        Vector2Int dir = InputDirection.Cardinal(UIController.Navigate);;
        if (dir != lastDir && dir.y != 0) sub.Step(-dir.y); // up = -index
        lastDir = dir;

        if (UIController.Cancel)
        {
            UIController.UsePress();
            CloseSub();
        }
        else if (UIController.Submit)
        {
            UIController.UsePress();
            sfx.PlayOneShot(menuSfx);
            Action effect = sub == itemMenu
                ? effects.UseItem(InventoryManager.Instance.Slots[sub.SelectedIndex].Item)
                : effects.CastSpell(sub.Selected);
            closingDue = UIController.WindowOpen;
            if (closingDue) pendingEffect = effect;
            else effect?.Invoke();
        }
    }

    void MoveCursor() => menuCursor.PointAt(slots[col * Rows + row].GetComponent<TMP_Text>(), cursorGap);

    void Execute(int i)
    {
        switch (i)
        {
            case Talk: commands.Talk(); break;
            case Status: StatusWindow.ShowFull(); break;
            case Stairs: commands.TakeStairs(); break;
            case Search: commands.Search(); break;
            case Spell: OpenSub(spellMenu, PlayerStats.Spells, "Thou hast not yet learned any spell."); break;
            case Item: OpenSub(itemMenu, ItemNames(), "Nothing of use has yet been given to thee."); break;
            case Door: commands.OpenDoor()?.Invoke(); break;
            case Take: commands.TakeChest(); break;
        }
    }

    void OpenSub(ListMenu menu, IReadOnlyList<string> entries, string emptyMessage)
    {
        if (entries.Count == 0)
        {
            MessageWindow.Show(emptyMessage);
            return;
        }
        sub = menu;
        sub.Open(entries);
    }

    void CloseSub()
    {
        if (sub == null) return;
        sub.Close();
        sub = null;
    }

    static List<string> ItemNames()
    {
        var names = new List<string>();
        if (InventoryManager.Instance == null) return names;
        foreach (var slot in InventoryManager.Instance.Slots) names.Add(slot.Item.ItemName);
        return names;
    }
}
