using System.Collections.Generic;

// TODO(stats): placeholder for the real player data (items come from InventoryManager).
public static class PlayerStats
{
    public static string Name => "HERO";

    static readonly List<string> spells = new()
    {
        SpellNames.Heal, SpellNames.Hurt, SpellNames.Sleep, SpellNames.Radiant, SpellNames.StopSpell,
        SpellNames.Outside, SpellNames.Return, SpellNames.Repel, SpellNames.HealMore,
    };
    static int mp = 99;

    public static IReadOnlyList<string> Spells => spells;

    public static int Level => 1;
    public static int HP => 15;
    public static int MaxHP => 15;
    public static int MP => mp;
    public static int MaxMP => 99;
    public static int Gold => 120;
    public static int Exp => 0;
    public static int Strength => 4;
    public static int Agility => 4;
    public static int Attack => Strength;    // + weapon
    public static int Defense => Agility / 2; // + armor + shield
    public static string Weapon => "";       // nothing equipped
    public static string Armor => "";
    public static string Shield => "";

    public static bool TrySpendMP(int cost)
    {
        if (mp < cost) return false;
        mp -= cost;
        return true;
    }
}
