using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script
{
    public class MonsterPool
    {
        private GameObject monsterPrefabs;
        private Queue<GameObject> monsterPool = new Queue<GameObject>();
        private Transform poolRoot;

        public MonsterPool(GameObject Monster)
        {
            monsterPrefabs = Monster;
            poolRoot = new GameObject("MonsterPool").transform;
        }

        public GameObject GetMonster()
        {
            GameObject obj;

            if (monsterPool.Count > 0)
            {
                obj = monsterPool.Dequeue();
                obj.SetActive(true);
                Debug.Log($"[Pool] Lấy monster từ pool, reset transform");
            }
            else
            {
                obj = GameObject.Instantiate(monsterPrefabs);
                Debug.Log($"[Pool] Tạo monster mới từ prefab");
            }

            // QUAN TRỌNG: Reset hoàn toàn transform khi lấy từ pool
            obj.transform.SetParent(null); // Tách khỏi pool root trước
            obj.transform.position = Vector3.zero;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            // Reset tất cả components về trạng thái ban đầu
            ResetMonsterComponents(obj);

            return obj;
        }

        private void ResetMonsterComponents(GameObject monster)
        {
            // Reset Health
            var health = monster.GetComponent<Health>();
            if (health != null)
            {
                health.CurrentHealth = health.MaxHealth;
            }

            // Reset Movement
            var movement = monster.GetComponent<MonsterMovement>();
            if (movement != null)
            {
                movement.ResetToPosition(Vector2.zero);
            }

            // Reset Visual/Animation
            var visual = monster.GetComponent<MonsterVisual>();
            if (visual != null)
            {
               // visual.ResetToIdle(); // Cần implement method này
            }

            // Reset Animator nếu có
            var animator = monster.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
            }

            Debug.Log($"[Pool] Đã reset tất cả components cho monster");
        }

        public void ReturnMonster(GameObject monster)
        {
            if (monster == null) return;

            Debug.Log($"[Pool] Trả monster về pool, vị trí: {monster.transform.position}");

            // Tách tất cả children (như HealthBar) trước khi return
            for (int i = monster.transform.childCount - 1; i >= 0; i--)
            {
                var child = monster.transform.GetChild(i);
                if (child.GetComponent<HealthBar>() != null)
                {
                    // HealthBar sẽ được handle bởi MonsterManager
                    continue;
                }
            }

            // Reset transform hoàn toàn
            monster.transform.SetParent(poolRoot);
            monster.transform.position = Vector3.zero;
            monster.transform.localPosition = Vector3.zero;
            monster.transform.rotation = Quaternion.identity;
            monster.transform.localScale = Vector3.one;

            // Reset components
            ResetMonsterComponents(monster);

            // Deactivate và return vào pool
            monster.SetActive(false);
            monsterPool.Enqueue(monster);

            Debug.Log($"[Pool] Monster đã được trả về pool và reset hoàn toàn");
        }

        public void ClearPool()
        {
            foreach (var monster in monsterPool)
            {
                if (monster != null)
                    GameObject.Destroy(monster);
            }
            monsterPool.Clear();

            if (poolRoot != null)
                GameObject.Destroy(poolRoot.gameObject);
        }
    }
}