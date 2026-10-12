using UnityEngine;

public class Command
{
    public string name {get ; private set;}
    public int value {get ; private set;}

    public Character user {get ; private set;}
    public Character target {get; private set;}

    public string useMessage {get; protected set;}
    public string resultMessage {get; protected set;}

    private string useTemplate;
    private string resultTemplate;

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
    public CommandEscape() : base("Escape", 0) {}
}
