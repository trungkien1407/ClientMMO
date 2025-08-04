using Assets.Script;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateCharacterManager : MonoBehaviour
{
    public static CreateCharacterManager instance;
    public GameObject characterPrefab; // Prefab của CharacterPreview
    private GameObject characterInstance; // Instance duy nhất của nhân vật
    //public GameObject panel; // Tham chiếu đến panel chứa nhân vật
    int gender = -1; // Giới tính mặc định
    int charclass = -1;
     private int headIndex = 1; // Số thứ tự của đầu
    private int bodyIndex = 1; // Số thứ tự của thân
    private int legsIndex = 1; // Số thứ tự của chân
    public GameObject createCharacterPanel;
    public TMP_InputField nameInput;
    private ClientManager clientManager;
    private PopupManager popupManager;
    public GameObject Background;
    public GameObject HUD;

    //private GameManager gameManager;

    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>(); // Cache sprite

    void Awake()
    {
        // Đảm bảo chỉ có 1 
        if (instance == null)
        {
            instance = this;
           
        }
        else
        {
            Destroy(gameObject);
        }
       
    }
    private void Start()
    {
        clientManager = ClientManager.Instance;
        popupManager = PopupManager.Instance;
        //gameManager = GameManager.Instance;
        clientManager.Dispatcher.RegisterHandler("create_char_result", HandleCreateResult);
    }


    public void ShowCreateCharacterPanel()
    {
        if (createCharacterPanel != null)
        {
            createCharacterPanel.SetActive(true);

            if (characterInstance == null)
            {
                characterInstance = Instantiate(characterPrefab, createCharacterPanel.transform);
                characterInstance.name = "PlayerPreview";
            }

            UpdateCharacterPreview();
        }
    }


    void UpdateCharacterPreview()
    {
        if (gender <0)
        {
            characterInstance.SetActive(false);
            return;
            
        }
        characterInstance.SetActive(true);

        if (characterInstance == null) return;
        string genderStr = gender == 0 ? "nam" : "nu";


        Image headImage = characterInstance.transform.Find("head").GetComponent<Image>();
        Image bodyImage = characterInstance.transform.Find("body").GetComponent<Image>();
        Image legsImage = characterInstance.transform.Find("leg").GetComponent<Image>();

        headImage.sprite = LoadSprite($"{genderStr}/Head/{genderStr}head{headIndex}");
        bodyImage.sprite = LoadSprite($"{genderStr}/Body/{genderStr}body{bodyIndex}");
        legsImage.sprite = LoadSprite($"{genderStr}/Leg/{genderStr}leg{legsIndex}");
        Debug.Log(genderStr);
    }

    private Sprite LoadSprite(string spriteName)
    {
        if (spriteCache.ContainsKey(spriteName))
        {
            return spriteCache[spriteName];
        }
        else
        {
            Sprite sprite = Resources.Load<Sprite>($"Preview/{spriteName}");
            if (sprite != null)
            {
                spriteCache[spriteName] = sprite;
            }
            else
            {
                Debug.LogWarning($"Không tìm thấy hình ảnh: {spriteName}");
            }
            return sprite;
        }
    }


    public void SetHeadIndex(int index)
    {
        headIndex = index + 1;
        UpdateCharacterPreview();
    }

    public void SetBodyIndex(int index)
    {
        bodyIndex = index + 1;
        UpdateCharacterPreview();
    }

    public void SetLegsIndex(int index)
    {
        legsIndex = index + 1;
        UpdateCharacterPreview();
    }
    // Hàm để chọn giới tính và cập nhật
    public void SetGender(string newGender)
    {
        AudioManager.Instance.PlaySFX("Click");
        gender = newGender == "nam" ? 0 : 1;
        UpdateCharacterPreview();
        Debug.Log(gender);
 
    }   
    public void SetClass(string newclass)
    {
        AudioManager.Instance.PlaySFX("Click");
        charclass = newclass == "kiem" ? 1 : 2;
    }
 
    public void CreateNewCharacter()
    {
        AudioManager.Instance.PlaySFX("Click");

        var characterGender = gender;
        var characterClass = charclass;

        var charname = nameInput.text;
        if (string.IsNullOrEmpty(charname))
        {
            popupManager.ShowPopup("Nhập tên nhân vật");
            return;
        }
        if (gender < 0)
        {
            popupManager.ShowPopup("Chọn Giới Tính");
            return;
        }
        if (charclass < 0)
        {
            popupManager.ShowPopup("Chọn class");
            return;
        }
        Loading.Instance.StartLoading();
        var newchar = new BaseMessage
        {
            Action = "Create_char",
            Data = new JObject
            {
                ["name"] = charname,
                ["gender"] = characterGender,
                ["Class"] = characterClass,
            }

        };
        string json = JsonConvert.SerializeObject(newchar);
        clientManager.Send(json);
     
    }
    private void HandleCreateResult(JObject data)
    {
        if (data == null)
        {
            Debug.LogError("HandleRegister: data is null");
            return;
        }

        if (popupManager == null)
        {
            Debug.LogError("popupManager is null");
            return;
        }

        bool success = data["Success"]?.ToObject<bool>() ?? false;
        bool islocal = data["islocal"]?.ToObject<bool>() ?? false;
        
        string message = data["message"]?.ToString() ?? "Có lỗi xảy ra.";
        CharacterBase character = data["character"]?.ToObject<CharacterBase>();
        Loading.Instance.EndLoading();
        if (success == false)
        {
            popupManager.ShowPopup(message);

           
        }
        else
        {
            if (character != null )
            {
                var iventory = new BaseMessage
                {
                    Action = "get_inventory_equip",
                };
                clientManager.Send(JsonConvert.SerializeObject(iventory));
                Background.SetActive(false);

                GameManager.instance.OnCharacterReceived(character,true);
                GetCharInMap(character.MapID);
                
                GameManager.instance.GetMonsterInmap(character.MapID);
                Joinmap(character.CharacterID, character.MapID, character);
                HUD.SetActive(true);
                HidePanelAndCleanup();

                OnDestroy();

            }
        }
    }

    public void GetCharInMap(int mapid)
    {
        var message = new BaseMessage()
        {
            Action = "Characters_in_map",
            Data = new JObject
            {
                ["mapid"] = mapid
            }
        };
        string json = JsonConvert.SerializeObject(message);
        clientManager.Send(json);
    }

    public void Joinmap(int characterid, int mapid, CharacterBase character)
    {
        var data = new BaseMessage
        {
            Action = "Joinmap",
            Data = new JObject
            {
                ["characterid"] = characterid,
                ["mapid"] = mapid,
                ["spawnX"] = character.X,
                ["spawnY"] = character.Y,
            }
        };
        clientManager.Send(JsonConvert.SerializeObject(data));
    }
    // Hàm để ẩn panel và giải phóng bộ nhớ
    public void HidePanelAndCleanup()
    {
        if (createCharacterPanel != null)
        {
            createCharacterPanel.SetActive(false); // Ẩn panel
        }

        // Giải phóng bộ nhớ (tùy chọn)
        if (characterInstance != null)
        {
            Destroy(characterInstance); // Xóa instance nếu không cần nữa
            characterInstance = null;
        }

        // Xóa cache sprite (nếu muốn giải phóng hoàn toàn)
        spriteCache.Clear();
    }

    void OnDestroy()
    {
        // Đảm bảo giải phóng khi object bị destroy
        if (characterInstance != null)
        {
            Destroy(characterInstance);
            characterInstance = null;
        }
        spriteCache.Clear();
    }
}