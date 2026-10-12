public class WindowBattleCommand : WindowBase
{
    [SerializeField] private Transform arrow;
    [SerializeField] private BrightButton[] options;
    [SerializeField] private BrightButton outsideArea;

    private bool listenersInitialized = false;

    public int current{get; private set;}

    public override void Initialize()
    {
        base.Initialize();

        current = 0;

        if (!listenersInitialized)
        {
            for (int i = 0; i < options.Length; i++)
            {
                options[i].id = i;
                options[i].onSelect.AddListener(OnOptionSelected);
            }

            outsideArea.OnClick.AddListener(OnClickOutsideArea);
            listenersInitialized = true;
        }

        if (options.Length > 0)
        {
            UpdateArrow();
        }
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
        if (options.Length == 0)
            return;

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            current++;

            if (current >= options.Length)
                current = 0;

            UpdateArrow();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            current--;

            if (current < 0)
                current = options.Length - 1;

            UpdateArrow();
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void UpdateArrow()
    {
        arrow.SetParent(options[current].transform, false);
        arrow.localPosition = new Vector3(-60f, 0f, 0f);
    }

    private void OnOptionSelected(int buttonID)
    {
        if (buttonID < 0 || buttonID >= options.Length)
            return;

        current = buttonID;
        UpdateArrow();
    }
}