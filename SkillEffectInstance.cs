using Assets.Script;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SkillEffectInstance : MonoBehaviour
{
    private List<SpriteRenderer> spriteRenderers = new();
    private List<BoxCollider2D> colliders = new();
    private List<Coroutine> coroutines = new();
    private SkillData data;
    private Transform selectedTarget; // Target đã được chọn
    private bool isLocalPlayer = false;

    public void Init(SkillData skillData, Vector2 position, bool facingRight, Transform targetTransform,bool islocal)
    {
        Reset();
        data = skillData;
        selectedTarget = targetTransform; // Lưu target đã chọn
        transform.position = position;
        transform.localScale = new Vector3(facingRight ? 1 : -1, 1, 1);
        isLocalPlayer = islocal;

        for (int i = 0; i < data.slashes.Count; i++)
        {
            var slash = data.slashes[i];
            SpriteRenderer sr;
            BoxCollider2D col;

            if (i < spriteRenderers.Count)
            {
                sr = spriteRenderers[i];
                col = colliders[i];
                sr.gameObject.SetActive(true);
            }
            else
            {
                GameObject go = new GameObject($"EffectPart_{i}");
                go.transform.SetParent(transform);
                sr = go.AddComponent<SpriteRenderer>();
                col = go.AddComponent<BoxCollider2D>();

                sr.sortingLayerName = "PlayerLocal";
                sr.sortingOrder = 11;
                col.isTrigger = true;
                col.enabled = isLocalPlayer;
                spriteRenderers.Add(sr);
                colliders.Add(col);
            }

            sr.sprite = slash.sprite;
            Vector2 offset = facingRight ? slash.offset : new Vector2(-slash.offset.x, slash.offset.y);
            float rotation = facingRight ? slash.rotation : -slash.rotation;

            sr.transform.localPosition = offset;
            sr.transform.localRotation = Quaternion.Euler(0, 0, rotation);
            sr.transform.localScale = Vector3.one;

            col.size = slash.hitboxSize;
          

            Coroutine co = slash.animationFrames != null && slash.animationFrames.Count > 0
                ? (slash.moveToTarget
                    ? StartCoroutine(MoveToSelectedTarget(sr, col, slash, facingRight))
                    : StartCoroutine(PlayAnimation(sr, col, slash)))
                : StartCoroutine(ShowOnly(sr, col, slash.delay, slash.duration));

            coroutines.Add(co);
        }

        // Disable extra
        for (int i = data.slashes.Count; i < spriteRenderers.Count; i++)
        {
            spriteRenderers[i].gameObject.SetActive(false);
            colliders[i].enabled = false;
        }
    }

    public void Reset()
    {
        foreach (var co in coroutines)
        {
            if (co != null) StopCoroutine(co);
        }
        coroutines.Clear();

        foreach (var sr in spriteRenderers) sr.gameObject.SetActive(false);
        foreach (var col in colliders) col.enabled = false;

        selectedTarget = null; // Clear target reference
    }

    private IEnumerator PlayAnimation(SpriteRenderer sr, BoxCollider2D col, EffectSlash slash)
    {
        
        sr.enabled = false;
        col.enabled = false;
        yield return new WaitForSeconds(slash.delay);

        sr.enabled = true;
        col.enabled = true;

        float elapsed = 0f;
        float frameTime = 1f / slash.frameRate;
        int frameCount = slash.animationFrames.Count;

        while (elapsed < slash.duration)
        {
            int index = Mathf.FloorToInt(elapsed / frameTime) % frameCount;
            sr.sprite = slash.animationFrames[index];
            elapsed += Time.deltaTime;
            yield return null;
        }

        sr.enabled = false;
        col.enabled = false;
        CheckAndReturn();
    }

    // Cải tiến MoveToTarget để bay đến target đã chọn
    private IEnumerator MoveToSelectedTarget(SpriteRenderer sr, BoxCollider2D col, EffectSlash slash, bool facingRight)
    {
     
        sr.enabled = false;
        col.enabled = false;
        yield return new WaitForSeconds(slash.delay);

        sr.enabled = true;
        col.enabled = true;

        Vector3 startWorld = sr.transform.position;
      //  Vector3 targetOffset = facingRight ? slash.targetOffset : new Vector2(-slash.targetOffset.x, slash.targetOffset.y);

        Vector3 endWorld;

        // Sử dụng selectedTarget thay vì parameter target
        if (selectedTarget != null)
        {
            // Bay đến vị trí của target đã chọn
            Vector3 targetPosition = selectedTarget.position;
            Vector3 toTarget = targetPosition - startWorld;

            // Giới hạn trong phạm vi skill nếu cần
            if (data.skillRange > 0)
            {
                float distance = Mathf.Min(toTarget.magnitude, data.skillRange);
                endWorld = startWorld + toTarget.normalized * distance;
            }
            else
            {
                endWorld = targetPosition;
            }
        }
        else
        {
            // Fallback: bay theo hướng facing nếu không có target
            Vector3 direction = (facingRight ? Vector3.right : Vector3.left) * 3f;
            endWorld = startWorld + direction;
        }

        float elapsed = 0f;
        float frameTime = 1f / slash.frameRate;
        int frameCount = slash.animationFrames.Count;

        while (elapsed < slash.duration)
        {
            float t = elapsed / slash.duration;

            // Di chuyển về phía target
            sr.transform.position = Vector3.Lerp(startWorld, endWorld, t);

            // Update animation frame
            int index = Mathf.FloorToInt(elapsed / frameTime) % frameCount;
            sr.sprite = slash.animationFrames[index];

            elapsed += Time.deltaTime;
            yield return null;
        }

        sr.enabled = false;
        col.enabled = false;
        if (selectedTarget != null)
        {
            var monster = selectedTarget.GetComponent<MonsterVisual>();
     
            if (monster != null && monster.gameObject.activeInHierarchy)
            {
              //  SendAttack();
              
                monster.PlayHit();//Trigger animation OnHit

                
            }
        }

        CheckAndReturn();
    }

    private IEnumerator ShowOnly(SpriteRenderer sr, BoxCollider2D col, float delay, float duration)
    {
        sr.enabled = false;
        col.enabled = false;

        yield return new WaitForSeconds(delay);

        sr.enabled = true;
        col.enabled = true;

        yield return new WaitForSeconds(duration);

        sr.enabled = false;
        col.enabled = false;
        CheckAndReturn();
    }

    private void CheckAndReturn()
    {
        if (spriteRenderers.All(sr => !sr.enabled))
        {
            SkillPool.Instance.ReturnToPool(this);
        }
    }




}