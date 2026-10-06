public class Command
{
    public string name {get ; private set;}
    public string value {get ; private set;}

    public Command (string name = "Attack", int value = 1)
    {
        this.name = name;
        this.value = value;
    }

    public void Execute(Character target)
    {
        int targetHp = target.Hp - value;
        target.Hp = targetHp;            
    }
}