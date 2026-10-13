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
    private ListMenu spellMenu, itemMenu;
    [SerializeField]
    private ItemData herb;
    [SerializeField]
    private EnemyData enemy;
    [SerializeField]
    private Image enemyImage;
    [SerializeField]
    private GameObject[] standaloneOnly; // camera, EventSystem

    private const string SceneName = "Combat";

    public static bool Active { get; private set; }
    public static event System.Action Ended;
    private static EnemyData pending;
    private static bool reserved;
    private bool launched;
    [SerializeField]
    private WindowPlayerStatus windowPlayerStatus;
    
    private Coroutine executionCoroutine;
    private List<Command> commands = new List<Command>();
    private int turn;
    private BattleEffects effects;
    private ListMenu sub;

    public enum BattlePhase
    {
        Initialize,
        WaitCommand,
        ChooseCommand,
        ChooseSub,
        Execute,
        Result,
        Defeat,
        End
    }

    // Marks a battle as coming (blocks input and encounters) while a transition plays
    public static void Reserve()
    {
        Active = true;
        reserved = true;
    }

    public static void Begin(EnemyData enemy)
    {
        if (Active && !reserved) return;
        Active = true;
        reserved = false;
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
        if (launched) Ended?.Invoke();
    }

    private void Finish()
    {
        PlayerStats.SetVitals(playerGroup.member.Hp, playerGroup.member.Mp);
        ChangeState(BattlePhase.Initialize);
        enabled = false;
        if (launched) SceneManager.UnloadSceneAsync(gameObject.scene);
        else Active = false;
    }

    void Start()
    {
        playerGroup = new Group();
        var hero = playerGroup.AddCharacter(PlayerStats.Name, PlayerStats.Strength, PlayerStats.Agility, PlayerStats.MaxHP, PlayerStats.MaxMP, PlayerStats.Level);
        hero.Hp = PlayerStats.HP;
        hero.Mp = PlayerStats.MP;
        hero.exp = PlayerStats.Exp;
        hero.gold = PlayerStats.Gold;
        hero.AddCommand("Attack", 3);

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

        effects = new BattleEffects(playerGroup.member, enemyGroup.member, herb);
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
                    switch (windowBattleCommand.current)
                    {
                        case CmdFight: QueueTurn(playerGroup.member.command); break;
                        case CmdRun: QueueTurn(new CommandEscape()); break;
                        case CmdSpell: OpenSub(spellMenu, PlayerStats.Spells, "Thou hast not yet learned any spell."); break;
                        case CmdItem: OpenSub(itemMenu, ItemNames(), "Nothing of use has yet been given to thee."); break;
                    }
                }
                break;
            case BattlePhase.ChooseSub:
                UpdateSub();
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
                    if (playerGroup.Dead())
                    {
                        MessageWindow.Show("");
                        MessageWindow.Show(Vocab.Defeat);
                        ChangeState(BattlePhase.Defeat);
                        break;
                    }
                    MessageWindow.Show("");
                    MessageWindow.Show(Vocab.Victory(enemyGroup.member.name));
                    MessageWindow.Show("");
                    MessageWindow.Show(Vocab.Exp(enemyGroup.member.exp));
                    MessageWindow.Show(Vocab.Gold(enemyGroup.member.gold));
                    PlayerStats.AddExp(enemyGroup.member.exp);
                    PlayerStats.AddGold(enemyGroup.member.gold);
                    MessageWindow.Show("");
                    ChangeState(BattlePhase.End);
                }
                break;
            case BattlePhase.Defeat:
                if(UIController.Submit && !MessageWindow.Typing)
                {
                    UIController.UsePress();
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
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

    private const int CmdFight = 0, CmdRun = 1, CmdSpell = 2, CmdItem = 3;
    private Vector2Int lastDir;

    // Queues the player's command then the enemy's reply
    private void QueueTurn(Command playerCmd)
    {
        playerCmd.SetUser(playerGroup.member);
        playerCmd.SetTarget(enemyGroup.member);
        commands.Add(playerCmd);

        var cmd = enemyGroup.member.command;
        cmd.SetTarget(playerGroup.member);
        cmd.shakeEffect = true;
        commands.Add(cmd);

        phase = BattlePhase.Execute;
        windowBattleCommand.Close();
        if (sub != null) { sub.Close(); sub = null; }
    }

    private void OpenSub(ListMenu menu, IReadOnlyList<string> entries, string emptyMessage)
    {
        if (menu == null) return;
        if (entries.Count == 0)
        {
            MessageWindow.Show(emptyMessage);
            return;
        }
        sub = menu;
        sub.Open(entries);
        lastDir = InputDirection.Cardinal(UIController.Navigate);
        ChangeState(BattlePhase.ChooseSub);
    }

    private void UpdateSub()
    {
        Vector2Int dir = InputDirection.Cardinal(UIController.Navigate);
        if (dir != lastDir && dir.y != 0) sub.Step(-dir.y); // up = -index
        lastDir = dir;

        if (UIController.Cancel)
        {
            UIController.UsePress();
            sub.Close();
            sub = null;
            ChangeState(BattlePhase.ChooseCommand);
        }
        else if (UIController.Submit && !MessageWindow.Typing)
        {
            UIController.UsePress();
            bool isItem = sub == itemMenu;
            int index = sub.SelectedIndex;
            string spell = isItem ? null : sub.Selected;
            ItemData item = isItem ? InventoryManager.Instance.Slots[index].Item : null;
            QueueTurn(new CommandEffect(isItem ? item.ItemName : spell,
                () => isItem ? effects.UseItem(item) : effects.CastSpell(spell)));
        }
    }

    private static List<string> ItemNames()
    {
        var names = new List<string>();
        if (InventoryManager.Instance == null) return names;
        foreach (var slot in InventoryManager.Instance.Slots) names.Add(slot.Item.ItemName);
        return names;
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
            // begin sleep update
            if (cmd.user.IsAsleep)
            {
                MessageWindow.Show($"{cmd.user.name} is fast asleep!");
                yield return WaitMessage();
                foreach (string wake in cmd.user.TickStatuses())
                {
                    MessageWindow.Show(wake);
                    yield return WaitMessage();
                }
                MessageWindow.Show("");
                continue;
            }
            // end sleep update
            if (cmd is CommandEffect effectCmd)
            {
                System.Action effect = effectCmd.Begin();
                yield return WaitMessage();
                effect?.Invoke();
                if (windowPlayerStatus != null)
                {
                    windowPlayerStatus.SetHP(playerGroup.member.Hp);
                    windowPlayerStatus.SetMP(playerGroup.member.Mp);
                }
                yield return WaitMessage();
                MessageWindow.Show("");
                if (IsBattleOver())
                {
                    executionCoroutine = null;
                    phase = BattlePhase.Result;
                    yield break;
                }
                continue;
            }
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
