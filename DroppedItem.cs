using UnityEngine;

[RequireComponent(typeof(Health), typeof(SpriteRenderer))]
public class DroppedItem : MonoBehaviour
{
    public int ItemId { get; private set; }
    public string ItemName { get; private set; }
    public string GroundItemID { get; private set; } // ID duy nhất trên mặt đất

    private Health healthComponent;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        healthComponent = GetComponent<Health>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(string GroundItemID,int id, string name, Sprite itemSprite)
    {
        this.ItemId = id;
        this.ItemName = name;
        this.GroundItemID = GroundItemID;

        if (healthComponent != null)
        {
            healthComponent.SetId(id);
            healthComponent.SetName(name);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = itemSprite;
        }
    }

    // Hàm này có thể được gọi bởi một script khác, ví dụ sau 1 thời gian item tự biến mất
    public void ReturnToPool()
    {
        if (ItemPoolManager.Instance != null)
        {
            ItemPoolManager.Instance.ReturnItem(gameObject);
        }
        else
        {
            gameObject.SetActive(false); // Fallback
        }
    }
}