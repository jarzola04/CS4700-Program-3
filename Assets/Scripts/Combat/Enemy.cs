public class Enemy : Character
{
    public int dodge = 1 / 64;
    
    public static int UNIQUE_ID = 0;

    public int id{get; private set;}

    public Enemy(string name = "char", int strength = 10, int agility = 10, int hp = 10, int mp = 5, int level = 1) : base(name, strength, agility, hp, mp, level)
    {
        id = UNIQUE_ID++;
    }

    public float GroupFactor()
    {
        if(id < 19) return 0.25f;
        else if(id < 29) return 0.375f;
        else if(id < 34) return 0.5f;

        return 1.0f;
    }
}