using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class StaticEncounter : MonoBehaviour
{
    [SerializeField] PlayerMovement mover;
    [SerializeField] EnemyData enemy;
    [SerializeField] AudioSource music;
    [SerializeField] AudioClip battleMusic;
    [SerializeField] AudioClip encounterSfx;
    [SerializeField] DarknessManager darkness; // for battle transition animation
    [SerializeField] bool repeatable;

    AudioResource fieldMusic;
    float fieldTime;
    bool done;
    bool fighting;

    void OnEnable()
    {
        if (mover == null) mover = FindAnyObjectByType<PlayerMovement>();
        if (darkness == null) darkness = FindAnyObjectByType<DarknessManager>();
        if (mover != null) mover.Stepped += OnStepped;
        else Debug.LogWarning("StaticEncounter: no PlayerMovement found, encounter disabled", this);
        Battle.Ended += OnBattleEnded;
    }

    void OnDisable()
    {
        if (mover != null) mover.Stepped -= OnStepped;
        Battle.Ended -= OnBattleEnded;
    }

    void OnStepped()
    {
        if (done || Battle.Active || enemy == null) return;

        Vector3Int spot = mover.Walls.WorldToCell(transform.position);
        Vector3Int player = mover.Walls.WorldToCell(mover.transform.position);
        if (player != spot) return;

        done = !repeatable;
        Battle.Reserve();
        if (encounterSfx != null && music != null) music.PlayOneShot(encounterSfx);
        StartCoroutine(StartBattle());
    }

    IEnumerator StartBattle()
    {
        fighting = true;
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
        if (!fighting) return; // not the static battle
        fighting = false;
        if (darkness != null) darkness.Reopen();
        if (music == null || fieldMusic == null) return;
        music.resource = fieldMusic;
        music.Play();
        music.time = fieldTime;
        fieldMusic = null;
    }
}
