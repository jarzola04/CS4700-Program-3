using UnityEngine;
using TMPro;

public class WindowPlayerStatus : WindowBase
{
    [Header("Player Status Text")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text mpText;
    [SerializeField] private TMP_Text levelText;

    private Character character;

    public override void Initialize()
    {
        base.Initialize();
    }

    public void SetStatus(Character character)
    {
        if (character == null)
            return;

        this.character = character;

        nameText.text = character.name;
        hpText.text = $"HP {character.Hp}/{character.MaxHP}";
        mpText.text = $"MP {character.Mp}/{character.MaxMP}";
        levelText.text = $"Lv {character.level}";
    }

    public void SetHP(int hp)
    {
        if (character == null)
            return;

        hpText.text = $"HP {hp}/{character.MaxHP}";
    }

    public void SetMP(int mp)
    {
        if (character == null)
            return;

        mpText.text = $"MP {mp}/{character.MaxMP}";
    }
}