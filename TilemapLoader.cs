using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;
public class TilemapLoader : MonoBehaviour
{
    public Grid Grid { get; private set; }

    public static TilemapLoader Instance { get; private set; }
    public event Action onLoad;

    private Dictionary<string, Queue<GameObject>> objectPools = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void ClearCurrentMap()
    {
        var mapGO = GameObject.Find("Map");
        if (mapGO == null) return;

        foreach (Transform child in mapGO.transform)
        {
            string prefabName = child.name;
            if (prefabName.Contains("("))
            {
                prefabName = prefabName.Split('(')[0].Trim();
            }
            GameObject.Destroy(child.gameObject);

            //if (child.CompareTag("map")) // Chỉ pool các hiệu ứng
            //{
               
            //}
            //else // Tilemap và các đối tượng khác thì hủy luôn
            //{

            //    ReturnToPool(prefabName, child.gameObject)
            //   ;
            //}
        }
    }




  //  [ContextMenu("Load Map From JSON")]
    public void LoadMap(int mapid)
    {
        string resourcePath = $"Map/Map{mapid}";
        TextAsset jsonText = Resources.Load<TextAsset>(resourcePath);

        if (jsonText == null)
        {
            Debug.LogError($"Không tìm thấy file JSON tại Resources/{resourcePath}");
            return;
        }

        string json = jsonText.text;
        MapData mapData = JsonConvert.DeserializeObject<MapData>(json);

        // Tạo hoặc tìm Grid cha
        GameObject gridGO = GameObject.Find("Map");
        if (gridGO == null)
            gridGO = new GameObject("Map", typeof(Grid));

        Grid = gridGO.GetComponent<Grid>();


        // Load các layer tilemap
        foreach (var layer in mapData.layers)
        {

            GameObject layerGO = new GameObject(layer.layerName);
            layerGO.tag = "map";
            layerGO.transform.SetParent(gridGO.transform);

            var tilemap = layerGO.AddComponent<Tilemap>();
            var renderer = layerGO.AddComponent<TilemapRenderer>();
            renderer.sortingLayerName = layer.layerName;

            string lowerLayer = layer.layerName.ToLower();

            // Gán layer Unity (có thể cấu hình trước trong Unity)
            tilemap.gameObject.layer = LayerMask.NameToLayer("Ground");

            // Nếu là ground hoặc midground thì thêm collider
            if (lowerLayer == "ground" || lowerLayer == "midground")
            {
                // Composite Collider cần cấu hình đúng
                var tilemapCollider = layerGO.AddComponent<TilemapCollider2D>();
                tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
              

                var rb = layerGO.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;
                rb.simulated = true;

                var composite = layerGO.AddComponent<CompositeCollider2D>();
                composite.usedByEffector = true;
                var effector = layerGO.AddComponent<PlatformEffector2D>();
                effector.useOneWay = true;
                effector.useOneWayGrouping = true;
                effector.surfaceArc = 180;
                composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
                composite.generationType = CompositeCollider2D.GenerationType.Synchronous;

                // Midground sẽ có PlatformEffector (bước nhảy một chiều)
                if (lowerLayer == "midground")
                {
                 
          
                    effector.surfaceArc = 130;

                    // Chắc chắn Collider có tick "Used by Effector"
                }
            }

            foreach (var tile in layer.tiles)
            {
                string folder = layer.layerName.ToLower() == "background" ? "background" : "Tile";
                // Tải tile từ Resources/Tiles/{tileID}
                TileBase tileAsset = Resources.Load<TileBase>($"{folder}/{tile.tileID}");
                if (tileAsset != null)
                {
                    Vector3Int pos = new Vector3Int(tile.x, tile.y, 0);
                    tilemap.SetTile(pos, tileAsset);
                }
                else
                {
                    Debug.LogWarning($"Missing tile: {tile.tileID}");
                }
            }
        }
        

        // Load các object khác trong map
        LoadMapObjects(mapData.mapObjects);

        onLoad?.Invoke();
        Debug.Log("Map loaded successfully.");
    }
    private Vector3? spawnStart = null;
    private Vector3? spawnEnd = null;

    public Vector3? SpawnStart => spawnStart;
    public Vector3? SpawnEnd => spawnEnd;


    private void LoadMapObjects(List<MapObjectData> mapObjects)
    {
        Transform parentTransform = GameObject.Find("Map").transform;

        foreach (var objData in mapObjects)
        {
            GameObject prefab = Resources.Load<GameObject>($"MapObjects/{objData.prefabName}");
            GameObject obj;

            if (prefab != null)
            {
                obj = GetFromPool(objData.prefabName, prefab);
                obj.transform.SetParent(parentTransform);
            }
            else
            {
                obj = new GameObject(objData.prefabName);
                obj.transform.SetParent(parentTransform);
            }

            obj.transform.localPosition = new Vector3(objData.x, objData.y, 0);
            obj.transform.localRotation = Quaternion.Euler(0, 0, objData.rotation);
            obj.transform.localScale = new Vector3(objData.scaleX, objData.scaleY, 1);
            obj.tag = objData.tag;
            obj.name = objData.prefabName;

            if (obj.name.ToLower() == "spawnpointstart")
                spawnStart = obj.transform.position;
            else if (obj.name.ToLower() == "spawnpointend")
                spawnEnd = obj.transform.position;
            AddCustomController(obj, objData.prefabName);
        }
    }


    private void AddCustomController(GameObject obj, string prefabName)
    {
        switch (prefabName.ToLower())
        {
            case "water":
            case "waterfall":
                if (obj.GetComponent<WaterfallAnimator>() == null)
                    obj.AddComponent<WaterfallAnimator>();
                break;

            case "arrow":
                if (obj.GetComponent<ArrowControl>() == null)
                    obj.AddComponent<ArrowControl>();
                break;

            case "previousmap":
            case "nextmap":
                {
                    var bc = obj.GetComponent<BoxCollider2D>();
                    if (bc == null)
                        bc = obj.AddComponent<BoxCollider2D>();

                    bc.size = new Vector2(0.1f, 1f);
                    bc.isTrigger = true;

                    var trigger = obj.AddComponent<MapTransitionTrigger>();
                    trigger.transitionType = prefabName.ToLower() == "nextmap"
                        ? MapTransitionTrigger.TransitionType.Next
                        : MapTransitionTrigger.TransitionType.Previous;


                    break;
                                
                }

            case "barrier":
                {
                    var bc = obj.GetComponent<BoxCollider2D>();
                    if (bc == null)
                        bc = obj.AddComponent<BoxCollider2D>();

                    bc.size = new Vector2(0.1f, 10f);
                    bc.isTrigger = false;
                    break;
                }

            default:
                break;
        }
    }


    private GameObject GetFromPool(string prefabName, GameObject prefab)
    {
        if (!objectPools.ContainsKey(prefabName))
            objectPools[prefabName] = new Queue<GameObject>();

        var queue = objectPools[prefabName];

        if (queue.Count > 0)
        {
            var go = queue.Dequeue();
            go.SetActive(true);
            return go;
        }

        return Instantiate(prefab);
    }

}



