public class Character
{
    public string name{get; private set;}

    public int strength{get; private set;}

    public int agility{get; private set;}

    private int hp;
    private int maxHP;

    private int mp;
    private int maxMP;

    public int exp;
    public int level{get; private set;}
    public int gold;
    
    public int Hp
    {
        get
        {
            return hp;
        }

        set
        {
            hp = value;
            if (hp < 0) hp = 0;
        }
    }

    public int mp{get; private set;}
    public int exp{get; private set;}
    public int level{get; private set;}

    public Command command{get; private set;}

    public Character(string name = "char", int hp = 10, int mp = 5, int level = 1)
    {
        this.name = name;
        this.hp = hp;
        this.mp = mp;
        this.level = level;
        this.exp = 0;
    }

    public Command AddCommand(string name = "Attack", int value = 1)
    {
        command atkCmd = new command(name, value);
        command = atkCmd;

        return atkCmd;
    }
}