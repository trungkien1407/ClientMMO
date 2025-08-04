using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Script;
using UnityEngine.U2D;

namespace Assets.Script
{
    /// <summary>
    /// Quản lý giao diện của Panel túi đồ.
    /// Nó lắng nghe sự kiện từ InventoryManager và sử dụng một coroutine để vẽ lại UI,
    /// đảm bảo các đối tượng được khởi tạo đầy đủ trước khi được sử dụng.
    /// </summary>
    public class InventoryPanelUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Kéo object Content của ScrollView vào đây")]
        [SerializeField] private Transform _slotsParent;

        [Tooltip("Kéo prefab của một slot item (ItemSlotUI) vào đây")]
        [SerializeField] private GameObject _itemSlotPrefab;

        [Header("Data References")]
        [Tooltip("Kéo Sprite Atlas chứa icon của các item vào đây")]
        [SerializeField] private SpriteAtlas _itemSprites;

        private List<ItemSlotUI> _slotUIs = new List<ItemSlotUI>();
        private bool _isInitialized = false;
        private Coroutine _redrawCoroutine; // Tham chiếu đến coroutine đang chạy để tránh gọi chồng chéo

        #region Unity Lifecycle & Event Handling

        private void Awake()
        {
            // Đảm bảo các Manager đã sẵn sàng trước khi đăng ký.
            // Nên sử dụng Script Execution Order để đảm bảo các Manager chạy trước.
            if (InventoryManager.Instance != null)
            {
                // Đăng ký phương thức khởi động coroutine vào sự kiện
                InventoryManager.Instance.OnInventoryChanged += HandleInventoryChange;
            }
            else
            {
                Debug.LogError("InventoryManager.Instance is NULL in Awake(). Please ensure Script Execution Order is set correctly so Managers run first.");
            }
        }

        private void OnDestroy()
        {
            // Luôn hủy đăng ký để tránh lỗi
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged -= HandleInventoryChange;
            }
        }

        /// <summary>
        /// Phương thức được gọi bởi sự kiện OnInventoryChanged.
        /// Nó sẽ dừng coroutine cũ (nếu có) và bắt đầu một coroutine mới để vẽ lại UI.
        /// </summary>
        private void HandleInventoryChange()
        {
            // Nếu có một coroutine vẽ lại đang chạy, hãy dừng nó lại trước
            if (_redrawCoroutine != null)
            {
                StopCoroutine(_redrawCoroutine);
            }
            // Bắt đầu một coroutine mới và lưu lại tham chiếu của nó
            _redrawCoroutine = StartCoroutine(RedrawCoroutine());
        }

        #endregion

        #region UI Logic

        /// <summary>
        /// Tạo ra các ô slot UI trống dựa trên MaxSlots.
        /// </summary>
        private void Initialize()
        {
            foreach (Transform child in _slotsParent)
            {
                Destroy(child.gameObject);
            }
            _slotUIs.Clear();

            if (InventoryManager.Instance == null) return;

            for (int i = 0; i < InventoryManager.Instance.MaxSlots; i++)
            {
                GameObject slotGO = Instantiate(_itemSlotPrefab, _slotsParent);
                var slotUI = slotGO.GetComponent<ItemSlotUI>();
                if (slotUI != null)
                {
                    slotUI.SetAsEquipmentSlot(false);
                    _slotUIs.Add(slotUI);
                }
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Coroutine để vẽ lại toàn bộ giao diện túi đồ một cách an toàn.
        /// </summary>
        private IEnumerator RedrawCoroutine()
        {
            // 1. Kiểm tra an toàn
            if (InventoryManager.Instance == null || EquipmentManager.Instance == null)
            {
                Debug.LogWarning("Cannot Redraw Inventory: A required Manager is not ready.");
                yield break; // Thoát khỏi coroutine
            }

            // 2. Khởi tạo UI nếu cần
            if (!_isInitialized || _slotUIs.Count != InventoryManager.Instance.MaxSlots)
            {
                Initialize();

                // *** ĐIỂM SỬA LỖI QUAN TRỌNG ***
                // Sau khi Instantiate các đối tượng mới, đợi đến cuối frame.
                // Điều này cho phép Unity gọi phương thức Awake() trên các ItemSlotUI mới.
                yield return new WaitForEndOfFrame();
            }

            // 3. Lấy và lọc dữ liệu. 
            // Code chạy đến đây có nghĩa là các slot UI đã sẵn sàng và an toàn để sử dụng.
            var allItems = InventoryManager.Instance.GetAllItems();
            var itemsToShow = new List<ClientInventoryItem>();
            foreach (var item in allItems)
            {
                if (!EquipmentManager.Instance.IsItemEquipped(item.InventoryItemID))
                {
                    itemsToShow.Add(item);
                }
            }

            // 4. Điền dữ liệu vào các ô slot
            for (int i = 0; i < _slotUIs.Count; i++)
            {
                if (i < itemsToShow.Count)
                {
                    _slotUIs[i].UpdateSlot(itemsToShow[i], _itemSprites);
                }
                else
                {
                    _slotUIs[i].Clear();
                }
            }

            _redrawCoroutine = null; // Đánh dấu là coroutine đã chạy xong
        }

        #endregion
    }
}