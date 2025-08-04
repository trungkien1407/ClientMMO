using Assets.Script;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.U2D;



public class MonsterManager : MonoBehaviour
{
    public GameObject monsterPrefab;
    public SpriteAtlas monsterSprite;
    public static MonsterManager instance;
    public Health health;

    private ClientManager clientmanager;
    private MonsterPool pool;
    private Dictionary<int, GameObject> Monsters = new();
    public Dictionary<int, GameObject> monsters => Monsters;
    private Dictionary<string, Dictionary<string, List<Sprite>>> spriteCache = new();
    private GameManager gamemanager;

    // THÊM: Track current map để tránh xung đột
    private int currentMapId = -1;

    // THÊM: Pending moves để xử lý moves đến trước khi monster được tạo
   // private List<PendingMonsterMove> _pendingMoves = new List<PendingMonsterMove>();

    private void Awake()
    {
        if (instance != null) Destroy(gameObject);
        instance = this;
    }

    private void Start()
    {
        clientmanager = ClientManager.Instance;
        pool = new MonsterPool(monsterPrefab);
        clientmanager.Dispatcher.RegisterHandler("MonsterList", MonsterListHandle);
        clientmanager.Dispatcher.RegisterHandler("monster_move", MonsterMoveHandle);
        clientmanager.Dispatcher.RegisterHandler("monster_take_damage", MonsterTakeDame);
        clientmanager.Dispatcher.RegisterHandler("monster_attack", MonsterAttack);
        clientmanager.Dispatcher.RegisterHandler("monster_death", MonsterDeath);
        clientmanager.Dispatcher.RegisterHandler("monster_respawn", MonsterRespawn);

    }

    private void OnDestroy()
    {
        pool.ClearPool();
    }

    public void MonsterListHandle(JObject data)
    {
        // THÊM: Lấy mapId từ data
        int mapId = data["mapId"]?.Value<int>() ?? -1;

        JArray monstersToken = data["monsters"] as JArray;
        if (monstersToken == null)
        {
            Debug.LogError("No 'monsters' field in MonsterList data.");
            return;
        }

        var monsterList = monstersToken.ToObject<List<MonsterData>>()
                                .FindAll(m => m.IsAlive);

      

        // QUAN TRỌNG: Nếu là map mới, clear toàn bộ monsters cũ
        if (currentMapId != mapId && currentMapId != -1)
        {
           
            ClearAllMonsters();
        }

        currentMapId = mapId;

        // Tạo HashSet để track monsters hiện tại
        var currentSpawnIds = new HashSet<int>();

        foreach (var monsterData in monsterList)
        {
            currentSpawnIds.Add(monsterData.SpawnID);

            GameObject monsterGO;
            Health health;

            if (Monsters.ContainsKey(monsterData.SpawnID))
            {
                // Monster đã tồn tại -> chỉ cập nhật trạng thái
                monsterGO = Monsters[monsterData.SpawnID];

                // CẬP NHẬT VỊ TRÍ: Quan trọng là set position trực tiếp
                monsterGO.transform.position = new Vector3(monsterData.X, monsterData.Y, 0);
                var healthBar = monsterGO.GetComponent<HealthBar>();

                var movement = monsterGO.GetComponent<MonsterMovement>();
                if (movement != null)
                {
                    movement.ResetToPosition(new Vector2(monsterData.X, monsterData.Y));
                }
                health = monsterGO.GetComponent<Health>();
                health.MaxHealth = monsterData.MaxHP;
                health.CurrentHealth = monsterData.CurrentHP;

                SetupHealthBar(healthBar, monsterGO.transform, health, monsterData.MonsterImg);

            }
            else
            {
                // Tạo monster mới
                monsterGO = pool.GetMonster();

                // QUAN TRỌNG: Reset hoàn toàn transform trước khi set position
                monsterGO.transform.position = Vector3.zero;
                monsterGO.transform.localPosition = Vector3.zero;
                monsterGO.transform.rotation = Quaternion.identity;
                monsterGO.transform.localScale = Vector3.one;

                // Set vị trí mới
                monsterGO.transform.position = new Vector3(monsterData.X, monsterData.Y, 0);
                var movement = monsterGO.GetComponent<MonsterMovement>();
                if (movement != null)
                {
                    movement.ResetToPosition(new Vector2(monsterData.X, monsterData.Y));
                }
             

                health = monsterGO.GetComponent<Health>();
                health.SetId(monsterData.SpawnID);
                health.SetName(monsterData.MonsterName);


                health.MaxHealth = monsterData.MaxHP;
                health.CurrentHealth = monsterData.CurrentHP;

                // Reset movement component
                //var movement = monsterGO.GetComponent<MonsterMovement>();
                //if (movement != null)
                //{
                //    movement.ResetToPosition(new Vector2(monsterData.X, monsterData.Y));
                //}

                Monsters[monsterData.SpawnID] = monsterGO;

                // Load sprites
                var visual = monsterGO.GetComponent<MonsterVisual>();
                if (visual != null)
                {
                    var spriteDict = LoadSpritesForMonster(monsterData.MonsterImg);
                    visual.SetSprites(spriteDict);
                }

                // Setup HealthBar
                GameObject healthBarGO = HealthBarPool.Instance.GetHealthBar();
                healthBarGO.transform.SetParent(monsterGO.transform);
                var healthBar = healthBarGO.GetComponent<HealthBar>();
                SetupHealthBar(healthBar, monsterGO.transform, health, monsterData.MonsterImg);

             

              
            }
        }

  
    }
    private void SetupHealthBar(HealthBar healthBar, Transform targetTransform, Health health, string monsterImg)
    {
        switch (monsterImg)
        {
            case "bunhin":
            case "quy1mat":
                {
                    healthBar.offset = new Vector3(0, 1f, 0);
                    healthBar.SetupForCreature(targetTransform, health);
                    health.healthBar = healthBar;
                    break;
                }
            default:
                {
                    healthBar.offset = new Vector3(0, 0.5f, 0);
                    healthBar.SetupForCreature(targetTransform, health);
                    health.healthBar = healthBar;
                    break;
                }
        }

      
    }


