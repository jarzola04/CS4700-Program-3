public enum StatusType
{
    None,
    Sleep,
    StopSpell
}

public class StatusEffect
{
    public StatusType Type { get; private set; }
    public int Duration { get; set; }
    public string ApplyMessage { get; private set; }
    public string WearOffMessage { get; private set; }

    public StatusEffect(StatusType type, int duration, string applyMessage, string wearOffMessage)
    {
        Type = type;
        Duration = duration;
        ApplyMessage = applyMessage;
        WearOffMessage = wearOffMessage;
    }
}