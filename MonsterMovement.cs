using Assets.Script;
using TMPro;
using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
    private Vector2 startPos;
    private Vector2 endPos;
    private float startTime;
    private float pausedTime;
    private float duration = 1f;
    private bool isMoving = false;
    private bool isPaused = false;

    private float pausedAtProgress = 0f;

    public void SetTarget(Vector2 from, Vector2 to, long serverTimestamp)
    {
        startPos = from;
        endPos = to;
        transform.position = startPos;
        startTime = Time.time;
        duration = 1f;
        isMoving = true;
        isPaused = false;

        Vector2 direction = (endPos - startPos).normalized;
        UpdateDirection(direction);
    }

    public void PauseMovement()
    {
        if (!isMoving || isPaused) return;

        pausedTime = Time.time;
        pausedAtProgress = (pausedTime - startTime) / duration;
        isPaused = true;
    }

    public void ResumeMovement()
    {
        if (!isMoving || !isPaused) return;

        startTime = Time.time - pausedAtProgress * duration;
        isPaused = false;
    }

    private void Update()
    {
        if (!isMoving || isPaused) return;

        float elapsed = Time.time - startTime;
        float t = elapsed / duration;

        if (t >= 1f)
        {
            transform.position = endPos;
            isMoving = false;
        }
        else
        {
            transform.position = Vector2.Lerp(startPos, endPos, t);
        }
    }

    private void UpdateDirection(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > 0.01f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(dir.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
    private void LaunchProjectile(Vector3 startPos, Vector3 targetPos)
    {
        var projGO = ProjectilePool.Instance.GetProjectile();
        projGO.transform.position = startPos;

        var projectile = projGO.GetComponent<Projectile>();
        projectile.Setup(targetPos, () =>
        {
            // Đây là nơi bạn có thể thêm hiệu ứng trúng đòn
            Debug.Log("Projectile hit target!");

            // Ví dụ: tạo hit effect hoặc rung camera
        });
    }
    //public void ResetMovement()
    //{
    //    // Reset các biến movement về trạng thái ban đầu
    //    isMoving = false;

    //    // Nếu có tween hoặc coroutine đang chạy thì stop
    //    if (moveCoroutine != null)
    //    {
    //        StopCoroutine(moveCoroutine);
    //        moveCoroutine = null;
    //    }

    //    // Reset velocity nếu có Rigidbody2D
    //    var rb = GetComponent<Rigidbody2D>();
    //    if (rb != null)
    //    {
    //        rb.velocity = Vector2.zero;
    //    }

    //    Debug.Log($"Reset movement for monster at position {transform.position}");
    //}
    public void ResetToPosition(Vector2 position)
    {
        StopAllCoroutines(); // nếu có coroutine
        isMoving = false;
        isPaused = false;

        startPos = position;
        endPos = position;
        transform.position = new Vector3(position.x, position.y, transform.position.z);

        Debug.Log($"[Movement] Reset monster to position ({position.x:F2}, {position.y:F2})");
    }

}
