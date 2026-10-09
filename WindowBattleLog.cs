public class WindowBattleLog : WindowBase
{
    [SerializeField] private Text textField;
    private string textToAdd;
    private float textSpeed;
    private float scrollSpeed;

    private const int TOTAL_LINES = 7;
    private const int CHARS_PER_LINE = 22;

    public override void Open()
    {
        base.Open();
        textSpeed = 0.03f;
        scrollSpeed = 0.1f;
    }

    public ClearText()
    {
        textField.text = "";
        textToAdd = "";
    }

    public AddText(string text, bool breakline = true)
    {
        var invokeRunning = textToAdd.Length > 0;

        var enter = breakline ? "\n" : "";
        textToAdd += enter + text;

        if(!invokeRunning) Invoke("AddToDisplay", textSpeed);
    }

    private void AddToDisplay()
    {
        textField.text += textToAdd.Substring(0, 1);

        if(textToAdd.Length == 1)
        {
            textToAdd = "";
        } 
        else
        {
            var full = Full();
            if(full) ScrollTextUp();

            textToAdd = textToAdd.Substring(1, textToAdd.Length - 1);
            Invoke("AddToDisplay", textSpeed + (full ? scrollSpeed : 0));
        }
    }

    private bool Full()
    {
        var lines = 0;
        for(int i = 0, count = 0; i < textField.text.Length; i++)
        {
            if(textField.text.ToCharArray()[i] == '\n')
            {
                lines++;
                count = 0;
            }
            else
            {
                count++;
                if(count >= CHARS_PER_LINE)
                {
                    lines++;
                    count = 0;
                }
            }
        }

        return (lines >= TOTAL_LINES);
    }

    private void ScrollTextUp()
    {
        var nextLineStart = textField.text.IndexOf("\n") + 1;
        if(nextLineStart > CHARS_PER_LINE) nextLineStart = CHARS_PER_LINE + 1;
        var substr = textField.text.Substring(nextLineStart, textField.text.Length - nextLineStart);
        textField.text = substr;
    }
}