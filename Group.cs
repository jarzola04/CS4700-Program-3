public class Group
{
    public Character member{get; private set;}

    public Group ()
    {
        
    }

    public Character AddCharacter(string name = "char", int hp = 10, int mp = 5, int level = 1)
    {
        var ch = new Character(name, hp, mp, level);
        member = ch;
        return ch;
    }

    public bool Dead()
    {
        return (member.Hp == 0);
    }
}