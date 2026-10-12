using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Battle : MonoBehaviour
{
    public Group playerGroup;
    public Group enemyGroup;
    public BattlePhase phase = BattlePhase.Initialize;
    [SerializeField]
    private Transform shakeTarget; // canvas whose children ScreenShakeEffect moves; defaults to the MessageWindow's canvas
    [SerializeField]
    private WindowBattleCommand windowBattleCommand;
    [SerializeField]
    private EnemyData enemy;
    [SerializeField]
    private Image enemyImage;
    [SerializeField]
    private GameObject[] standaloneOnly; // camera, EventSystem

    private const string SceneName = "Combat";

    public static bool Active { get; private set; }
    private static EnemyData pending;
    private bool launched;
    [SerializeField]
    private WindowPlayerStatus windowPlayerStatus;
    
    private Coroutine executionCoroutine;
    private List<Command> commands = new List<Command>();
    private int turn;

    public enum BattlePhase
    {
        Initialize,
        WaitCommand,
        ChooseCommand,
        Execute,
        Result,
        End
    }

    public static void Begin(EnemyData enemy)
    {
        if (Active) return;
        Active = true;
        pending = enemy;
        SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
    }

    void Awake()
    {
        Active = true;
        if (pending == null) return;
        launched = true;
        enemy = pending;
        pending = null;
        foreach (var go in standaloneOnly) go.SetActive(false);
    }

    void OnDestroy()
    {
        Active = false;
    }

    private void Finish()
    {
        ChangeState(BattlePhase.Initialize);
        enabled = false;
        if (launched) SceneManager.UnloadSceneAsync(gameObject.scene);
        else Active = false;
    }

    void Start()
    {
        playerGroup = new Group();
        playerGroup.AddCharacter(PlayerStats.Name, PlayerStats.Strength, PlayerStats.Agility, PlayerStats.MaxHP, PlayerStats.MaxMP, PlayerStats.Level).AddCommand("Attack", 3);

        enemyGroup = new Group();
        if (enemy != null)
        {
            var foe = enemyGroup.AddCharacter(enemy.EnemyName, enemy.Strength, enemy.Agility, enemy.HP, enemy.MP, 1);
            foe.exp = enemy.Exp;
            foe.gold = enemy.Gold;
            foe.AddCommand("Attack", 1);
            if (enemyImage != null) enemyImage.sprite = enemy.Sprite;
        }
        else // default enemy is a slime but it shouldn't be used
        {
            enemyGroup.AddCharacter("Slime", 5, 3, 5, 0, 1).AddCommand("Bite", 1);
        }

        Enter();
    }

    private void Enter()
    {
        commands.Clear();
        MessageWindow.Clear();
        CreateWindowPlayerStatus();
        ChangeState(BattlePhase.Initialize);
    }

    private void Update()
    {
        switch(phase)
        {
            case BattlePhase.Initialize:
                turn = 0;
                var msg = playerGroup.member.name + " is ready for battle! HP: " + playerGroup.member.Hp;
                MessageWindow.Show(msg);
                msg = "A " + enemyGroup.member.name + " draws near! HP: " + enemyGroup.member.Hp;
                MessageWindow.Show(msg);
                ChangeState(BattlePhase.WaitCommand);
                break;
            case BattlePhase.WaitCommand:
                if(UIController.Submit && !MessageWindow.Typing)
                {
                    UIController.UsePress();
                    MessageWindow.Clear();
                    ChangeState(BattlePhase.Execute);
                }
                break;
            case BattlePhase.ChooseCommand:
                windowBattleCommand.ManualUpdate();
                if(UIController.Submit && !MessageWindow.Typing)
                {
                    UIController.UsePress();
                    var cmd = playerGroup.member.command;
                    cmd.SetTarget(enemyGroup.member);
                    commands.Add(cmd);

                    cmd = enemyGroup.member.command;
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
                if(UIController.Submit && !MessageWindow.Typing)
                {
                    UIController.UsePress();
                    MessageWindow.Show("");
                    MessageWindow.Show(Vocab.Victory(enemyGroup.member.name));
                    MessageWindow.Show("");
                    MessageWindow.Show(Vocab.Exp(enemyGroup.member.exp));
                    MessageWindow.Show(Vocab.Gold(enemyGroup.member.gold));
                    MessageWindow.Show("");
                    ChangeState(BattlePhase.End);
                }
                break;
            case BattlePhase.End:
                if(UIController.Submit && !MessageWindow.Typing)
                {
                    UIController.UsePress();
                    Finish();
                }
                break;
        }
    }

    private void CreateWindowBattleCommand()
    {
        windowBattleCommand.Open();
    }

    private void CreateWindowPlayerStatus()
    {
        if (windowPlayerStatus == null) return;
        windowPlayerStatus.gameObject.SetActive(true);
        windowPlayerStatus.Initialize();
        windowPlayerStatus.SetStatus(playerGroup.member);
    }
    private void ChangeState(BattlePhase phase)
    {
        this.phase = phase;
    }

    private IEnumerator Execute()
    {
        while (commands.Count > 0)
        { 
            Command cmd = commands[0];
            commands.RemoveAt(0);
            cmd.Execute();
            MessageWindow.Show(cmd.useMessage);
            yield return WaitMessage();
            if(cmd.success) yield return ScreenShakeEffect();
            MessageWindow.Show(cmd.resultMessage);
            yield return WaitMessage();
            MessageWindow.Show("");

            if(IsBattleOver())
            {
                executionCoroutine = null;
                phase = BattlePhase.Result;
                yield break;
            }
            else if(cmd is CommandEscape)
            {
                if(cmd.success)
                {
                    executionCoroutine = null;
                    phase = BattlePhase.End;
                    yield break;
                }
                
            }
            if (windowPlayerStatus != null) windowPlayerStatus.SetHP(playerGroup.member.Hp);
        }

        executionCoroutine = null;

        MessageWindow.Show("Command?");
        phase = BattlePhase.ChooseCommand;
        CreateWindowBattleCommand();
    }  
    private bool IsBattleOver()
    {
        return enemyGroup.Dead() || playerGroup.Dead();
    }

    private IEnumerator WaitMessage()
    {
        while(MessageWindow.Typing)
        {
            yield return null;
        }
    }

    public IEnumerator ScreenShakeEffect(float duration = 0.1f, bool waitComplete = true)
    {
        if (shakeTarget == null && MessageWindow.Instance != null)
            shakeTarget = MessageWindow.Instance.GetComponentInParent<Canvas>().rootCanvas.transform;
        if (shakeTarget == null) yield break;

        // An overlay canvas's own transform is locked, so shake everything on it together
        var offset = new Vector3(
            Random.Range(2, 4) * (Random.Range(0, 100) > 50 ? -1 : 1),
            Random.Range(2, 4) * (Random.Range(0, 100) > 50 ? -1 : 1));
        foreach (Transform child in shakeTarget)
            child.DOLocalMove(child.localPosition + offset, duration).SetLoops(4, LoopType.Yoyo);

        yield return new WaitForSeconds(waitComplete ? duration : 0f);
    }
}
