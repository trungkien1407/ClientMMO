using UnityEngine;
using System.Collections;

public class ItemMovement : MonoBehaviour
{
    private bool isMoving = false;
    private Transform target; // Người chơi sẽ là mục tiêu
    private float moveSpeed = 10f; // Tốc độ item bay về, có thể điều chỉnh

    // Hàm được gọi từ bên ngoài để bắt đầu hiệu ứng
    public void StartMoveToTarget(Transform targetTransform)
    {
        if (targetTransform == null || isMoving)
        {
            // Nếu không có mục tiêu hoặc đang di chuyển rồi thì hủy luôn
            ReturnToPool();
            return;
        }

        this.target = targetTransform;
        this.isMoving = true;

        // Vô hiệu hóa collider để không thể target vào nó nữa
        GetComponent<Collider2D>().enabled = false;
    }

    void Update()
    {
        if (!isMoving || target == null) return;

        // Di chuyển item về phía mục tiêu
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        // Kiểm tra nếu đã đến rất gần mục tiêu
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            // Khi đến nơi, trả item về pool
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        isMoving = false;
        target = null;

        // Kích hoạt lại collider để có thể tái sử dụng
        GetComponent<Collider2D>().enabled = true;

        // Gọi hàm của DroppedItem để trả về pool
        GetComponent<DroppedItem>()?.ReturnToPool();
    }
}