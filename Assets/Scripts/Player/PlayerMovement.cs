using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Tilemap walls;
    [SerializeField] InputActionReference move;
    [SerializeField] float stepTime = 0.25f;

    bool moving;
    public bool IsMoving => moving;
    public event Action Stepped; // fired after each completed tile move

    [Header("Animation")]
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Sprite[] up = new Sprite[2];
    [SerializeField] Sprite[] down = new Sprite[2];
    [SerializeField] Sprite[] left = new Sprite[2];
    [SerializeField] Sprite[] right = new Sprite[2];

    Vector3Int facing = Vector3Int.down;
    public Vector3Int Facing
    {
        get => facing;
        private set
        {
            if (facing == value) return;
            facing = value;
            RefreshSprite();
        }
    }
    public Tilemap Walls => walls;
    public Vector3Int FacingCell => walls.WorldToCell(transform.position) + Facing;

    int frame; // toggles 0/1 every step tick
    float stepTimer;

    [Header("Audio")]
    [SerializeField] AudioSource audioSource; // bump sound

    void Start()
    {
        Vector3Int cell = walls.WorldToCell(transform.position);
        transform.position = walls.GetCellCenterWorld(cell);

        if (sprite == null) sprite = GetComponent<SpriteRenderer>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null) Debug.LogWarning("PlayerMovement: no AudioSource assigned, bump sound disabled", this);
        else if (audioSource.clip == null) Debug.LogWarning("PlayerMovement: AudioSource has no clip, bump sound will be silent", this);
        RefreshSprite();
    }

    void RefreshSprite()
    {
        if (sprite == null) return;

        Sprite[] frames =
            facing == Vector3Int.up ? up :
            facing == Vector3Int.left ? left :
            facing == Vector3Int.right ? right : down;

        if (frames == null || frames.Length == 0) return;
        Sprite s = frames[frame % frames.Length];
        if (s != null) sprite.sprite = s;
    }

    void Update()
    {
        if (UIController.Blocking) return;

        Vector3Int dir = (Vector3Int)InputDirection.Cardinal(move.action.ReadValue<Vector2>());
        if (dir != Vector3Int.zero && !moving) Facing = dir;

        stepTimer += Time.deltaTime;
        if (stepTimer < stepTime || moving) return; // step in progress delays tick

        // Same tick drives the animation + movement
        stepTimer -= stepTime;
        frame ^= 1;
        RefreshSprite();

        if (dir == Vector3Int.zero) return;

        Vector3Int target = walls.WorldToCell(transform.position) + dir;
        if (!walls.HasTile(target)) StartCoroutine(MoveTo(walls.GetCellCenterWorld(target)));
        else if (audioSource != null) audioSource.Play(); // bumped into a wall
    }

    IEnumerator MoveTo(Vector3 target)
    {
        moving = true;
        Vector3 start = transform.position;

        if (DarkPlace.IsDark)
        {
            // Snap halfway first then to the tile center
            transform.position = (start + target) / 2f;
            yield return new WaitForSeconds(stepTime / 2f);
            transform.position = target;
            yield return new WaitForSeconds(stepTime / 2f);
        }
        else // I wrote this logic before I realized that caves had different movement
        {    // Keeping it just in case there's any reason to make an outside scene?
            for (float t = 0; t < stepTime; t += Time.deltaTime) // probably not
            {
                transform.position = Vector3.Lerp(start, target, t / stepTime);
                yield return null;
            }
            transform.position = target;
        }

        moving = false;
        Stepped?.Invoke();
    }
}
