using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Stats")]
    public int MaxHealth = 100;
    public int MaxMana = 100;
    public int CurrentHealth;
    public int CurrentMana;
    public string TargetName;
    public int id;

    [Header("Loot Information (For Monsters)")]
    public int dropItemID;
    public string dropItemName;
    public string dropItemSpriteName; // Tên của sprite bên trong Sprite Atlas

    [Header("Dependencies")]
    public PlayerHUD playerHUD; // Gán nếu đây là player
    [HideInInspector] public HealthBar healthBar; // Được quản lý bởi Pool Manager

    void Start()
    {
        // Khởi tạo máu khi bắt đầu
        CurrentHealth = MaxHealth;
        CurrentMana = MaxMana;
    }

    public void SetId(int newId)
    {
        this.id = newId;
    }

    public void SetName(string newName)
    {
        this.TargetName = newName;
    }

    public void SetTargeted(bool isTargeted)
    {
        if (healthBar != null)
        {
            healthBar.SetTargeted(isTargeted);
        }
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
    }

    public void UpdateHealthMana(int health, int mana)
    {
        CurrentHealth = health;
        CurrentMana = mana;
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} is dead.");
        //SetTargeted(false);

        //// --- Logic rơi item ---
        //// Chỉ rơi item nếu đây là quái vật (có dropItemID > 0)
        //if (ItemPoolManager.Instance != null && dropItemID > 0)
        //{
        //    ItemPoolManager.Instance.DropNewItem(dropItemID, dropItemName, dropItemSpriteName, transform.position);
        //}

        //// Trả healthbar về pool
        //if (ItemPoolManager.Instance != null && healthBar != null)
        //{
        //    ItemPoolManager.Instance.ReturnHealthBar(healthBar);
        //}

        //// Vô hiệu hóa đối tượng (hoặc trả về pool quái vật nếu có)
        //gameObject.SetActive(false);
    }

    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
    }
}