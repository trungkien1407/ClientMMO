// File: Assets/Script/Managers/EquipmentManager.cs
using Assets.Script;
using UnityEngine;
using UnityEngine.U2D;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    // Kéo các slot UI từ Hierarchy vào các trường này trong Inspector
    [Header("UI Slots")]
    [SerializeField] private ItemSlotUI _weaponSlot;
    [SerializeField] private ItemSlotUI _aoSlot; // Áo
    [SerializeField] private ItemSlotUI _quanSlot; // Quần
    [SerializeField] private ItemSlotUI _giaySlot; // Giày
    [SerializeField] private ItemSlotUI _gangTaySlot; // Găng tay
    [SerializeField] private ItemSlotUI _buaSlot; // Bùa

    // Kéo Sprite Atlas vào đây, giống như InventoryPanelUI
    [Header("Data")]
    [SerializeField] private SpriteAtlas _itemSprites;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        _weaponSlot.SetAsEquipmentSlot(true);
        _aoSlot.SetAsEquipmentSlot(true);
        _quanSlot.SetAsEquipmentSlot(true);
        _giaySlot.SetAsEquipmentSlot(true);
        _gangTaySlot.SetAsEquipmentSlot(true);
        _buaSlot.SetAsEquipmentSlot(true);
    }

    // Phương thức này sẽ được gọi bởi InventoryManager
    public void UpdateEquipment(ClientEquipmentData equipData)
    {
        // Lấy InventoryManager để tìm item từ InventoryItemID
        var inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager is not available to look up items for equipment.");
            return;
        }

        // Cập nhật từng slot
        UpdateSingleSlot(_weaponSlot, equipData.WeaponSlot_InventoryItemID, inventoryManager);
        UpdateSingleSlot(_aoSlot, equipData.AoSlot_InventoryItemID, inventoryManager);
        UpdateSingleSlot(_quanSlot, equipData.QuanSlot_InventoryItemID, inventoryManager);
        UpdateSingleSlot(_giaySlot, equipData.GiaySlot_InventoryItemID, inventoryManager);
        UpdateSingleSlot(_gangTaySlot, equipData.GangTaySlot_InventoryItemID, inventoryManager);
        UpdateSingleSlot(_buaSlot, equipData.BuaSlot_InventoryItemID, inventoryManager);
    }

    private void UpdateSingleSlot(ItemSlotUI slotUI, int? inventoryItemId, InventoryManager invManager)
    {
        if (slotUI == null) return; // Bỏ qua nếu slot chưa được gán

        // Nếu có ID của item trong slot
        if (inventoryItemId.HasValue)
        {
            // Tìm item tương ứng trong túi đồ bằng InventoryItemID
            ClientInventoryItem item = invManager.GetItemByInventoryId(inventoryItemId.Value);

            // Cập nhật slot UI với item tìm được
            slotUI.UpdateSlot(item, _itemSprites);
        }
        else
        {
            // Nếu không có item nào được trang bị (ID là null), xóa slot
            slotUI.Clear();
        }
    }
    public bool IsItemEquipped(int inventoryItemId)
    {
        // Kiểm tra lần lượt từng slot trang bị.
        // Nếu tìm thấy ở bất kỳ đâu, trả về true ngay lập tức.
        if (_weaponSlot != null && _weaponSlot.HasItem(inventoryItemId)) return true;
        if (_aoSlot != null && _aoSlot.HasItem(inventoryItemId)) return true;
        if (_quanSlot != null && _quanSlot.HasItem(inventoryItemId)) return true;
        if (_giaySlot != null && _giaySlot.HasItem(inventoryItemId)) return true;
        if (_gangTaySlot != null && _gangTaySlot.HasItem(inventoryItemId)) return true;
        if (_buaSlot != null && _buaSlot.HasItem(inventoryItemId)) return true;

        // Nếu đã kiểm tra hết tất cả các slot mà không tìm thấy, trả về false.
        return false;
    }

}