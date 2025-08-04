using Assets.Script;
using TMPro;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.TextCore.Text;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public GameObject RegisterPanel;
    public GameObject LoginPanel;
    public GameObject Background;
    public GameObject CreateCharacterPanel;
    private ClientManager clientManager;
    private PopupManager popupManager;
    private CreateCharacterManager createCharacterManager;
    private TilemapLoader tilemapLoader;
    private GameManager gameManager;
    public static LoginManager Instance;
    public GameObject HUD;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        clientManager = ClientManager.Instance;
        clientManager.Dispatcher.RegisterHandler("login_result", HandleLoginResult);
      
        clientManager.Dispatcher.RegisterHandler("duplicate_login", HandleDuplicateLogin);

        popupManager = PopupManager.Instance;
        createCharacterManager = CreateCharacterManager.instance;
        tilemapLoader = TilemapLoader.Instance;
        gameManager = GameManager.instance;
    }

    public  void LoginClick()
    {
        AudioManager.Instance.PlaySFX("Click");
        var username = usernameInput.text;
        var password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            popupManager.ShowPopup("Nhập đầy đủ thông tin!");
            return;
        }
        Loading.Instance.StartLoading();
        var logindata = new BaseMessage
        {
            Action = "login",
            Data = new JObject
            {
                ["username"] = username,
                ["password"] = password,
            }
        };

     
        string json = JsonConvert.SerializeObject(logindata);
        clientManager.Send(json); // Gửi dữ liệu lên server
      

    }

    public void RegisterClick()
    {
        AudioManager.Instance.PlaySFX("Click");
        RegisterPanel.SetActive(true);
     

        LoginPanel.SetActive(false);
    }
    public void HandleDuplicateLogin(JObject data)
    {
        var message = data["message"]?.ToString();
        if (data == null)
        {
            Debug.Log("Duplicate data null");
            return;
        }
        popupManager.ShowPopup(message,() =>
        {


            ClearDisconect();
        }
        );

    }
    public void HandleLoginResult(JObject data)
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
        bool success = data["success"]?.ToObject<bool>() ?? false;
        string message = data["message"]?.ToString() ?? "Có lỗi xảy ra.";
        bool create = data["create"]?.ToObject<bool>() ?? false;
        bool islocal = data["islocal"]?.ToObject<bool>() ?? false;
        Loading.Instance.EndLoading();
        //if (success == true && create == true)
        //{
        //    LoginPanel.SetActive(false);
        //    CreateCharacterPanel.SetActive(true);
        //    createCharacterManager.ShowCreateCharacterPanel();
        //    clientManager.SetClientState(ClientManager.ClientState.InGame);
        //    return;
        //}
        if (success)
        {
            LoginPanel.SetActive(false);
            
            clientManager.SetClientState(ClientManager.ClientState.InGame);

            if (create)
            {
               
                CreateCharacterPanel.SetActive(true);
                createCharacterManager.ShowCreateCharacterPanel();
            }
            else
            {
                var iventory = new BaseMessage
                {
                    Action = "get_inventory_equip",
                };
                clientManager.Send(JsonConvert.SerializeObject(iventory));
                CharacterBase myCharacter = data["character"]?.ToObject<CharacterBase>();
                if (myCharacter == null)
                {
                    Debug.LogError("Character data missing.");
                    popupManager.ShowPopup("Không thể tải nhân vật.");
                    return;
                }
                Background.SetActive(false);
                gameManager.GetMonsterInmap(myCharacter.MapID);
                gameManager.Init(myCharacter);
                gameManager.GetCharInMap(myCharacter.MapID);
                Joinmap(myCharacter.CharacterID, myCharacter.MapID,myCharacter);
                HUD.SetActive(true);
            }
            
        }
        else 
        {
            popupManager.ShowPopup(message,() =>
            {
                LoginPanel.SetActive(true);
            });
        }
    }


    public void Joinmap(int characterid,int mapid,CharacterBase character)
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

    public void ClearDisconect()
    {
        Background.SetActive(true);
        GameManager.instance.SendLeavermap();
        TilemapLoader.Instance.ClearCurrentMap();
        GameManager.instance.ClearPlayers();
        GameManager.instance.DestroyLocal();
        MonsterManager.instance.ClearAllMonsters();
        clientManager.SetClientState(ClientManager.ClientState.Login);
        LoginPanel.SetActive(true);
        HUD.SetActive(false);
    }
}
