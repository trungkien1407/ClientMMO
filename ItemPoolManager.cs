using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D; // Bắt buộc phải có để dùng SpriteAtlas

public class ItemPoolManager : MonoBehaviour
{
    public static ItemPoolManager Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private int initialPoolSize = 20; // Tăng size lên một chút

    [Header("Assets")]
    [SerializeField] private SpriteAtlas itemAtlas;

    [Header("Dependencies")]
    [SerializeField] private HealthBar healthBarPrefab;

    private Queue<GameObject> itemPool = new Queue<GameObject>();
    private Queue<HealthBar> healthBarPool = new Queue<HealthBar>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (itemAtlas == null || itemPrefab == null || healthBarPrefab == null)
        {
            Debug.LogError("One or more required prefabs/assets are not assigned in ItemPoolManager!", this);
        }

        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject itemObject = Instantiate(itemPrefab);
            SetupHealthBarForItem(itemObject);
            itemObject.transform.SetParent(this.transform, false);
            itemObject.SetActive(false);
            itemPool.Enqueue(itemObject);
        }
    }

    public GameObject DropNewItem(string uniqueId, int staticId, string name, string spriteNameInAtlas, Vector3 position)
    {
        if (itemPool.Count == 0)
        {
            Debug.LogWarning("Item pool is empty. Creating a new item on the fly.");
            GameObject newItem = Instantiate(itemPrefab);
            SetupHealthBarForItem(newItem);
            newItem.transform.SetParent(this.transform, false);
            itemPool.Enqueue(newItem);
        }

        GameObject itemObject = itemPool.Dequeue();

        // --- BƯỚC SỬA 1: KÍCH HOẠT LẠI HEALTHBAR TƯƠNG ỨNG ---
        Health healthComponent = itemObject.GetComponent<Health>();
        if (healthComponent != null && healthComponent.healthBar != null)
        {
            healthComponent.healthBar.gameObject.SetActive(true);
            // Cập nhật lại target cho healthbar phòng trường hợp có lỗi
            healthComponent.healthBar.SetupForItem(itemObject.transform, healthComponent);
        }

        // Tìm sprite
        Sprite itemSprite = itemAtlas.GetSprite(spriteNameInAtlas);
        if (itemSprite == null)
        {
            Debug.LogWarning($"Sprite with name '{spriteNameInAtlas}' not found in Item Atlas.");
        }

        // Cấu hình item
        itemObject.transform.position = position;
        itemObject.GetComponent<DroppedItem>()?.Initialize(uniqueId, staticId, name, itemSprite);

        // Kích hoạt item
        itemObject.SetActive(true);

        return itemObject;
    }

    // --- BƯỚC SỬA 2: QUẢN LÝ HEALTHBAR KHI TRẢ ITEM VỀ POOL ---
    public void ReturnItem(GameObject itemObject)
    {
        // Vô hiệu hóa và trả HealthBar của item này về pool của nó
        Health healthComponent = itemObject.GetComponent<Health>();
        if (healthComponent != null && healthComponent.healthBar != null)
        {
            ReturnHealthBar(healthComponent.healthBar);
        }

        // Vô hiệu hóa và trả item về pool của nó
        itemObject.SetActive(false);
        itemPool.Enqueue(itemObject);
    }

    private void SetupHealthBarForItem(GameObject itemObject)
    {
        Health healthComponent = itemObject.GetComponent<Health>();
        if (healthComponent == null) return;

        // Nếu item này đã có healthbar (khi tạo thêm lúc hết pool), không cần tạo mới
        if (healthComponent.healthBar != null) return;

        HealthBar newHealthBar = GetHealthBar();
        newHealthBar.transform.SetParent(this.transform, false);
        newHealthBar.offset = new Vector3(0, 0.2f, 0);
        // Gán tham chiếu 2 chiều
        healthComponent.healthBar = newHealthBar;
        newHealthBar.SetupForItem(itemObject.transform, healthComponent);
    }

    public HealthBar GetHealthBar()
    {
        if (healthBarPool.Count > 0)
        {
            HealthBar hb = healthBarPool.Dequeue();
            // Không cần SetActive(true) ở đây, nơi gọi sẽ quyết định
            return hb;
        }
        return Instantiate(healthBarPrefab);
    }

    public void ReturnHealthBar(HealthBar healthBar)
    {
        if (healthBar != null)
        {
            healthBar.ResetState(); // ResetState đã bao gồm SetActive(false)
            healthBarPool.Enqueue(healthBar);
        }
    }
}