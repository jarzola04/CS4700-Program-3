using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

// Rolls for a random encounter after every step the player takes
public class EncounterManager : MonoBehaviour
{
    [SerializeField] PlayerMovement mover;
    [SerializeField] EnemyData[] enemies;
    [SerializeField] AudioSource music;   // this should be set to overworld music
    [SerializeField] AudioClip battleMusic;
    [SerializeField] DarknessManager darkness; // for battle transition animation

    AudioResource fieldMusic;
    float fieldTime;
    const int EncounterOdds = 16; // 1 in 16 per step

    void OnEnable()
    {
        if (mover == null) mover = FindAnyObjectByType<PlayerMovement>();
        if (darkness == null) darkness = FindAnyObjectByType<DarknessManager>();
        if (mover != null) mover.Stepped += OnStepped;
        else Debug.LogWarning("EncounterManager: no PlayerMovement found, encounters disabled", this);
        Battle.Ended += OnBattleEnded;
    }

    void OnDisable()
    {
        if (mover != null) mover.Stepped -= OnStepped;
        Battle.Ended -= OnBattleEnded;
    }

    void OnStepped()
    {
        if (Battle.Active || enemies.Length == 0) return;
        if (Random.Range(0, EncounterOdds) != 0) return;

        Battle.Reserve();
        StartCoroutine(StartBattle(enemies[Random.Range(0, enemies.Length)]));
    }

    IEnumerator StartBattle(EnemyData enemy)
    {
        if (music != null && battleMusic != null)
        {
            fieldMusic = music.resource;
            fieldTime = music.time;
            music.resource = battleMusic;
            music.Play();
        }
        if (DarkPlace.IsDark && darkness != null) yield return darkness.CloseIn();
        Battle.Begin(enemy);
    }

    void OnBattleEnded()
    {
        if (darkness != null) darkness.Reopen();
        if (music == null || fieldMusic == null) return;
        music.resource = fieldMusic;
        music.Play();
        music.time = fieldTime;
        fieldMusic = null;
    }
}
