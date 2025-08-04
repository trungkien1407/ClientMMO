// File: Assets/Script/UI/ItemActionPopup.cs
using Assets.Script;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using Newtonsoft.Json;

public class ItemActionPopup : MonoBehaviour
{
    // Bỏ Singleton đi, không cần nữa.
    // public static ItemActionPopup Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Button _actionButton;
    [SerializeField] private TextMeshProUGUI _actionButtonText;
    [SerializeField] private Button _closeButton;
 

    [Header("Data")]

    private ClientInventoryItem _currentItem;
    private bool _wasClickedFromEquipmentSlot;

    // Chỉ cần Awake để gán sự kiện cho các nút của chính nó
    void Awake()
    {
        _actionButton.onClick.AddListener(OnActionButtonClick);
        _closeButton.onClick.AddListener(Hide);

        // Giữ cho nó tắt khi bắt đầu
        gameObject.SetActive(false);
    }

    // BỎ HOÀN TOÀN Start() và OnDestroy() cũ
    // private void Start() { }
    // private void OnDestroy() { }

    // Phương thức Show vẫn là bộ não, nhưng giờ nó được gọi từ bên ngoài
    public void Show(ClientInventoryItem item, bool isEquipmentSlot)
    {
        _currentItem = item;
        _wasClickedFromEquipmentSlot = isEquipmentSlot;

        var staticData = ItemDataManager.Instance.GetItemData(item.ItemID);
        if (staticData == null) return;

        // ... (code cập nhật UI và quyết định nút không đổi) ...
       
        _nameText.text = staticData.Name;
        _descriptionText.text = staticData.Description;

        if (isEquipmentSlot)
        {
            _actionButtonText.text = "Tháo ra";
        }
        else
        {
            if (staticData.Type == "Equipment") _actionButtonText.text = "Trang bị";
            else _actionButtonText.text = "Dùng";
        }

        // Quan trọng: Bật chính nó lên để hiển thị
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnActionButtonClick()
    {
        var message = new BaseMessage
        {
            Action = "use_item",
            Data = new Newtonsoft.Json.Linq.JObject
            {
                ["inventoryItemId"] = _currentItem.InventoryItemID,
            }
        };
        ClientManager.Instance.Send(JsonConvert.SerializeObject(message));
        Hide();
    }
}