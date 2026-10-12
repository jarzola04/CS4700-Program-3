using System;

// What ITEM and SPELL do in the field. Each shows its message right away and returns the effect to apply
// once that message has been read (null if nothing changes), matching how Dragon Warrior paces it.
public class FieldEffects
{
    readonly FieldCommands commands;
    readonly DarknessManager darkness;
    readonly ItemData torch, magicKey;
    readonly int radiantCost;

    public FieldEffects(FieldCommands commands, DarknessManager darkness, ItemData torch, ItemData magicKey, int radiantCost)
    {
        this.commands = commands;
        this.darkness = darkness;
        this.torch = torch;
        this.magicKey = magicKey;
        this.radiantCost = radiantCost;
    }

    public Action UseItem(ItemData item)
    {
        if (item == torch) return UseTorch();
        if (item == magicKey) return commands.OpenDoor();
        return NothingHappened();
    }

    public Action CastSpell(string spell)
    {
        switch (spell)
        {
            case SpellNames.Radiant: return CastRadiant();
            default: return NothingHappened(); // TODO: the other field spells
        }
    }

    Action UseTorch()
    {
        if (!DarkPlace.IsDark)
        {
            MessageWindow.Show("A torch can be used only in dark places.");
            return null;
        }
        MessageWindow.Show($"{PlayerStats.Name} sparked a torch.");
        return () =>
        {
            InventoryManager.Instance.RemoveItem(torch);
            darkness.UseTorch();
        };
    }

    Action CastRadiant()
    {
        if (PlayerStats.MP < radiantCost)
        {
            MessageWindow.Show("Thy MP is too low.");
            return null;
        }
        MessageWindow.Show($"{PlayerStats.Name} chanted the spell of Radiant.");
        return () =>
        {
            PlayerStats.TrySpendMP(radiantCost);
            if (DarkPlace.IsDark) darkness.CastRadiant(); // outside, MP is spent for nothing
        };
    }

    static Action NothingHappened()
    {
        MessageWindow.Show("But nothing happened."); // Watch what happens when I cast a spell that isn't implemented!
        return null;
    }
}
