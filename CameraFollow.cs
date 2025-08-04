using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;


    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Grid grid; // Gán Grid chứa Tilemaps vào đây

        public float smoothSpeed = 4f;

        private Vector3 minBounds;
        private Vector3 maxBounds;
        private float halfHeight;
        private float halfWidth;

        void Start()
        {
            TilemapLoader.Instance.onLoad += StartFollowing; // Đăng ký sự kiện khi bản đồ đã tải
        }

        void StartFollowing()
        {
            Camera cam = Camera.main;
            halfHeight = cam.orthographicSize;
            halfWidth = halfHeight * cam.aspect;

            grid = TilemapLoader.Instance.Grid;

            if (grid == null)
            {
                Debug.LogError("CameraFollow: Grid is null after map load.");
                return;
            }

            StartCoroutine(CalculateMapBounds());
        }


        private Vector3 velocity = Vector3.zero;

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, transform.position.z);

            float clampedX = Mathf.Clamp(desiredPosition.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
            float clampedY = Mathf.Clamp(desiredPosition.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);

            Vector3 clampedPosition = new Vector3(clampedX, clampedY, desiredPosition.z);
            transform.position = Vector3.SmoothDamp(transform.position, clampedPosition, ref velocity, 0.05f); // 0.05s để mượt
        }

        private IEnumerator CalculateMapBounds()
        {
            // Đợi 1 frame để tilemap hoàn tất setup
            yield return new WaitForEndOfFrame();

            Tilemap[] tilemaps = grid.GetComponentsInChildren<Tilemap>();
            Debug.Log("Tilemaps found: " + tilemaps.Length);

            if (tilemaps.Length == 0)
            {
                Debug.LogWarning("No tilemaps found in grid!");
                yield break;
            }

            Bounds combinedBounds = tilemaps[0].localBounds;
            foreach (Tilemap tm in tilemaps)
            {
                combinedBounds.Encapsulate(tm.localBounds);
            }

            minBounds = combinedBounds.min;
            maxBounds = combinedBounds.max;

            Debug.Log($"Map bounds calculated: Min = {minBounds}, Max = {maxBounds}");
        }
    }

    
