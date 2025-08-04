using System;



// Lớp này đại diện cho một item duy nhất trong gói tin
[Serializable]
public class GroundItemDTO
{
    public string uniqueId; // Server gửi Guid dưới dạng string
    public int itemId;
    public float x;
    public float y;
    public int ownerId;
}

// Lớp này đại diện cho toàn bộ phần "Data" của gói tin
[Serializable]
public class ItemSpawnedData
{
    public GroundItemDTO[] items; // Một mảng các item rơi ra
}
