public class Battle : monoBehaviour
{
    public Group playerGroup;
    public Group enemyGroup;
    public BattleState state;
    public WindowBattleLog windowBattleLog;

    enum BattleState
    {
        Initialize,
        WaitCommand,
        Executing,
        Result,
    }
    void Start()
    {
        playerGroup = new Group();
        playerGroup.AddCharacter("Alex", 20, 7, 1).AddCommand("Attack", 3);

        enemyGroup = new Group();
        enemyGroup.AddCharacter("Slime", 5, 0, 1).AddCommand("Bite", 1);

        Enter();
    }

    private void Enter()
    {
        CreateWindowBattleLog();
    }

    private void Update()
    {
        switch(state)
        {
            case state.Initialize:
                turn = 0;
                var msg = playerGroup.member.name + "is ready for battle! HP: " + playerGroup.member.Hp;
                windowBattleLog.AddText(msg, false);
                msg = "A " + enemyGroup.member.name + " draws near! HP: " + enemyGroup.member.Hp;
                windowBattleLog.AddText(msg);
                ChangeState(BattleState.WaitCommand);
                break;
            case state.WaitCommand:
                if(Input.GetButtonDown("Submit"))
                {
                    windowBattleLog.ClearText();
                    ChangeState(BattleState.Executing);
                }
                break;
            case state.Executing:
                Character attacker, target;
                attacker = enemyGroup.member;
                target = playerGroup.member;
                attacker.command.Execute(target);

                var msg2 = attacker.name + "uses " + attacker.command.name;
                windowBattleLog.AddText(msg2, false);
                windowBattleLog.AddText(msg2);

                attacker = playerGroup.member;
                target = enemyGroup.member;
                attacker.command.Execute(target);

                msg2 = attacker.name + " uses " + attacker.command.name;
                windowBattleLog.AddText(msg2);
                msg2 = target.name + " took " + attacjer.command.value + "damage. HP droppped to " + target.Hp;
                windowBattleLog.AddText(msg2);

                state = enemyGroup.Dead() ? BattleState.Result : BattleState.WaitCommand;
                break;

            case state.Result:
                if (Input.GetButtonDown("Submit"))
                {
                    windowBattleLog.ClearText();
                    windowBattleLog.AddText("Enemy defeated!");
                    ChangeState(BattleState.End);
                }
                break;
        }
    }

    private void CreateWindowBattleLog()
    {
        windowBattleLog.Open();
        windowBattleLog.ClearText();

    }    
}