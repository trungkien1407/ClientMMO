using System.Collections.Generic;
using UnityEngine;

public class HealthBarPool : MonoBehaviour
{
    public static HealthBarPool Instance;

    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Transform healthBarRoot;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        Instance = this;

        if (healthBarRoot == null)
        {
            GameObject rootGO = new GameObject("HealthBarRoot");
            healthBarRoot = rootGO.transform;
        }
    }

    public GameObject GetHealthBar()
    {
        GameObject healthBar;

        if (pool.Count == 0)
        {
            // Tạo mới nếu pool trống
            healthBar = Instantiate(healthBarPrefab, healthBarRoot);
        }
        else
        {
            healthBar = pool.Dequeue();
        }

        healthBar.SetActive(true);
        healthBar.transform.SetParent(healthBarRoot, false);
        return healthBar;
    }

    public void ReturnHealthBar(GameObject healthBar)
    {
        healthBar.SetActive(false);
        healthBar.transform.SetParent(healthBarRoot, false);
        pool.Enqueue(healthBar);
    }
}
