public static class Vocab
{
    public static string BattleStart(string enemy)
    {
        return (string.Format("A {0} draws near!", enemy));
    }

    public static string Victory(string enemy)
    {
        return (string.Format("Thou hast done well in defeating the {0}.", enemy));
    }

    public static string Gold(int amount)
    {
        return (string.Format("Thy Gold \nincreases by {0}.", amount));
    }

    public static string Exp(int amount)
    {
        return (string.Format("The Experience \nincreases by {0}.", amount));
    }
}