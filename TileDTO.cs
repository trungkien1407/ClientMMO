using System.Collections.Generic;

[System.Serializable]
public class MapData
{
    public string mapName;
    public int mapWidth;
    public int mapHeight;
    public List<TilemapLayerData> layers;   // Danh sách các layer
    public List<MapObjectData> mapObjects; // Các object như effect, collider, warp point ...
}

[System.Serializable]
public class TilemapLayerData
{
    public string layerName;
    public List<TileData> tiles;
}

[System.Serializable]
public class TileData
{
    public int x;
    public int y;
    public string tileID;  // hoặc có thể là string tileName, tùy cách bạn quản lý tile
}

[System.Serializable]
public class MapObjectData
{
    public string prefabName;
    public float x;
    public float y;
    public float rotation;
    public float scaleX;
    public float scaleY;
    public string tag;
}
