using UnityEngine;

public class TransitionPointScript : MonoBehaviour
{
    [Tooltip("Tên map đích sẽ chuyển đến")]
    public string targetMap;

    private void OnTriggerEnter2D(Collider2D other)
    {
   
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Chuyển đến bản đồ: {targetMap}");

            // Gọi TilemapLoader nếu bạn đã có Singleton
            if (TilemapLoader.Instance != null)
            {
                int mapId = MapNameToId(targetMap);
                TilemapLoader.Instance.LoadMap(mapId);

                // (Tùy chọn) dịch chuyển player đến vị trí spawn
                // other.transform.position = Vector3.zero;
            }
        }
    }

    private int MapNameToId(string mapName)
    {
        // Chuyển từ tên sang ID nếu bạn lưu map dạng "Map1", "Map2"
        if (mapName.StartsWith("Map"))
        {
            if (int.TryParse(mapName.Substring(3), out int id))
                return id;
        }
        Debug.LogError($"Tên map không hợp lệ: {mapName}");
        return 1;
    }
}
