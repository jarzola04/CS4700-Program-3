using UnityEngine;

// One monster type: what EncounterManager picks and Battle fights
[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy Data")]
public class EnemyData : ScriptableObject
{
    [SerializeField] string enemyName; // as shown in battle messages, e.g. "Ghost"
    [SerializeField] Sprite sprite;
    [SerializeField] int strength;
    [SerializeField] int agility;
    [SerializeField] int hp;
    [SerializeField] int mp;
    [SerializeField] int exp;  // awarded on victory
    [SerializeField] int gold; // awarded on victory

    public string EnemyName => enemyName;
    public Sprite Sprite => sprite;
    public int Strength => strength;
    public int Agility => agility;
    public int HP => hp;
    public int MP => mp;
    public int Exp => exp;
    public int Gold => gold;
}