    public void RemoveMonster(int spawnId)
    {
        if (Monsters.TryGetValue(spawnId, out GameObject monsterGO))
        {
         

           // Trả HealthBar về pool
            var healthBar = monsterGO.GetComponentInChildren<HealthBar>();
            if (healthBar != null)
            {
                healthBar.ResetState();
                HealthBarPool.Instance.ReturnHealthBar(healthBar.gameObject);
            }
            var move = monsterGO.GetComponent<MonsterMovement>();
            move.ResetToPosition(Vector2.zero);
            // Trả monster về pool
            pool.ReturnMonster(monsterGO);
            Monsters.Remove(spawnId);
        }
    }

    private Dictionary<string, List<Sprite>> LoadSpritesForMonster(string monsterImg)
    {
        if (spriteCache.TryGetValue(monsterImg, out var cached))
            return cached;

        var result = new Dictionary<string, List<Sprite>>();
        Sprite[] allSprites = new Sprite[monsterSprite.spriteCount];
        monsterSprite.GetSprites(allSprites);

        string prefix = monsterImg + "_";

        foreach (var sprite in allSprites)
        {
            if (sprite == null || !sprite.name.StartsWith(prefix)) continue;

            string rawSuffix = sprite.name.Substring(prefix.Length);
            int parenIndex = rawSuffix.IndexOf('(');
            string suffix = parenIndex >= 0 ? rawSuffix.Substring(0, parenIndex) : rawSuffix;

            if (int.TryParse(suffix, out int frameIndex))
            {
                switch (frameIndex)
                {
                    case 0:
                    case 1:
                        AddSprite(result, "Idle", sprite);
                        break;
                    case 2:
                        AddSprite(result, "Hit", sprite);
                        break;
                    case 3:
                        AddSprite(result, "Attack", sprite);
                        break;
                    case 4:
                        AddSprite(result, "Move", sprite);
                        break;
                }
            }
        }

        if (!result.ContainsKey("Move") && result.TryGetValue("Idle", out var idleList) && idleList.Count > 1)
        {
            result["Move"] = new List<Sprite>(idleList);
        }

        spriteCache[monsterImg] = result;
        return result;
    }

