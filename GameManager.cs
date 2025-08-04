using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections;
using UnityEngine.U2D;
using Assets.Script;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using System.Threading.Tasks;
using System;
using UnityEngine.InputSystem.XR;
using TMPro;
using UnityEngine.SocialPlatforms;

public class GameManager : MonoBehaviour
{
    private PlayerPool playerPool;
    private Dictionary<int, GameObject> activePlayers = new Dictionary<int, GameObject>();
    public GameObject malePlayerPrefab; // Prefab cho nhân vật nam
    public GameObject femalePlayerPrefab; // Prefab cho nhân vật nữ
    private GameObject myPlayer;
    private CharacterData myCharacterData;
    private MonsterManager monsterManager;
    public GameObject playerHUD;
    public TMP_Text leveltext;
    public TMP_Text expText;
    public TargetInfor targetInfor; 



    public SpriteAtlas spriteAtlas; // Giả định dùng SpriteAtlas để tải sprite
    public static GameManager instance;
 //   private bool isInitialized = false;
    private int currentMapId = 1; // Map ID mặc định
    public string nextSpawnType = "Start";
  
    public Dictionary<int, GameObject> ActivePlayers => activePlayers;
  //  public bool IsInitialized => isInitialized;
    public int CurrentMapId => currentMapId;
    private ClientManager clientManager;
    private TilemapLoader tilemapLoader;
    private Dictionary<string, Sprite> spriteLookup;
    public SpriteAtlas itemSprites;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            playerPool = new PlayerPool(malePlayerPrefab, femalePlayerPrefab);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        spriteLookup = new Dictionary<string, Sprite>();
        Sprite[] sprites = new Sprite[spriteAtlas.spriteCount];
        spriteAtlas.GetSprites(sprites);
        foreach (var sprite in sprites)
        {
            if (sprite != null && !spriteLookup.ContainsKey(sprite.name))
                spriteLookup[sprite.name] = sprite;
        }
    }
    private void Start()
    {
        clientManager = ClientManager.Instance;
        tilemapLoader = TilemapLoader.Instance;
        clientManager.Dispatcher.RegisterHandler("Characters_in_map", CharacterInMap);
        clientManager.Dispatcher.RegisterHandler("Joinmap", CharacterJoinMap);
        clientManager.Dispatcher.RegisterHandler("Leavermap", OnCharacterLeaveMap);
       clientManager.Dispatcher.RegisterHandler("player_death", PlayerDeath);
        clientManager.Dispatcher.RegisterHandler("player_respawn", OnPlayerRespawn);
        clientManager.Dispatcher.RegisterHandler("level_up", LevelUp);
        clientManager.Dispatcher.RegisterHandler("percent", Percent);
        clientManager.Dispatcher.RegisterHandler("update_sprite", UpdateSprite);
        clientManager.Dispatcher.RegisterHandler("update_character_stats", UpdateCharacterStats);



    }
    public void Init(CharacterBase playerLocal)
    {
        currentMapId = playerLocal.MapID;
       
        OnCharacterReceived(playerLocal, true);
    

    }

    private GameObject GetPlayer(bool isMale)
        {
            return playerPool.GetPlayer(isMale);
        }


    private List<Sprite> GetSpritesFromAtlas(string prefix)
    {
        return spriteLookup
            .Where(kvp => kvp.Key.StartsWith(prefix))
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => kvp.Value)
            .ToList();
    }



    public void OnCharacterReceived(CharacterBase character, bool isLocalPlayer)
    {
        if (character == null || character.CharacterID == 0) return;
        Debug.Log($"Received character: {character.CharacterID}, isLocal: {isLocalPlayer}");
        if (isLocalPlayer)
        {

            Debug.Log(">>> Calling Loading.Instance.StartLoading()");
            Loading.Instance.StartLoading();
            TilemapLoader.Instance.LoadMap(character.MapID);
        }
        if (activePlayers.TryGetValue(character.CharacterID, out var playerObj))
        {
            Debug.Log($"Character {character.CharacterID} already exists. Updating only.");
            var identity = playerObj.GetComponent<CharacterData>();
            identity?.UpdateFromCharacterBase(character);
            StartCoroutine(LoadAndAssignModularSprites(character));
            return;
        }
       
        Debug.Log($"OnCharacterReceived - CharacterID: {character.CharacterID}, IsLocal: {isLocalPlayer}");
       
        GameObject playerObject = GetPlayer(character.Gender == 0);
        var health = playerObject.GetComponent<Health>() ?? playerObject.AddComponent<Health>();
        health.MaxHealth = character.Health;
        health.SetId(character.CharacterID);
        health.SetName(character.Name);
        health.CurrentHealth = character.CurrentHealth;
        health.CurrentMana = character.CurrentMana;
        health.MaxMana = character.Mana;
        if (playerObject == null)
        {
            Debug.LogError("PlayerObject from pool is null!");
            return;
        }
        playerObject.transform.position = new Vector3(character.X, character.Y, 0);
        activePlayers[character.CharacterID] = playerObject;

        // Thêm component tương ứng
        BasePlayerController controller;
        
     
        if (isLocalPlayer)
        {
            var hud = playerHUD.GetComponent<PlayerHUD>();
            hud.SetTarget(health);
            health.playerHUD = hud;
            controller = playerObject.GetComponent<PlayerController>() ?? playerObject.AddComponent<PlayerController>();
            
        }
        else
        {
            controller = playerObject.GetComponent<RemoteController>() ?? playerObject.AddComponent<RemoteController>();
            GameObject healthBarGO = HealthBarPool.Instance.GetHealthBar();
            healthBarGO.transform.SetParent(playerObject.transform);
            var healthBar = healthBarGO.GetComponent<HealthBar>();
            healthBar.offset = new Vector3(0, 1f, 0);
            healthBar.Name.text = character.Name;
            health.healthBar = healthBar;
            healthBar.SetupForCreature(playerObject.transform, health);
            Debug.Log($"Remote Health {health.CurrentHealth}");
           
        }
        controller.Init();


        CharacterData identityData = playerObject.GetComponent<CharacterData>() ?? playerObject.AddComponent<CharacterData>();
        identityData.UpdateFromCharacterBase(character);
        StartCoroutine(LoadAndAssignModularSprites(character,true));
        if (isLocalPlayer)
        {
            SetupLocalPlayer(playerObject);
            controller.ShowMyPlayer();
        }

    }

 

    private void SetupLocalPlayer(GameObject player)
    {
        myPlayer = player;
        myCharacterData = myPlayer.GetComponent<CharacterData>();
        if(leveltext != null)
        {
            leveltext.text = myCharacterData.Level.ToString();
        }
       
        // Thiết lập camera, UI, hoặc các thành phần khác cho local player
        Camera.main.GetComponent<CameraFollow>().target = player.transform;
        myPlayer.AddComponent<PlayerMovement>();
       var target = myPlayer.AddComponent<TargetingSystem>();
        target.SetTargetInfor(targetInfor);
        myPlayer.tag = "LocalPlayer";
        myPlayer.layer = 8;
        var renderers = myPlayer.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
        {
            sr.sortingLayerName = "PlayerLocal";
        }
        AudioManager.Instance.PlayBGM("Background");

    }

    private IEnumerator LoadAndAssignModularSprites(CharacterBase character, bool isInitialLoad = false)
    {
        string gender = character.Gender == 0 ? "Male" : "Female";
        int headId = character.HeadID;
        int bodyId = character.BodyID;
        int legId = character.PantID;
        int weaponId = character.WeaponID;
        int characterClass = character.Class;

        string[] headStates = { "Idle", "Run","Hit"};
        Dictionary<string, List<Sprite>> headSprites = new();
        foreach (string state in headStates)
        {
            string prefix = $"{gender}_Head{headId}_{state}_";
            if (state == "Hit")
            {
                prefix = "Hit";
            }
         headSprites[state] = GetSpritesFromAtlas(prefix);
        }

        string[] states = { "Idle", "Run", "Attack", "Jump","Spin" };
        Dictionary<string, List<Sprite>> bodySprites = new();
        Dictionary<string, List<Sprite>> legSprites = new();
        Dictionary<string, List<Sprite>> weaponSprites = new();
        Dictionary<string, List<Sprite>> spinSprites = new();
        foreach (string state in states)
        {
            string bodyPrefix = $"{gender}_Body{bodyId}_{state}_";
            string legPrefix = $"{gender}_Leg{legId}_{state}_";
            string weaponPrefix =( weaponId > 0 && characterClass !=0 ) ? $"Weapon{weaponId}" : null;
            string spinPrefix = "JumpSpin_";
            bodySprites[state] = GetSpritesFromAtlas(bodyPrefix);
            legSprites[state] = GetSpritesFromAtlas(legPrefix);
            spinSprites[state]=GetSpritesFromAtlas(spinPrefix);
            if (weaponPrefix != null)
                weaponSprites[state] = GetSpritesFromAtlas(weaponPrefix);
        }


        if (bodySprites.ContainsKey("Attack"))
        {
            // Body giữ nguyên tất cả frame Attack
        }
        else
        {
            // Nếu không có Attack, dùng Idle hoặc Run thay thế
            bodySprites["Attack"] = bodySprites.ContainsKey("Idle") ? bodySprites["Idle"] : new List<Sprite>();
        }

        if (legSprites.ContainsKey("Attack") && legSprites["Attack"].Count > 0)
        {
            // Leg chỉ lấy 1 frame đầu (ví dụ frame 0) của Attack
            legSprites["Attack"] = new List<Sprite> { legSprites["Attack"][0] };
        }
        else
        {
            // Nếu không có Attack, fallback
            legSprites["Attack"] = legSprites.ContainsKey("Idle") ? new List<Sprite> { legSprites["Idle"][0] } : new List<Sprite>();
        }

        if (headSprites.ContainsKey("Run") && headSprites["Run"].Count > 0)
        {
            // Head chỉ lấy 1 frame đầu của Run cho Attack
            headSprites["Attack"] = new List<Sprite> { headSprites["Run"][0] };
        }
        else
        {
            // Nếu không có Run, fallback
            headSprites["Attack"] = headSprites.ContainsKey("Idle") ? new List<Sprite> { headSprites["Idle"][0] } : new List<Sprite>();
        }

        //// Gán sprite Hit cho Body/Leg (dùng frame 2 của Jump như bạn làm)
        //if (bodySprites.ContainsKey("Jump") && bodySprites["Jump"].Count >1)
        //    bodySprites["Hit"] = new List<Sprite> { bodySprites["Jump"][1] };
        //if (legSprites.ContainsKey("Jump") && legSprites["Jump"].Count > 1)
        //    legSprites["Hit"] = new List<Sprite> { legSprites["Jump"][1] };

        // Gán sprite cho controller
        GameObject playerObj = activePlayers[character.CharacterID];
        var controller = playerObj.GetComponent<BasePlayerController>();
        if (controller != null)
        {
            controller.SetSprites(headSprites, bodySprites, legSprites, weaponSprites);
            controller.SetSpinSprites(spinSprites);
        }
        if (isInitialLoad)
        {
            yield return new WaitForSeconds(1f);
            Loading.Instance.EndLoading();
           
        }
        else
        {
            // Nếu chỉ là cập nhật, không cần đợi và không cần tắt màn hình loading
            yield return null;
        }
    }
    public void GetCharInMap(int mapid)
    {
        var message = new BaseMessage()
        {
            Action = "Characters_in_map",
            Data = new JObject
            {
                ["mapid"] = mapid,
            }
        };
        string json = JsonConvert.SerializeObject(message);
        clientManager.Send(json);
    }

    public void OnCharacterHit(JObject data)
    {
        int charId = data["characterId"].Value<int>();
        float damage = data["damage"]?.Value<float>() ?? 0f;
        if (activePlayers.TryGetValue(charId, out var playerObj))
        {
            var controller = playerObj.GetComponent<BasePlayerController>();
            if (controller != null)
            {
              
                controller.TriggerHit();
            }
        }
    }

    public void SendLeavermap()
    {
        if (activePlayers == null || activePlayers.Count == 0)
        {
            Debug.LogWarning("[GameManager] No active players to send Leavermap.");
            return;
        }

        var character = activePlayers.FirstOrDefault().Value?.GetComponent<CharacterData>();
        if (character == null)
        {
            Debug.LogWarning("[GameManager] CharacterData not found for active player.");
            return;
        }

        var data = new BaseMessage
        {
            Action = "Leavermap",
            Data = new JObject
            {
                ["characterId"] = character.CharacterID,
                ["mapid"] = currentMapId
            }
        };

        string json = JsonConvert.SerializeObject(data);
        ClientManager.Instance.Send(json);
        Debug.Log($"[GameManager] Sent Leavermap for character {character.CharacterID} on map {currentMapId}");
    }

    public void SetNextSpawnType(string spawnType)
    {
        nextSpawnType = spawnType;
        Debug.Log($"[GameManager] Set next spawn type: {spawnType}");
    }

    public void ChangeMap(int newMapId, bool isLocalPlayer)
    {

        if (newMapId < 1)
        {
            Debug.LogError($"[GameManager] Invalid map ID: {newMapId}");
            return;
        }

        StartCoroutine(TransitionToMap(newMapId, isLocalPlayer));
    }

    private IEnumerator TransitionToMap(int newMapId, bool isLocalPlayer)
    {
        Loading.Instance.StartLoading();

        // 1. Clear tất cả dữ liệu cũ TRƯỚC
      
        MonsterManager.instance.ClearAllMonsters();

        // 2. Disable player movement
        myCharacterData = myPlayer.GetComponent<CharacterData>();
        var playermove = myPlayer.GetComponent<PlayerMovement>();
        playermove.enabled = false;
        ClearPlayers();
        // 3. Clear map cũ
        tilemapLoader.ClearCurrentMap();

        // 4. Load map mới
        TilemapLoader.Instance.LoadMap(newMapId);
        currentMapId = newMapId;

        // 5. Đợi một chút để đảm bảo map đã load xong
        yield return new WaitForSeconds(0.5f);

        // 6. Đặt vị trí nhân vật và gửi Joinmap
        if (isLocalPlayer)
        {
            var spawnPos = nextSpawnType.ToLower() == "start" ? tilemapLoader.SpawnStart : tilemapLoader.SpawnEnd;
            if (spawnPos.HasValue)
            {
                myCharacterData.transform.position = spawnPos.Value;
                var rb = myCharacterData.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
            myCharacterData.MapID = newMapId;
            myCharacterData.X = spawnPos.Value.x;
            myCharacterData.Y = spawnPos.Value.y;

         //   SendLeavermap();

            var data = new BaseMessage
            {
                Action = "Joinmap",
                Data = new JObject
                {
                    ["characterId"] = myCharacterData.CharacterID,
                    ["mapid"] = newMapId,
                    ["spawnX"] = spawnPos.Value.x,
                    ["spawnY"] = spawnPos.Value.y
                }
            };
            AudioManager.Instance.PlayBGM("Background");

            ClientManager.Instance.Send(JsonConvert.SerializeObject(data));
        }

        // 7. Đợi server response Joinmap
        yield return new WaitForSeconds(0.5f);

        // 8. SAU ĐÓ mới request monsters và characters
        GetCharInMap(newMapId);
        GetMonsterInmap(newMapId);

        // 9. Đợi thêm một chút để đảm bảo data đã được xử lý
        yield return new WaitForSeconds(1.0f);

        // 10. Enable lại player movement
        playermove.enabled = true;
        Loading.Instance.EndLoading();
    }
    public void CharacterInMap( JObject data)
    {
        var myplayerId = myPlayer.GetComponent<CharacterData>();
        JArray charactersArray = data["characters"] as JArray;
        if (charactersArray == null) return;

        List<CharacterBase> others = charactersArray.ToObject<List<CharacterBase>>();

        foreach (var character in others)
        {
            bool isLocal = character.CharacterID == myplayerId.CharacterID;
            if(isLocal == false)
            OnCharacterReceived(character, isLocal);
        }
    }
    public void CharacterJoinMap(JObject data)
    {
        var character = data["character"]?.ToObject<CharacterBase>();
        Debug.Log(data);
        if (data == null) return;
        OnCharacterReceived(character, false);
    }
    public void OnCharacterLeaveMap(JObject data)
    {
        int charId = data["characterid"].Value<int>();
        Debug.Log(charId);
        if (activePlayers.TryGetValue(charId, out var playerObj))
        {
            if (playerObj == myPlayer) return;
            bool isMale = playerObj.GetComponent<CharacterData>().Gender == 0;
            activePlayers.Remove(charId);
            playerPool.ReturnPlayer(playerObj, isMale);
        }
    }

        public void GetMonsterInmap(int mapid)
        {

            var request = new BaseMessage
            {
                Action = "Getmonster_map",
                Data = JObject.FromObject(new
                {
                    mapId = mapid,
                })
            };
          clientManager.Send(JsonConvert.SerializeObject(request));
      
        }
    public void DestroyLocal()
    {
        if(myPlayer != null)
        GameObject.Destroy(myPlayer);
    }

    public void ClearPlayers()
    {
        foreach (var kvp in activePlayers)
        {
            var playerObj = kvp.Value;
            if (playerObj == null) continue;

            var characterData = playerObj.GetComponent<CharacterData>();
            bool isMale = characterData != null && characterData.Gender == 0;
            if(characterData.CharacterID != myCharacterData.CharacterID) { playerPool.ReturnPlayer(playerObj, isMale); }
           
        }

        activePlayers.Clear();

        // Nếu bạn giữ lại myPlayer, thì add lại vào dictionary
        if (myPlayer != null)
        {
            var charId = myPlayer.GetComponent<CharacterData>()?.CharacterID ?? 0;
            if (charId > 0)
                activePlayers[charId] = myPlayer;
        }

        Debug.Log("[GameManager] Cleared all players.");
    }

    public Sprite GetItemIcon(int iconId)
    {
        return itemSprites.GetSprite(iconId.ToString());
    }
    public void OnPlayerRespawn(JObject data)
    {
        var character = data["character"]?.ToObject<CharacterBase>();
        if (character == null)
        {
            Debug.LogError("[GameManager] Invalid respawn data from server");
            return;
        }

        int playerId = character.CharacterID;
        bool isLocalPlayer = playerId == myCharacterData.CharacterID;

        Debug.Log($"[GameManager] Player {playerId} respawned. IsLocal: {isLocalPlayer}");

        if (isLocalPlayer)
        {
            
          
                StartCoroutine(HandleLocalPlayerRespawn(character));
        
        }
        else
        {
            // Remote player respawn
            if (character.MapID == currentMapId)
            {
                RespawnPlayerInCurrentMap(character, false);
            }
            else
            {
                // Nếu remote player respawn ở map khác, remove khỏi current map
                RemovePlayerFromCurrentMap(playerId);
            }
        }
    }

    // 4. Xử lý hồi sinh local player (có thể cần đổi map)
    private IEnumerator HandleLocalPlayerRespawn(CharacterBase character)
    {
        Loading.Instance.StartLoading();

        // Disable player movement
        var playerMovement = myPlayer.GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Nếu cần đổi map
        if (character.MapID != currentMapId)
        {
            // Clear current map data
            MonsterManager.instance?.ClearAllMonsters();
            ClearPlayers();
            tilemapLoader.ClearCurrentMap();

            // Load new map
            TilemapLoader.Instance.LoadMap(character.MapID);
            currentMapId = character.MapID;

            yield return new WaitForSeconds(0.5f);
        }

        // Update player data và vị trí
        RespawnPlayerInCurrentMap(character, true);

        // Get other players và monsters trong map mới
        GetCharInMap(character.MapID);
        Debug.Log($"Map id hiện tại{currentMapId} ");
        GetMonsterInmap(character.MapID);

        yield return new WaitForSeconds(1.0f);

        // Enable lại player movement
        if (playerMovement != null)
            playerMovement.enabled = true;

        Loading.Instance.EndLoading();
    }

    // 5. Hồi sinh player trong map hiện tại
    private void RespawnPlayerInCurrentMap(CharacterBase character, bool isLocalPlayer)
    {
        GameObject playerObj;

        if (activePlayers.TryGetValue(character.CharacterID, out playerObj))
        {
            // Player đã tồn tại, update data
            UpdatePlayerAfterRespawn(playerObj, character, isLocalPlayer);
        }
        else
        {
            // Player chưa tồn tại, tạo mới
            OnCharacterReceived(character, isLocalPlayer);
        }
    }

    // 6. Update player sau khi hồi sinh
    private void UpdatePlayerAfterRespawn(GameObject playerObj, CharacterBase character, bool isLocalPlayer)
    {
        // Update position
        playerObj.transform.position = new Vector3(character.X, character.Y, 0);

        // Reset velocity nếu có Rigidbody2D
        var rb = playerObj.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // Update health và mana
        var health = playerObj.GetComponent<Health>();
        if (health != null)
        {
            health.CurrentHealth = character.CurrentHealth;
            health.MaxHealth = character.Health;
            health.CurrentMana = character.CurrentMana;
            health.MaxMana = character.Mana;
        }

        // Update character data
        var characterData = playerObj.GetComponent<CharacterData>();
        if (characterData != null)
        {
            characterData.UpdateFromCharacterBase(character);
        }

        // Enable player controller nếu bị disable
        var controller = playerObj.GetComponent<BasePlayerController>();
        controller.HideDead();
   
        Debug.Log($"[GameManager] Player {character.CharacterID} respawned at ({character.X}, {character.Y}) with {character.CurrentHealth} HP");
    }

    // 7. Remove player khỏi map hiện tại
    private void RemovePlayerFromCurrentMap(int playerId)
    {
        if (activePlayers.TryGetValue(playerId, out var playerObj))
        {
            bool isMale = playerObj.GetComponent<CharacterData>().Gender == 0;
            activePlayers.Remove(playerId);
            playerPool.ReturnPlayer(playerObj, isMale);
            Debug.Log($"[GameManager] Removed player {playerId} from current map");
        }
    }

    // 8. Update hàm PlayerDeath để thêm button hồi sinh
    public void PlayerDeath(JObject data)
    {
        var items = data["items"] as JArray;
        if (items == null || items.Count == 0)
            return;

        foreach (var item in items)
        {
            var playerID = item["playerId"]?.ToObject<int>() ?? 0;
            var isLocal = playerID == myCharacterData.CharacterID;

            if (activePlayers.TryGetValue(playerID, out GameObject playerObj))
            {
                BasePlayerController controller = playerObj.GetComponent<BasePlayerController>();
                controller.ShowDeadOnly();

                if (isLocal)
                {
                    PopupManager.Instance.ShowPopup("Bạn đã chết, nhấn OK để hồi sinh", () =>
                    {
                        RequestRespawn(); // Gọi hàm hồi sinh
                        Debug.Log($"Map hiện tại {currentMapId}");
                    });
                }
            }
        }
    }

    // 1. Thêm hàm gửi request hồi sinh trong GameManager
    public void RequestRespawn()
    {
        if (myCharacterData == null)
        {
            Debug.LogError("[GameManager] Cannot respawn: myCharacterData is null");
            return;
        }

        var request = new BaseMessage
        {
            Action = "Respawn_player",
            Data = new JObject
            {
                ["characterId"] = myCharacterData.CharacterID
            }
        };

        string json = JsonConvert.SerializeObject(request);
        clientManager.Send(json);
        Debug.Log($"[GameManager] Sent respawn request for character {myCharacterData.CharacterID}");
    }
    public void LevelUp(JObject data)
    {
        
        var character = data["character"]?.ToObject<CharacterBase>();
        if (character == null) { return; }
        var health = myPlayer.GetComponent<Health>();
        health.MaxHealth = character.Health;
        health.MaxMana = character.Mana;
        health.CurrentHealth = character.CurrentHealth;
        health.CurrentMana = character.CurrentMana;
        leveltext.text = null;

        expText.text = "00.00%";
        leveltext.text = character.Level.ToString();
        
    }
    public void Percent(JObject data)
    {
       
        var percent = data["percent"]?.ToObject<float>() ?? 0f;
        expText.text = $"{percent:F2}%";
        
    }

    public void UpdateSprite(JObject data)
    {
        if (data == null) return;
        var bodyid = data["body"]?.ToObject<int>() ?? 1;
        var characterid= data["characterid"]?.ToObject<int>() ?? 0;
        var character = data["character"]?.ToObject<CharacterBase>() ?? null;
        if(character == null) return; 
        var pantid = data["pant"]?.ToObject<int>() ?? 1;
        StartCoroutine(LoadAndAssignModularSprites(character,false));
    }
    public void UpdateCharacterStats(JObject data)
    {
        if (data == null)
        {
            return;
        }

        var characterId = data["characterId"]?.ToObject<int>() ?? 0;

        Debug.Log($"characterid {characterId}");
        var currentHealth = data["currentHealth"]?.ToObject<int>() ?? 0;
        var currentMana = data["currentMana"]?.ToObject<int>() ?? 0;
        if (ActivePlayers.TryGetValue(characterId, out GameObject playerObj))
        {
            if (playerObj == null)
            {
                Debug.Log("Player null");
            }
            Health health;
           health = playerObj.GetComponent<Health>();
            health.CurrentHealth = currentHealth;
            health.CurrentMana = currentMana;
        }

       
    }

}