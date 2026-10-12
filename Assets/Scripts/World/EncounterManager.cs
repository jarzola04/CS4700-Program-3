using UnityEngine;

// Rolls for a random encounter after every step the player takes
public class EncounterManager : MonoBehaviour
{
    [SerializeField] PlayerMovement mover;

    static readonly string[] enemies = { "Ghost", "Magician", "Scorpion", "Druin", "Druin" };
    const int EncounterOdds = 16; // 1 in 16 per step

    void OnEnable()
    {
        if (mover == null) mover = FindAnyObjectByType<PlayerMovement>();
        if (mover != null) mover.Stepped += OnStepped;
        else Debug.LogWarning("EncounterManager: no PlayerMovement found, encounters disabled", this);
    }

    void OnDisable()
    {
        if (mover != null) mover.Stepped -= OnStepped;
    }

    void OnStepped()
    {
        if (Random.Range(0, EncounterOdds) != 0) return;

        // TODO(combat): hand the enemy to the combat system once it exists.
        string enemy = enemies[Random.Range(0, enemies.Length)];
        MessageWindow.Show($"A {enemy} draws near!");
    }
}
