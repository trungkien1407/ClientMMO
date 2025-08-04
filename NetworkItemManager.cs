using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq; // Hoặc thư viện JSON bạn đang dùng

public class NetworkItemManager : MonoBehaviour
{
    public static NetworkItemManager Instance { get; private set; }

    // Dictionary vẫn giữ nguyên để quản lý các item đang hoạt động
    private Dictionary<string, GameObject> activeDroppedItems = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }
    }

    void Start()
    {
        ClientManager.Instance.Dispatcher.RegisterHandler("ground_item_spawned", HandleItemSpawned);
        ClientManager.Instance.Dispatcher.RegisterHandler("ground_item_picked_up", HandleItemPickedUp);
        ClientManager.Instance.Dispatcher.RegisterHandler("ground_item_despawn", HandleItemDespawn);
    }

    // Hàm được gọi bởi hệ thống mạng khi có gói tin ITEM_SPAWNED
    // using Newtonsoft.Json.Linq; // Cần thêm namespace này để dùng JObject, JToken...
    // using System.Collections.Generic;
    // using UnityEngine;

    public void HandleItemSpawned(JObject data) // Hàm nhận trực tiếp JObject
    {
        if (data == null) return;

        // 1. Lấy JToken tương ứng với mảng "items"
        JToken itemsToken = data["items"];

        // 2. Kiểm tra xem token có tồn tại và có phải là một mảng không
        if (itemsToken == null || itemsToken.Type != JTokenType.Array)
        {
          
            return;
        }

        // 3. Lặp qua từng JObject trong mảng
        foreach (JToken itemToken in itemsToken.AsJEnumerable())
        {
            // Mỗi itemToken bây giờ là một JObject đại diện cho một item
            // Ví dụ: { "uniqueId": "...", "itemId": 101, ... }

            // 4. Trích xuất dữ liệu một cách an toàn
            string uniqueId = itemToken["uniqueId"]?.Value<string>();
            int? itemId = itemToken["itemId"]?.Value<int>(); // Dùng nullable để an toàn
            float? x = itemToken["x"]?.Value<float>();
            float? y = itemToken["y"]?.Value<float>();

            // Kiểm tra nếu các trường bắt buộc bị thiếu
            if (string.IsNullOrEmpty(uniqueId) || !itemId.HasValue || !x.HasValue || !y.HasValue)
            {
                Debug.LogWarning("Skipping an item in spawn list due to missing required fields.");
                continue;
            }

            // Kiểm tra trùng lặp
            if (activeDroppedItems.ContainsKey(uniqueId)) continue;

            // Từ đây, logic giống hệt như trước
            string spriteName = itemId.Value.ToString();
            string itemName = $"Item [{itemId.Value}]";

            Debug.Log($"Spawning item ID: {itemId.Value} at ({x.Value}, {y.Value})");

            Vector3 spawnPosition = new Vector3(x.Value, y.Value, 0);

            GameObject itemObject = ItemPoolManager.Instance.DropNewItem(
                uniqueId,
                itemId.Value,
                itemName,
                spriteName,
                spawnPosition
            );

            if (itemObject != null)
            {
                activeDroppedItems.Add(uniqueId, itemObject);
            }
        }
    }

    public void HandleItemPickedUp(JObject data)
    {
        if (data == null) return;

        // 1. Trích xuất dữ liệu từ JObject
        string uniqueId = data["uniqueId"]?.Value<string>();
        int? pickerId = data["pickerId"]?.Value<int>();

        // Kiểm tra dữ liệu hợp lệ
        if (string.IsNullOrEmpty(uniqueId) || !pickerId.HasValue)
        {
            Debug.LogWarning("Received ITEM_PICKED_UP message with missing data.");
            return;
        }

        // 2. Tìm GameObject của item trên mặt đất
        if (activeDroppedItems.TryGetValue(uniqueId, out GameObject itemObject))
        {
            // 3. Tìm GameObject của người nhặt thông qua EntityManager
           GameManager.instance.ActivePlayers.TryGetValue(pickerId.Value, out GameObject gameObject);
            var pickerTransform = gameObject.transform;

            if (pickerTransform != null)
            {
                Debug.Log($"Player {pickerId.Value} picked up item {uniqueId}. Starting move effect.");

                // 4. Lấy component ItemMovement và kích hoạt hiệu ứng bay
                ItemMovement itemMovement = itemObject.GetComponent<ItemMovement>();
                AudioManager.Instance.PlaySFX("Pickup");
                if (itemMovement != null)
                {
                    itemMovement.StartMoveToTarget(pickerTransform);
                }
                else
                {
                    // Fallback: nếu không có script movement, xóa luôn để tránh lỗi
                    Debug.LogWarning($"Item {uniqueId} is missing ItemMovement script. Despawning immediately.");
                    ItemPoolManager.Instance.ReturnItem(itemObject);
                }
            }
            else
            {
                // Nếu không tìm thấy người nhặt (ví dụ người đó khuất màn hình, vừa đổi map...)
                // thì xóa item ngay lập tức để tránh nó lơ lửng mãi mãi.
                Debug.LogWarning($"Picker with ID {pickerId.Value} not found for item {uniqueId}. Despawning immediately.");
                ItemPoolManager.Instance.ReturnItem(itemObject);
            }

            // 5. Xóa item khỏi danh sách quản lý ngay lập tức
            // Vì nó không còn là một "item có thể nhặt được" nữa.
            activeDroppedItems.Remove(uniqueId);
        }
        else
        {
            // Có thể xảy ra nếu gói tin đến trễ, không sao cả.
            Debug.Log($"Received pickup for item {uniqueId} which is no longer active on this client.");
        }
    }
    // Hàm xử lý khi item bị xóa/nhặt không thay đổi
    public void HandleItemDespawn(JObject data)
    {
        if (data == null) return;

        // 1. Lấy JToken của mảng "uniqueIds"
        JToken idsToken = data["uniqueIds"];

        // Kiểm tra xem token có tồn tại và có phải là một mảng không
        if (idsToken == null || idsToken.Type != JTokenType.Array)
        {
     
            return;
        }

        // 2. Lặp qua danh sách các JToken trong mảng
        foreach (JToken idToken in idsToken.AsJEnumerable())
        {
            // 3. Chuyển đổi trực tiếp JToken thành string
            string uniqueId = idToken.Value<string>();

            if (string.IsNullOrEmpty(uniqueId)) continue;

            // 4. Tìm và xóa item tương ứng
            if (activeDroppedItems.TryGetValue(uniqueId, out GameObject itemObject))
            {
               

                // Trả item về pool
                ItemPoolManager.Instance.ReturnItem(itemObject);

                // Xóa khỏi danh sách quản lý
                activeDroppedItems.Remove(uniqueId);
            }
        }
    }


    // Hàm yêu cầu nhặt item không thay đổi
    public void RequestPickupItem(string uniqueId)
    {
        // Gửi yêu cầu lên server
        // YourNetworkLayer.SendMessage(...);
    }
}