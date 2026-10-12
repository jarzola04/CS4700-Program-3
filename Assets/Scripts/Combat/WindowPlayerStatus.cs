using UnityEngine;
using TMPro;

// same as StatusWindow's mini status but I don't wanna change that one
public class WindowPlayerStatus : WindowBase
{
    [SerializeField] private TMP_Text labels, values;

    private Character character;

    public void SetStatus(Character character)
    {
        if (character == null)
            return;

        this.character = character;
        Refresh();
    }

    // Values are read from the character, so these redraw
    // don't wanna rewrite combat
    public void SetHP(int hp) => Refresh();
    public void SetMP(int mp) => Refresh();

    private void Refresh()
    {
        if (character == null)
            return;

        labels.text = $"\nLV\nHP\nMP\nG\nE";
        values.text = $"\n{character.level}\n{character.Hp}\n{character.Mp}\n{character.gold}\n{character.exp}";
    }
}