    public class PendingMonsterMove
    {
        public int SpawnId;
        public Vector2 OldPos;
        public Vector2 NewPos;
        public long Timestamp;
    }

    public void MonsterMoveHandle(JObject data)
    {
        var items = data["items"] as JArray;
        if (items == null || items.Count == 0)
            return;

        long timestamp = data["timestamp"]?.Value<long>() ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        foreach (var item in items)
        {
            int spawnId = item["spawnId"]?.Value<int>() ?? -1;
            float x = item["x"]?.Value<float>() ?? 0f;
            float y = item["y"]?.Value<float>() ?? 0f;
            float oldX = item["oldX"]?.Value<float>() ?? x;
            float oldY = item["oldY"]?.Value<float>() ?? y;
            int monsterId = item["MonsterId"]?.Value<int>() ?? -1;

            // Bỏ qua nếu không có thay đổi vị trí
            if (Mathf.Approximately(oldX, x) && Mathf.Approximately(oldY, y))
                continue;

            // Bỏ qua monsters không di chuyển (bù nhìn = monsterId 1)
            if (monsterId == 1)
            {
              //  Debug.Log($"[MonsterMove] Bỏ qua movement cho bù nhìn monsterId={monsterId}, spawnId={spawnId}");
                continue;
            }

            if (!Monsters.TryGetValue(spawnId, out GameObject monsterGO))
            {
                //Debug.LogWarning($"Không tìm thấy monster với SpawnID {spawnId}, thêm vào pending moves.");

                //// Xóa pending move cũ của cùng spawnId để tránh duplicate
                //_pendingMoves.RemoveAll(p => p.SpawnId == spawnId);

                //_pendingMoves.Add(new PendingMonsterMove
                //{
                //    SpawnId = spawnId,
                //    OldPos = new Vector2(oldX, oldY),
                //    NewPos = new Vector2(x, y),
                //    Timestamp = timestamp
                //});
                break; 
            }

            var movement = monsterGO.GetComponent<MonsterMovement>();
            
            if (movement != null)
            {
                movement.SetTarget(new Vector2(oldX, oldY), new Vector2(x, y), timestamp);
             //   Debug.Log($"[Movement] Monster SpawnID={spawnId} từ ({oldX:F1}, {oldY:F1}) đến ({x:F1}, {y:F1})");
            }
        }
    }

     public void MonsterTakeDame( JObject data)
     {
        
        var spawnid = data["spawnId"]?.ToObject<int>() ?? 0;
        var damage = data["damage"]?.ToObject<int>() ?? 0;
        var currentHP = data["currentHp"]?.ToObject<int>() ?? 0;
        if (Monsters.TryGetValue(spawnid, out GameObject monsterGO))
        {
         var health =   monsterGO.GetComponent<Health>();
            health.CurrentHealth = currentHP;
        }

    }
    private void AddSprite(Dictionary<string, List<Sprite>> dict, string key, Sprite sprite)
    {
        if (!dict.ContainsKey(key))
            dict[key] = new List<Sprite>();
        dict[key].Add(sprite);
    }

    // THAY ĐỔI: Đổi tên method để rõ ràng hơn
    public void ClearAllMonsters()
    {
       

        foreach (var kvp in Monsters)
        {
            var monster = kvp.Value;

            // Trả HealthBar về pool
            var healthBar = monster.GetComponentInChildren<HealthBar>();
            if (healthBar != null)
            {
                healthBar.ResetState();
                HealthBarPool.Instance.ReturnHealthBar(healthBar.gameObject);
            }
            // Trả monster về pool
            pool.ReturnMonster(monster);
        }

        Monsters.Clear();
      //  _pendingMoves.Clear(); // Clear pending moves cũ

     
    }

