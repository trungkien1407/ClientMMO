using System.Collections.Generic;
using UnityEngine;

public class SkillPool : MonoBehaviour
{
    public static SkillPool Instance;
    [SerializeField] private int maxPoolSize = 20;
    private readonly Queue<SkillEffectInstance> pool = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public SkillEffectInstance GetSkillEffect()
    {
        SkillEffectInstance effect;

        if (pool.Count > 0)
        {
            effect = pool.Dequeue();
            effect.gameObject.SetActive(true);
        }
        else if (transform.childCount < maxPoolSize)
        {
            GameObject go = new GameObject("SkillEffect");
            go.transform.SetParent(transform);

            var effectInstance = go.AddComponent<SkillEffectInstance>();
            go.AddComponent<SpriteRenderer>().sortingLayerName = "Player";
            go.GetComponent<SpriteRenderer>().sortingOrder = 10;
            go.AddComponent<BoxCollider2D>().isTrigger = true;

            effect = effectInstance;
        }
        else
        {
            effect = pool.Dequeue();
            effect.Reset();
            effect.gameObject.SetActive(true);
        }

        return effect;
    }

    public void ReturnToPool(SkillEffectInstance effect)
    {
        effect.Reset();
        effect.gameObject.SetActive(false);
        effect.transform.position = Vector3.zero;
        pool.Enqueue(effect);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
