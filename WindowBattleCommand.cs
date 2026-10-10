public class WindowBattleCommand : WindowBase
{
    [SerializeField] private Transform arrow;
    [SerializeField] private BrightButton[] options;
    [SerializeField] private BrightButton outsideArea;

    public int current{get; private set;}

    public void Initialize()
    {
        
    }

    public override void Initialize()
    {
        base.Initialize();

        for(int i = 0 ; i < options.Length ; i++)
        {
            options[i].id = i;
            options[i].onSelect.AddListener(OnOptionSelected);
        }
        outsideArea.OnClick.AddListener(OnClickOutsideArea);
    }

    public override void Execute()
    {
        useMessage = string.Format(useMessage, user.name);

        if(!(user is Enemy))
        {
            success = (user.agility * RandomNum() > target.agility * RandomNum() * (target as Enemy).GroupFactor());
        }
        resultMessage = success ? successMessage : failMessage;
    }

    public void ManualUpdate()
    {
        
    }

    public void Close()
    {
        
    }

    private void OnOptionSelected(int buttonID)
    {
        arrow.SetParent(options[buttonID].transform);
        current = buttonID;
    }
}