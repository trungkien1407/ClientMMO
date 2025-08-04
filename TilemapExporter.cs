//using UnityEngine;
//using UnityEngine.Tilemaps;
//using System.Collections.Generic;
//using System.IO;
//using Newtonsoft.Json;

//public class TilemapExporter : MonoBehaviour
//{
//    [Header("Tilemap layers to export")]
//    public Tilemap[] tilemaps;


//    [Header("Output path (relative to project)")]
//    public string outputPath = "Assets/ExportedMaps/map1.json";

//    [Header("Transition points")]
//    public List<GameObject> transitionPoints = new List<GameObject>();  // Các điểm chuyển map
//    //public List<GameObject> 
//    [ContextMenu("Export Map to JSON")]
//    public void Export()
//    {
//        MapData mapData = new MapData();

//        // Xuất các layer tilemap
//        foreach (Tilemap tilemap in tilemaps)
//        {
//            LayerData layer = new LayerData
//            {
//                name = tilemap.name
//            };

//            BoundsInt bounds = tilemap.cellBounds;
//            foreach (var pos in bounds.allPositionsWithin)
//            {
//                TileBase tile = tilemap.GetTile(pos);
//                if (tile != null)
//                {
//                    // Đặt đường dẫn cho các tile theo tên layer
//                    string tilePath = GetTilePath(tilemap.name, tile.name);

//                    layer.tiles.Add(new TileData
//                    {
//                        x = pos.x,
//                        y = pos.y,
//                        tileName = tile.name,
//                        tilePath = tilePath  // Đường dẫn cho tile
//                    });
//                }
//            }

//            mapData.layers.Add(layer);
//        }

//        // Xuất các điểm chuyển map
//        //mapData.transitionPoints = transitionPoints;

//        // Serialize MapData to JSON using Unity's JsonUtility (for better Unity integration)
//        string json = JsonConvert.SerializeObject(mapData);
//        File.WriteAllText(outputPath, json);
//        Debug.Log($"Map exported to: {outputPath}");
//    }

//    // Phương thức xác định đường dẫn tile dựa trên tên tilemap
//    private string GetTilePath(string tilemapName, string tileName)
//    {
//        if (tilemapName == "Background" || tilemapName == "Midground") // Nếu là tilemap nền
//        {
//            return $"Background/{tileName}";
//        }
//        else // Nếu là tilemap khác
//        {
//            return $"Tile/{tileName}";
//        }
//    }

//}


using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.IO;


public class MapExporter : MonoBehaviour
{
    public string mapName = "MyMap";

    // Danh sách Tilemaps là các layer trên scene, assign trong inspector
    public List<Tilemap> tilemapLayers;

    // Danh sách các map object (collider, effect...) bạn có thể lấy theo tag hoặc group khác
    public List<GameObject> mapObjects;
 //   [Header("Output path (relative to project)")]
    public string exportPath = "Assets/ExportedMaps/";
    [ContextMenu("Export Map to JSON")]
    public void ExportMap()
    {
        MapData mapData = new MapData();
        mapData.mapName = mapName;

        // Giả sử lấy size map từ tilemap lớn nhất
        int maxWidth = 0;
        int maxHeight = 0;

        mapData.layers = new List<TilemapLayerData>();

        foreach (Tilemap tilemap in tilemapLayers)
        {
            TilemapLayerData layerData = new TilemapLayerData();
            layerData.layerName = tilemap.gameObject.name;
            layerData.tiles = new List<TileData>();

            BoundsInt bounds = tilemap.cellBounds;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    TileBase tile = tilemap.GetTile(pos);
                    if (tile != null)
                    {
                        TileData tileData = new TileData();
                        tileData.x = x;
                        tileData.y = y;
                        tileData.tileID = tile.name;  // Cần chắc tile.name là chuỗi số


                        layerData.tiles.Add(tileData);

                        maxWidth = Mathf.Max(maxWidth, x);
                        maxHeight = Mathf.Max(maxHeight, y);
                    }
                }
            }

            mapData.layers.Add(layerData);
        }

        mapData.mapWidth = maxWidth + 1;
        mapData.mapHeight = maxHeight + 1;

        // Export các object map (collider, effect, warp point...)
        mapData.mapObjects = new List<MapObjectData>();

        foreach (var obj in mapObjects)
        {
            string prefabName = obj.name;

            // Nếu là bản clone kiểu "Water (1)", "Water (2)", ta chuẩn hóa tên về gốc
            if (prefabName.Contains("("))
            {
                prefabName = prefabName.Split('(')[0].Trim();
            }
            MapObjectData objData = new MapObjectData();
            objData.prefabName = prefabName;

            objData.x = obj.transform.position.x;
            objData.y = obj.transform.position.y;
            objData.rotation = obj.transform.eulerAngles.z;
            objData.scaleX = obj.transform.localScale.x;
            objData.scaleY = obj.transform.localScale.y;
            objData.tag = obj.tag;

            mapData.mapObjects.Add(objData);
        }

        // Serialize MapData thành JSON
        string json = JsonConvert.SerializeObject(mapData, Formatting.Indented);

        // Lưu ra file
        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }

        string filePath = Path.Combine(exportPath, mapName + ".json");
        File.WriteAllText(filePath, json);

        Debug.Log($"Map exported to {filePath}");
    }
}