    public void MonsterAttack(JObject data)
    {
        var players = GameManager.instance.ActivePlayers;

        var items = data["items"] as JArray;
        if (items == null || items.Count == 0)
            return;

        foreach (var item in items)
        {
            var spawnID = item["spawnId"]?.ToObject<int>() ?? 0;
            var damage = item["damage"]?.ToObject<int>() ?? 0;
            var targetID = item["targetPlayerId"]?.ToObject<int>() ?? 0;

            // Tìm monster theo spawnID
            if (Monsters.TryGetValue(spawnID, out GameObject monsterGO)
                && players.TryGetValue(targetID, out GameObject playerGO))
            {
                var monster = monsterGO.GetComponent<MonsterVisual>();
                monster?.PlayAttack();

                GameObject projectileObj = ProjectilePool.Instance.GetProjectile();
                projectileObj.transform.position = monster.transform.position;

                var projectile = projectileObj.GetComponent<Projectile>();
                projectile.Setup(playerGO.transform.position, () =>
                {
                  BasePlayerController controller = playerGO.GetComponent<BasePlayerController>();
                    controller.ChangeState("Hit");
                    var health = playerGO.GetComponent<Health>();
                    health.TakeDamage(damage);

                });
            }
        }
    }
    public void MonsterDeath(JObject data)
    {
        var spawnId = data["spawnId"]?.ToObject<int>() ?? 0;
        var facing = data["facing"]?.ToObject<bool>() ?? false;

        if (Monsters.TryGetValue(spawnId, out GameObject monsterGO))
        {
            var visual = monsterGO.GetComponent<MonsterVisual>();
            visual.PlayDeath(facing, spawnId); // Truyền spawnId vào để delay RemoveMonster
        }
    }
    public void MonsterRespawn(JObject data)
    {
        var items = data["items"] as JArray;
        if (items == null || items.Count == 0)
        {
           
            return;
        }

      

        foreach (var item in items)
        {
            var spawnId = item["spawnId"]?.ToObject<int>() ?? 0;
            var x = item["x"]?.ToObject<float>() ?? 0f;
            var y = item["y"]?.ToObject<float>() ?? 0f;
            var currentHp = item["currentHp"]?.ToObject<int>() ?? 100;
            var monsterImg = item["monsterImg"]?.ToString() ?? "default";
            var monsterName = item["monsterName"]?.ToString() ?? "default";



            GameObject monsterGO;

            // Nếu không có thì tạo mới từ pool
            if (!Monsters.TryGetValue(spawnId, out monsterGO))
            {
                monsterGO = pool.GetMonster();
                Monsters[spawnId] = monsterGO;
                monsterGO.transform.position = Vector3.zero;
                monsterGO.transform.localPosition = Vector3.zero;
                monsterGO.transform.rotation = Quaternion.identity;
                monsterGO.transform.localScale = Vector3.one;

                // Load sprite nếu cần
                var visual = monsterGO.GetComponent<MonsterVisual>();
                if (visual != null)
                {
                    var spriteDict = LoadSpritesForMonster(monsterImg);
                    visual.SetSprites(spriteDict);
                }
            }

            monsterGO.SetActive(true);
            monsterGO.transform.position = new Vector3(x, y, 0);

            var movement = monsterGO.GetComponent<MonsterMovement>();
            if (movement != null)
            {
                movement.ResetToPosition(new Vector2(x, y));
            }

            var health = monsterGO.GetComponent<Health>();
            health.SetId(spawnId);
            health.SetName(monsterName);

            health.MaxHealth = currentHp;
            health.CurrentHealth = currentHp;

            var visualComp = monsterGO.GetComponent<MonsterVisual>();
            if (visualComp != null)
            {
                visualComp.SetState("Idle");
               
            }

            GameObject healthBarGO = HealthBarPool.Instance.GetHealthBar();
            healthBarGO.transform.SetParent(monsterGO.transform);
            var healthBar = healthBarGO.GetComponent<HealthBar>();
            healthBar.SetupForCreature(monsterGO.transform, health);

            if (healthBar != null)
            {
                SetupHealthBar(healthBar, monsterGO.transform, health, monsterImg);
            }


        }
    }







}