using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TargetInfor : MonoBehaviour
{
    public TextMeshProUGUI TxtName;
    public TextMeshProUGUI TxtHealth;
    public GameObject HealthBarGroup; // Group chứa thanh máu và text máu

    public Health health;

    public void SetTarget(Health health)
    {
        this.health = health;
        gameObject.SetActive(true); // Hiển thị panel thông tin
    }

    void Update()
    {
        if (health == null || !health.gameObject.activeInHierarchy)
        {
            Clear();
            return;
        }

        // Cập nhật tên
        TxtName.text = health.TargetName;

        // Kiểm tra tag của target
        if (health.CompareTag("Item"))
        {
            // Nếu là item, ẩn thông tin máu
            if (HealthBarGroup != null) HealthBarGroup.SetActive(false);
            TxtHealth.text = "Item"; // Hoặc để trống
        }
        else
        {
            // Nếu là quái, người, NPC, hiển thị thông tin máu
            if (HealthBarGroup != null) HealthBarGroup.SetActive(true);
            TxtHealth.text = $"{health.CurrentHealth}/{health.MaxHealth}";
        }
    }

    public void Clear()
    {
        this.health = null;
        gameObject.SetActive(false); // Ẩn panel thông tin
    }
}