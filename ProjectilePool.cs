using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance;

    [SerializeField] GameObject projectilePrefab;
    [SerializeField] int initialSize = 5;

    [SerializeField] Transform poolRoot; // <- Thêm cái này

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewProjectile();
        }
    }

    private void CreateNewProjectile()
    {
        var obj = Instantiate(projectilePrefab, poolRoot); // <- Gắn vào poolRoot
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    public GameObject GetProjectile()
    {
        if (pool.Count == 0)
        {
            CreateNewProjectile();
        }

        var projectile = pool.Dequeue();
        projectile.SetActive(true);
        return projectile;
    }

    public void ReturnProjectile(GameObject projectile)
    {
        projectile.SetActive(false);
        projectile.transform.SetParent(poolRoot); // <- Gắn lại về poolRoot khi trả về
        pool.Enqueue(projectile);
    }
}
