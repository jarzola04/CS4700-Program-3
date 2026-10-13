using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// TODO(stats): placeholder for the real player data (items come from InventoryManager).
public static class PlayerStats
{
    public static string Name => "HERO";

    static readonly List<string> spells = new()
    {
        SpellNames.Heal, SpellNames.Hurt, SpellNames.Sleep, SpellNames.Radiant, SpellNames.StopSpell,
        SpellNames.Outside, SpellNames.Return, SpellNames.Repel, SpellNames.HealMore,
    };
    static int hp = 120;
    static int mp = 80;
    static int gold = 120;
    static int exp = 0;

    public static IReadOnlyList<string> Spells => spells;

    public static int Level => 17;
    public static int HP => hp;
    public static int MaxHP => 120;
    public static int MP => mp;
    public static int MaxMP => 80;
    public static int Gold => gold;
    public static int Exp => exp;
    public static int Strength => 72;
    public static int Agility => 66;
    public static int Attack => Strength;
    public static int Defense => Agility / 2;
    public static string Weapon => "White Sword";
    public static string Armor => "Purple";
    public static string Shield => "Purple Too";

    public static void AddGold(int amount) => gold += amount;
    public static void AddExp(int amount) => exp += amount;

    // Returns how much was actually restored
    public static int Heal(int amount)
    {
        int before = hp;
        hp = System.Math.Min(hp + amount, MaxHP);
        return hp - before;
    }

    // Carries HP / MP out of a battle
    public static void SetVitals(int newHp, int newMp)
    {
        hp = System.Math.Clamp(newHp, 1, MaxHP);
        mp = System.Math.Clamp(newMp, 0, MaxMP);
    }

    public static void Restore()
    {
        hp = MaxHP;
        mp = MaxMP;
    }

    // Full heal whenever the SwampCave scene starts
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void HookSceneLoad()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SwampCave") Restore();
    }

    public static bool TrySpendMP(int cost)
    {
        if (mp < cost) return false;
        mp -= cost;
        return true;
    }
}
