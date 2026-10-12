public class Character
{
    public string name { get; private set; }

    public int strength { get; private set; }
    public int agility { get; private set; }

    private int hp;
    private int maxHP;

    private int mp;
    private int maxMP;

    public int exp { get; private set; }
    public int level { get; private set; }
    public int gold;

    public int Hp
    {
        get { return hp; }
        set { hp = UnityEngine.Mathf.Clamp(value, 0, maxHP); }
    }

    public int MaxHP { get { return maxHP; } }
    public int Mp { get { return mp; } }
    public int MaxMP { get { return maxMP; } }

    public Command command { get; private set; }

    public Character(
        string name = "char",
        int hp = 10,
        int mp = 5,
        int level = 1)
    {
        this.name = name;

        this.maxHP = hp;
        this.hp = hp;

        this.maxMP = mp;
        this.mp = mp;

        this.level = level;
        this.exp = 0;

        this.strength = 0;
        this.agility = 0;
        this.gold = 0;
    }

    public Character(string name, int strength, int agility, int hp, int mp, int level)
        : this(name, hp, mp, level)
    {
        this.strength = strength;
        this.agility = agility;
    }

    public Command AddCommand(string name = "Attack", int value = 1)
    {
        Command atkCmd = new Command(name, value);
        command = atkCmd;

        return atkCmd;
    }
}