using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script
{
    public class Projectile : MonoBehaviour
    {
        public float speed = 5f;
        private Vector3 targetPosition;
        private Action onHitCallback;

        public void Setup(Vector3 target, Action onHit)
        {
            targetPosition = target;
            onHitCallback = onHit;

            Vector3 direction = (targetPosition - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Trừ 90 độ vì viên đạn mặc định dọc theo trục Y
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }


        void Update()
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                onHitCallback?.Invoke();
                ProjectilePool.Instance.ReturnProjectile(gameObject);
            }
        }
    }

}
