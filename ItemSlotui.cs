// File: Assets/Script/ItemSlotUI.cs

using Assets.Script;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

// Không cần [RequireComponent] nữa, chúng ta sẽ tự kiểm tra
public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _quantityText;

    private ClientInventoryItem _currentItem;

    // Biến Button sẽ được khởi tạo "lười biếng"
    private Button _button;
    // Public property để truy cập an toàn vào Button
    public Button Button
    {
        get
        {
            // Nếu _button chưa được gán (vẫn là null)
            if (_button == null)
            {
                // Thì hãy lấy nó ngay bây giờ
                _button = GetComponent<Button>();
                if (_button != null)
                {
                    // Và gán sự kiện cho nó chỉ một lần duy nhất
                    _button.onClick.AddListener(HandleClick);
                }
                else
                {
                    // Nếu không tìm thấy, đây là lỗi nghiêm trọng trong thiết kế prefab
                    Debug.LogError("FATAL: Could not find Button component on ItemSlotUI prefab!", this.gameObject);
                }
            }
            return _button;
        }
    }

    public static event System.Action<ClientInventoryItem, bool> OnAnySlotClicked;
    private bool _isEquipmentSlot = false;

    // Awake không còn cần thiết để lấy button nữa
    // private void Awake() { }

    public void SetAsEquipmentSlot(bool isEquipment)
    {
        _isEquipmentSlot = isEquipment;
    }

    private void HandleClick()
    {
        if (_currentItem != null)
        {
            OnAnySlotClicked?.Invoke(_currentItem, _isEquipmentSlot);
        }
    }

    public void UpdateSlot(ClientInventoryItem item, SpriteAtlas itemAtlas)
    {
        _currentItem = item;

        // *** THAY ĐỔI QUAN TRỌNG ***
        // Truy cập Button thông qua property. 
        // Lần đầu tiên dòng này chạy, code bên trong "get" sẽ được thực thi.
        // Các lần sau, nó sẽ trả về _button đã được cache.
        if (Button == null) return; // Nếu không có button thì không làm gì cả
        Button.interactable = (_currentItem != null);

        if (_currentItem == null)
        {
            Clear();
            return;
        }

        // --- Kiểm tra an toàn cho các tham chiếu khác ---
        if (_icon == null || itemAtlas == null)
        {
            if (_icon == null) Debug.LogError("Icon reference is missing in ItemSlotUI prefab inspector.", this.gameObject);
            if (itemAtlas == null) Debug.LogError("itemAtlas passed to UpdateSlot is null.", this.gameObject);
            Clear();
            return;
        }

        _icon.enabled = true;
        _icon.sprite = itemAtlas.GetSprite(_currentItem.ItemID.ToString());

        if (_quantityText != null)
        {
            _quantityText.enabled = _currentItem.Quantity > 1;
            _quantityText.text = _quantityText.enabled ? _currentItem.Quantity.ToString() : "";
        }
    }

    public void Clear()
    {
        _currentItem = null;

        if (_icon != null)
        {
            _icon.sprite = null;
            _icon.enabled = false;
        }
        if (_quantityText != null)
        {
            _quantityText.text = "";
            _quantityText.enabled = false;
        }

        // Truy cập qua property để đảm bảo an toàn
        if (Button != null)
        {
            Button.interactable = false;
        }
    }
    public bool HasItem(int inventoryItemId)
    {
        // Trả về true chỉ khi:
        // 1. Slot không trống (_currentItem != null)
        // 2. ID của item trong slot khớp với ID được cung cấp.
        return _currentItem != null && _currentItem.InventoryItemID == inventoryItemId;
    }
}