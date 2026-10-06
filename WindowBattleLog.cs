public class WindowBattleLog : MonoBehaviour
{
    private string text;

    public Open()
    {
        
    }

    public ClearText()
    {
        text = string.Empty;
    }

    public AddText(string text)
    {
        this.text = text;
    }
}