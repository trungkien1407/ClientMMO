using Assets.Script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterVisual : MonoBehaviour,ITargetable
{
    public string monsterName;
    public int maxHP;
    public int currentHP;

    public string Name => monsterName;
    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;

    public SpriteRenderer spriteRenderer;

    private Dictionary<string, List<Sprite>> stateFrames;
    private string currentState = "Idle";
    private int currentFrameIndex = 0;
    private float frameTimer = 0f;
    private float frameDuration = 0.3f;

    public void SetSprites(Dictionary<string, List<Sprite>> sprites)
    {
        stateFrames = sprites;

        if (stateFrames == null || stateFrames.Count == 0)
        {
            Debug.LogWarning("MonsterVisual received empty sprite data.");
            return;
        }

        if (stateFrames.ContainsKey("Idle"))
            SetState("Idle");
        else
            SetState(new List<string>(stateFrames.Keys)[0]);
    }

    public void SetState(string newState)
    {
        if (stateFrames == null || !stateFrames.ContainsKey(newState))
        {
            Debug.LogWarning($"State '{newState}' not found.");
            return;
        }

        currentState = newState;
        currentFrameIndex = 0;
        frameTimer = 0f;
        UpdateSprite();
    }

    private void Update()
    {
        if (stateFrames == null || !stateFrames.ContainsKey(currentState)) return;

        var frames = stateFrames[currentState];
        if (frames == null || frames.Count <= 1) return;

        frameTimer += Time.deltaTime;
        if (frameTimer >= frameDuration)
        {
            frameTimer = 0f;
            currentFrameIndex = (currentFrameIndex + 1) % frames.Count;
            UpdateSprite();
        }
    }

    private void UpdateSprite()
    {
        if (stateFrames.TryGetValue(currentState, out var frames) && frames.Count > 0)
        {
            spriteRenderer.sprite = frames[currentFrameIndex];
        }
    }
    public void PlayAttack(System.Action onComplete = null)
    {
        SetState("Attack");
        StartCoroutine(PlayAttackAnimationThenIdle(onComplete));
    }
    public void PlayHit(System.Action onComplete = null)
    {
        SetState("Hit");
        StartCoroutine(PlayHitAnimation(onComplete));
    }
    public void PlayDeath(bool facingRight, int spawnId)
    {
        SetState("Hit");
        StartCoroutine(PlayDeathAnimation(facingRight, spawnId));
    }

    private IEnumerator PlayAttackAnimationThenIdle(System.Action onComplete)
    {
        var move = gameObject.GetComponent<MonsterMovement>();
        move.enabled = false;
        var frames = stateFrames["Attack"];
        for (int i = 0; i < frames.Count; i++)
        {
            spriteRenderer.sprite = frames[i];
            yield return new WaitForSeconds(frameDuration);
        }

        onComplete?.Invoke(); // Gọi khi animation xong
       
        move.enabled = true;
        SetState("Idle");
    }
    private IEnumerator PlayHitAnimation(System.Action onComplete)
    {
     
        var frames = stateFrames["Hit"];
        for (int i = 0; i < frames.Count; i++)
        {
            spriteRenderer.sprite = frames[i];
            yield return new WaitForSeconds(frameDuration);
        }

        onComplete?.Invoke(); // Gọi khi animation xong

        SetState("Idle");
    }
    private IEnumerator PlayDeathAnimation(bool facingRight, int spawnId)
    {
        AudioManager.Instance.PlaySFX("MonsterDead");

        var frames = stateFrames["Hit"];
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + new Vector3(facingRight ? 1f : -1f, 0.5f, 0);

        float duration = frames.Count * 0.3f;
        float elapsed = 0f;

        for (int i = 0; i < frames.Count; i++)
        {
            spriteRenderer.sprite = frames[i];
            elapsed += 0.3f;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return new WaitForSeconds(0.3f);
        }
        // Sau animation mới gọi Remove
        MonsterManager.instance.RemoveMonster(spawnId);
    }



}
