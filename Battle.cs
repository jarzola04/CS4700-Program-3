using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Battle : MonoBehaviour
{
    public Group playerGroup;
    public Group enemyGroup;
    public BattlePhase phase = BattlePhase.Initialize;
    [SerializeField]
    private WindowBattleLog windowBattleLog;
    [SerializeField]
    private WindowBattleCommand windowBattleCommand;
    private Vocab vocab;
    private Coroutine executionCoroutine;
    private List<Command> commands = new List<Command>();
    private int turn;

    enum BattlePhase
    {
        Initialize,
        WaitCommand,
        ChooseCommand,
        Execute,
        Result,
        End
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
        commands.Clear();
        CreateWindowBattleLog();
        ChangeState(BattlePhase.Initialize);
    }

    private void Update()
    {
        switch(phase)
        {
            case BattlePhase.Initialize:
                turn = 0;
                var msg = playerGroup.member.name + "is ready for battle! HP: " + playerGroup.member.Hp;
                windowBattleLog.AddText(msg, false);
                msg = "A " + enemyGroup.member.name + " draws near! HP: " + enemyGroup.member.Hp;
                windowBattleLog.AddText(msg);
                ChangeState(BattlePhase.WaitCommand);
                break;
            case BattlePhase.WaitCommand:
                if(Input.GetButtonDown("Submit"))
                {
                    windowBattleLog.ClearText();
                    ChangeState(BattlePhase.Execute);
                }
                break;
            case BattlePhase.ChooseCommand:
                windowBattleCommand.ManualUpdate();
                if(Input.GetButtonDown("Submit"))
                {
                    var cmd = playerGroup.member.commands[windowBattleCommand.current];
                    cmd.SetTarget(enemyGroup.member);
                    commands.Add(cmd);

                    cmd = enemyGroup.member.commands[0];
                    cmd.SetTarget(playerGroup.member);
                    cmd.shakeEffect = true;
                    commands.Add(cmd);

                    phase = BattlePhase.Execute;
                    windowBattleCommand.Close();
                }
                break;
            case BattlePhase.Execute:
                if (executionCoroutine == null)
                {
                    executionCoroutine = StartCoroutine(Execute());
                }
                break;

            case BattlePhase.Result:
                if (Input.GetButtonDown("Submit"))
                {
                    windowBattleLog.AddText("");
                    windowBattleLog.AddText(vocab.Victory(enemyGroup.member.name));
                    windowBattleLog.AddText("");
                    windowBattleLog.AddText(vocab.Exp(enemyGroup.member.exp));
                    windowBattleLog.AddText(vocab.Gold(enemyGroup.member.gold));
                    windowBattleLog.AddText("");
                    ChangeState(BattlePhase.End);
                }
                break;
            case BattlePhase.End:
                break;
        }
    }

    private void CreateWindowBattleLog()
    {
        windowBattleLog.Open();
        windowBattleLog.ClearText();

    }  

    private void CreateBattleCommand()
    {
        windowBattleCommand.Initialize();
    }

    private void ChangeState(BattlePhase phase)
    {
        this.phase = phase;
    }

    private IEnumerator Execute()
    {
        while (commands.Count > 0)
        {
            var cmd = commands[0] as windowBattleCommand;
            commands.RemoveAt(0);
            windowBattleLog.AddText(cmd.useMessage);
            yield return WaitMessage();
            if(cmd.success) yield return ScreenShakeEffect();
            windowBattleLog.AddText(cmd.resultMessage);
            yield return WaitMessage();
            windowBattleLog.Breakline();

            if(IsBattleOver())
            {
                phase = BattlePhase.Result;
                yield break;
            }
            else if(cmd is CommandEscape)
            {
                if(cmd.success)
                {
                    phase = BattlePhase.End;
                    yield break;
                }
                
            }
            windowPlayerStatus.SetHP(playerGroup.member.Hp);
        }

        executionCoroutine = null;

        windowBattleLog.AddText("Command?");
        phase = BattlePhase.ChooseCommand;
        CreateWindowBattleCommand();
    }  

    private IEnumerator WaitMessage()
    {
        while(!windowBattleLog.IsIdle())
        {
            yield return new WaitForSeconds(0.1f);
        }
    }

    public IEnumerator ScreenShakeEffect(float duration = 0.1f, bool waitComplete = true)
    {
        Transform parent = windowBattleLog.transform.parent;
        var targetPos = parent.localPosition;
        targetPos.x = Random.Range(2, 4) * (Random.Range(0, 100) > 50 ? -1 : 1);
        targetPos.y = Random.Range(2, 4) * (Random.Range(0, 100) > 50 ? -1 : 1);
        parent.DOLocalMove(targetPos, duration).SetLoops(4, LoopType.Yoyo);

        yield return new WaitForSeconds(waitComplete ? duration : 0f);
    }
}