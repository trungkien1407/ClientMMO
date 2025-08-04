using Newtonsoft.Json.Linq;
using System.Collections;
using TMPro; // Sử dụng thư viện TextMeshPro
using UnityEngine;

public class ToastManager : MonoBehaviour
{
    public static ToastManager Instance;

    [Header("UI References")]
    public GameObject toastPanel; // Kéo Panel từ Hierarchy vào đây
    public TMP_Text toastText;    // Kéo đối tượng TextMeshPro từ Hierarchy vào đây

    [Header("Settings")]
    public float displayDuration = 3.0f; // Thời gian hiển thị trước khi tự ẩn (tăng lên 3s cho dễ đọc)

    private Coroutine _toastCoroutine; // Biến để theo dõi coroutine đang chạy

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Đảm bảo panel đã tắt khi bắt đầu
            if (toastPanel != null)
            {
                toastPanel.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Đăng ký handler khi script được bật
    private void Start()
    {
        ClientManager.Instance.Dispatcher.RegisterHandler("show_toast", ToastHandle);
    }

    // Hủy đăng ký handler khi script bị tắt hoặc hủy


    public void ToastHandle(JObject data)
    {
        // *** SỬA LỖI 2: TRIỂN KHAI HÀM HANDLE ***

        // Phân tích dữ liệu JSON nhận được
        string message = data["text"]?.Value<string>() ?? "No message";
        string type = data["type"]?.Value<string>() ?? "info";

        // Sau khi có dữ liệu, gọi hàm ShowToast để hiển thị lên UI
        ShowToast(message, type);
    }

    /// <summary>
    /// Hàm nội bộ để hiển thị toast, không cần gọi trực tiếp từ bên ngoài nữa.
    /// </summary>
    public void ShowToast(string message, string type)
    {
        // Nếu có một coroutine đang chạy (tức là đang hiển thị một toast cũ),
        // hãy dừng nó lại.
        if (_toastCoroutine != null)
        {
            StopCoroutine(_toastCoroutine);
        }

        // Bắt đầu một coroutine mới để hiển thị toast mới.
        _toastCoroutine = StartCoroutine(ToastLifecycle(message, type));
    }

    private IEnumerator ToastLifecycle(string message, string type)
    {
        // 1. Cập nhật nội dung và màu sắc
        toastText.text = message;
        toastText.color = GetColorForType(type);

        // 2. Hiển thị panel
        toastPanel.SetActive(true);

        // 3. Chờ trong một khoảng thời gian
        yield return new WaitForSeconds(displayDuration);

        // 4. Sau khi chờ xong, ẩn panel đi
        toastPanel.SetActive(false);

        // 5. Đặt coroutine về null để báo hiệu nó đã kết thúc
        _toastCoroutine = null;
    }

    private Color GetColorForType(string type)
    {
        switch (type)
        {
            case "item_received": return Color.yellow;
            case "error": return Color.red;
            case "info": return new Color(1f, 0.92f, 0.016f); // Vàng
            default: return Color.white;
        }
    }
}