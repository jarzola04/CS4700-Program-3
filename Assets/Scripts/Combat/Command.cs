using UnityEngine;

public class Command
{
    public string name {get ; private set;}
    public int value {get ; private set;}

    public Character user {get ; private set;}
    public Character target {get; private set;}

    public string useMessage {get; protected set;}
    public string resultMessage {get; protected set;}

    protected string useTemplate;
    protected string resultTemplate;

    public bool success {get; protected set;}
    public bool shakeEffect;

    public Command (string name = "Attack", int value = 1)
    {
        this.name = name;
        this.value = value;
        useTemplate = "{0} attacks!";
        resultTemplate = "The {0}'s Hit Points have been reduced by {1}.";
        useMessage = useTemplate;
        resultMessage = resultTemplate;
    }

    public void SetUser(Character user)
    {
        this.user = user;
    }

    public void SetTarget(Character target)
    {
        this.target = target;
    }

    public virtual void Execute()
    {
        int targetDefense = (int) Mathf.Floor(target.agility / 2);
        int min, max, damage = 0;

        if(user is Enemy)
        {
            if (user.strength > targetDefense)
            {
                min = (user.strength - targetDefense / 2) / 4;
                max = (user.strength - targetDefense / 2) / 2;
                damage = Random.Range(min, max + 1);
                damage = (int)Mathf.Round(damage);
            }
            else
            {
                min = 0;
                max = (user.strength + 4) / 6;
                damage = Random.Range(min, max + 1);
            }
        } 
        else
        {
            min = (user.strength - targetDefense / 2) / 4;
            max = (user.strength - targetDefense / 2) / 2;
            damage = Random.Range(min, max + 1);
            damage = (int)Mathf.Round(damage);

            if(damage < 1)
            {
                damage = Random.Range(0, 100) > 50 ? 1 : 0;
            }
        } 
        int targetHp = target.Hp - damage;
        target.Hp = targetHp;
        useMessage = string.Format(useTemplate, user.name);
        resultMessage = string.Format(resultTemplate, target.name, damage, targetHp);

        success = (damage > 0);      
    }
}

public class CommandEscape : Command
{
    public bool Escaped { get; private set; }

    public CommandEscape() : base("Escape", 0) 
    {
        useTemplate = "{0} started to run away!";
        resultTemplate = "Thou hast escaped safely!";
    }

    public override void Execute()
    {
        useMessage = string.Format(useTemplate, user.name);

        int runChance = CalculateRunChance();
        int roll = Random.Range(0, 256);

        if (roll < runChance)
        {
            Escaped = true;
            resultMessage = "Thou hast escaped safely!";
            success = true;
        }
        else
        {
            Escaped = false;
            resultMessage = "But was blocked in front!";
            success = false;
        }
    }

    private int CalculateRunChance()
    {
        if (target != null)
        {
            int agilityDiff = user.agility - target.agility;
            if (agilityDiff >= 10) return 255;
            if (agilityDiff < 0) return 64;
        }
        return 128;
    }
}
