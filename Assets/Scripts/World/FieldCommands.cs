using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FieldCommands
{
    readonly PlayerMovement player;
    readonly Tilemap floor;
    readonly TileBase openChestTile; // replaces a chest tile once taken
    readonly ItemData magicKey;

    public FieldCommands(PlayerMovement player, Tilemap floor, TileBase openChestTile, ItemData magicKey)
    {
        this.player = player;
        this.floor = floor;
        this.openChestTile = openChestTile;
        this.magicKey = magicKey;
    }

    Vector3Int PlayerCell => floor.WorldToCell(player.transform.position);

    static bool IsTile(TileBase tile, string prefix) => tile != null && tile.name.StartsWith(prefix, StringComparison.Ordinal);

    public void Talk()
    {
        if (!IsTile(player.Walls.GetTile(player.FacingCell), "princess"))
        {
            MessageWindow.Show("There is no one there.");
            return;
        }
        MessageWindow.Show("Princess Gwaelin: WOOF WOOF WOOF BARK BARK BARK WOOF WOOF!!");
        MessageWindow.Show("WOOF WOOF BARK WOOF WOOF BARK BARK BARK WOOF WOOF!!!");
        MessageWindow.Show("(You win the game!)");
    }

    public void Search()
    {
        MessageWindow.Show($"{PlayerStats.Name} searched the ground all about.");
        MessageWindow.Show("But there found nothing.");
    }

    // Stairs end the game for now
    public void TakeStairs()
    {
        if (!IsTile(floor.GetTile(PlayerCell), "stairs"))
        {
            MessageWindow.Show("There are no stairs here.");
            return;
        }
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void TakeChest()
    {
        Vector3Int cell = PlayerCell;
        if (!IsTile(floor.GetTile(cell), "chest"))
        {
            MessageWindow.Show("There is nothing to take here.");
            return;
        }
        if (InventoryManager.Instance == null || !InventoryManager.Instance.AddItem(magicKey))
        {
            MessageWindow.Show("Thou cannot carry any more items.");
            return;
        }
        floor.SetTile(cell, openChestTile);
        MessageWindow.Show($"{PlayerStats.Name} found a {magicKey.ItemName}.");
    }

    public Action OpenDoor() // the only door in swamp cave is locked
    {
        Tilemap walls = player.Walls;
        Vector3Int cell = player.FacingCell;
        if (!IsTile(walls.GetTile(cell), "door"))
        {
            MessageWindow.Show("There is no door here.");
            return null;
        }
        if (InventoryManager.Instance == null || !InventoryManager.Instance.Has(magicKey))
        {
            MessageWindow.Show("Thou hast no key.");
            return null;
        }
        MessageWindow.Show($"{PlayerStats.Name} unlocked the door.");
        return () =>
        {
            InventoryManager.Instance.RemoveItem(magicKey);
            walls.SetTile(cell, null);
        };
    }
}
