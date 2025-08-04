using System.Collections.Generic;
using UnityEngine;

namespace Assets.Script
{
    public class PlayerPool
    {
        private GameObject malePrefab;
        private GameObject femalePrefab;

        private Queue<GameObject> malePool = new Queue<GameObject>();
        private Queue<GameObject> femalePool = new Queue<GameObject>();
        private Rigidbody2D rb;
        private RemoteController remoteController;
        private Transform poolRoot; // Tạo object cha để chứa pool (cho sạch hierarchy)

        public PlayerPool(GameObject malePrefab, GameObject femalePrefab)
        {
            this.malePrefab = malePrefab;
            this.femalePrefab = femalePrefab;

            poolRoot = new GameObject("PlayerPool").transform;
        }

        // Lấy player từ pool, nếu không có thì Instantiate mới
        public GameObject GetPlayer(bool isMale)
        {
            Queue<GameObject> pool = isMale ? malePool : femalePool;
            if (pool.Count > 0)
            {
                var player = pool.Dequeue();
                rb = player.GetComponent<Rigidbody2D>();
                remoteController = player.GetComponent<RemoteController>();
                remoteController.ResetState();
                rb.linearVelocity = Vector3.zero;
                player.SetActive(true);
                return player;
            }
            else
            {
                var prefab = isMale ? malePrefab : femalePrefab;
                var player = GameObject.Instantiate(prefab);
                player.transform.SetParent(poolRoot); // Đặt dưới poolRoot để quản lý
                return player;
            }
        }

        // Trả player về pool
        public void ReturnPlayer(GameObject player, bool isMale)
        {
            player.SetActive(false);
            player.transform.SetParent(poolRoot);
            player.transform.position = Vector3.zero;
            rb = player.GetComponent<Rigidbody2D>();
            remoteController = player.GetComponent<RemoteController>();
            remoteController.ResetState();
            rb.linearVelocity = Vector2.zero;

            if (isMale)
                malePool.Enqueue(player);
            else
                femalePool.Enqueue(player);
        }

        // Xóa toàn bộ pool (nếu cần)
        public void ClearPool()
        {
            foreach (var p in malePool)
                GameObject.Destroy(p);
            malePool.Clear();

            foreach (var p in femalePool)
                GameObject.Destroy(p);
            femalePool.Clear();
        }
    }
}
