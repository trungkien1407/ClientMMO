using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq; // Cần thiết cho .ToList()
using Newtonsoft.Json.Linq;

namespace Assets.Script
{
    /// <summary>
    /// Quản lý dữ liệu túi đồ của người chơi ở phía client.
    /// Đây là "nguồn chân lý" cho tất cả các item mà nhân vật sở hữu.
    /// Nó nhận dữ liệu từ server và kích hoạt sự kiện để các UI khác cập nhật.
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        // Dùng Dictionary để lưu trữ item, với key là SlotNumber để dễ dàng truy cập
        private Dictionary<int, ClientInventoryItem> _inventoryItems = new Dictionary<int, ClientInventoryItem>();

        // Số lượng ô tối đa trong túi đồ, sẽ được cập nhật từ server
        public int MaxSlots { get; private set; } = 30;

        /// <summary>
        /// Sự kiện được kích hoạt sau khi dữ liệu túi đồ VÀ trang bị đã được cập nhật xong.
        /// Các UI Panel (như InventoryPanelUI) sẽ lắng nghe sự kiện này để vẽ lại giao diện.
        /// </summary>
        public event Action OnInventoryChanged;

        #region Unity Lifecycle Methods

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Đăng ký handler để nhận message từ server
            if (ClientManager.Instance != null && ClientManager.Instance.Dispatcher != null)
            {
                ClientManager.Instance.Dispatcher.RegisterHandler("get_inventory_equip", InventoryHandle);
            }
            else
            {
                Debug.LogError("ClientManager or its Dispatcher is not ready. Cannot register handler for inventory.");
            }
        }

        #endregion

        #region Data Handling

        /// <summary>
        /// Handler chính, được gọi khi nhận được message `get_inventory_equip` từ server.
        /// Nó sẽ phân tích, cập nhật tất cả dữ liệu liên quan, và cuối cùng kích hoạt sự kiện cho UI.
        /// </summary>
        public void InventoryHandle(JObject data)
        {
            // === BƯỚC 1: PHÂN TÍCH DỮ LIỆU THÔ TỪ SERVER ===
            var inventoryDataToken = data["inventory"];
            var equipDataToken = data["equip"];

            if (inventoryDataToken == null || equipDataToken == null)
            {
                Debug.LogError("Received invalid inventory/equip data structure from server.");
                return;
            }

            // Phân tích dữ liệu túi đồ
            var itemsToken = inventoryDataToken["Items"];
            int maxSlotsFromServer = inventoryDataToken["MaxSlots"].Value<int>();
            var receivedItems = new List<ClientInventoryItem>();
            if (itemsToken != null)
            {
                var itemsDictionary = itemsToken.ToObject<Dictionary<string, ClientInventoryItem>>();
                receivedItems = itemsDictionary.Values.ToList();
            }

            // Phân tích dữ liệu trang bị
            ClientEquipmentData equipmentData = equipDataToken.ToObject<ClientEquipmentData>();

            // === BƯỚC 2: CẬP NHẬT TRẠNG THÁI CỦA CÁC MANAGER (KHÔNG KÍCH HOẠT SỰ KIỆN) ===

            // Cập nhật dữ liệu nội bộ của InventoryManager
            UpdateInventoryData(receivedItems, maxSlotsFromServer);

            // Cập nhật dữ liệu cho EquipmentManager
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.UpdateEquipment(equipmentData);
                Debug.Log("EquipmentManager data has been updated.");
            }
            else
            {
                Debug.LogWarning("EquipmentManager not found. Could not update equipment data.");
            }

            // === BƯỚC 3: KÍCH HOẠT SỰ KIỆN ĐỂ THÔNG BÁO CHO UI VẼ LẠI ===
            // Sau khi TẤT CẢ các manager đã có dữ liệu mới nhất và nhất quán,
            // bây giờ chúng ta mới gửi tín hiệu cho UI để chúng bắt đầu quá trình vẽ lại.
            Debug.Log("All data managers updated. Invoking OnInventoryChanged to redraw UI.");
            OnInventoryChanged?.Invoke();
        }

        /// <summary>
        /// Cập nhật dữ liệu túi đồ nội bộ một cách "im lặng" (không kích hoạt sự kiện).
        /// </summary>
        private void UpdateInventoryData(List<ClientInventoryItem> items, int maxSlots)
        {
            _inventoryItems.Clear();
            MaxSlots = maxSlots;
            foreach (var item in items)
            {
                // Key của dictionary là SlotNumber của item.
                _inventoryItems[item.SlotNumber] = item;
            }
        }

        #endregion

        #region Public Accessors (Hàm để các lớp khác lấy dữ liệu)

        /// <summary>
        /// Lấy một item cụ thể trong một ô (slot) của túi đồ.
        /// </summary>
        public ClientInventoryItem GetItemInSlot(int slotNumber)
        {
            _inventoryItems.TryGetValue(slotNumber, out var item);
            return item;
        }

        /// <summary>
        /// Lấy một item cụ thể dựa trên ID duy nhất của nó trong túi đồ (InventoryItemID).
        /// </summary>
        public ClientInventoryItem GetItemByInventoryId(int inventoryItemId)
        {
            foreach (var item in _inventoryItems.Values)
            {
                if (item.InventoryItemID == inventoryItemId)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// Trả về một danh sách chứa TẤT CẢ các vật phẩm hiện đang có trong túi đồ.
        /// </summary>
        public List<ClientInventoryItem> GetAllItems()
        {
            return _inventoryItems.Values.ToList();
        }

        #endregion
    }
}