using System;
using UnityEngine;

public class BattleEffects
{
    readonly Character user;
    readonly Character target;
    readonly ItemData herb;
    readonly int healCost, healMoreCost, hurtCost, sleepCost;

    public BattleEffects(Character user, Character target, ItemData herb, int healCost = 4, int healMoreCost = 10, int hurtCost = 2, int sleepCost = 2)
    {
        this.user = user;
        this.target = target;
        this.herb = herb;
        this.healCost = healCost;
        this.healMoreCost = healMoreCost;
        this.hurtCost = hurtCost;
        this.sleepCost = sleepCost;
    }

    public Action UseItem(ItemData item)
    {
        if (item == herb) return UseHerb();
        return NothingHappened();
    }

    public Action CastSpell(string spell)
    {
        switch (spell)
        {
            case SpellNames.Heal: return CastHeal();
            case SpellNames.HealMore: return CastHealMore();
            case SpellNames.Hurt: return CastHurt();
            case SpellNames.Sleep: return CastSleep();
            default: return NothingHappened();
        }
    }

    Action UseHerb()
    {
        MessageWindow.Show($"{user.name} used an Herb.");
        return () =>
        {
            InventoryManager.Instance.RemoveItem(herb);
            int healAmount = 25; 
            user.Hp = user.Hp + healAmount; 
        };
    }

    Action CastHeal()
    {
        if (user.Mp < healCost)
        {
            MessageWindow.Show("Thy MP is too low.");
            return null;
        }

        MessageWindow.Show($"{user.name} chanted the spell of Heal.");
        return () =>
        {
            user.Mp -= healCost;
            Restore(SpellNames.Heal, UnityEngine.Random.Range(10, 18));
        };
    }

    Action CastHealMore()
    {
        if (user.Mp < healMoreCost)
        {
            MessageWindow.Show("Thy MP is too low.");
            return null;
        }

        MessageWindow.Show($"{user.name} chanted the spell of HealMore.");
        return () =>
        {
            user.Mp -= healMoreCost;
            Restore(SpellNames.HealMore, UnityEngine.Random.Range(85, 101));
        };
    }

    Action CastHurt()
    {
        if (user.Mp < hurtCost)
        {
            MessageWindow.Show("Thy MP is too low.");
            return null;
        }

        MessageWindow.Show($"{user.name} chanted the spell of Hurt.");
        return () =>
        {
            user.Mp -= hurtCost;
            int damage = UnityEngine.Random.Range(10, 16);
            target.Hp -= damage;
            MessageWindow.Show($"The {SpellNames.Hurt} spell flashes!");
            MessageWindow.Show($"The {target.name}'s Hit Points have been reduced by {damage}.");
        };
    }

    Action CastSleep()
    {
        if (user.Mp < sleepCost)
        {
            MessageWindow.Show("Thy MP is too low.");
            return null;
        }

        MessageWindow.Show($"{user.name} chanted the spell of Sleep.");
        return () =>
        {
            user.Mp -= sleepCost;
            bool hit = UnityEngine.Random.Range(0, 100) < 55;

            if (hit)
            {
                int sleepDuration = UnityEngine.Random.Range(2, 5);
                StatusEffect sleepEffect = new StatusEffect(
                    StatusType.Sleep,
                    sleepDuration,
                    $"{target.name} falls asleep!",
                    $"{target.name} wakes up!"
                );
                target.AddStatus(sleepEffect);
                MessageWindow.Show(sleepEffect.ApplyMessage);
            }
            else
            {
                MessageWindow.Show($"The spell had no effect on {target.name}!");
            }
        };
    }

    void Restore(string spell, int amount)
    {
        int before = user.Hp;
        user.Hp += amount;
        MessageWindow.Show($"The {spell} spell flashes!");
        MessageWindow.Show($"{user.name}'s Hit Points have been restored by {user.Hp - before}.");
    }

    static Action NothingHappened()
    {
        MessageWindow.Show("But nothing happened.");
        return null;
    }
}