using Assets.Script;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class RegisterManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField comfirmPass;
    public TMP_InputField emailInput;
    public GameObject RegisterPanel;
    public GameObject LoginPanel;
    private ClientManager clientManager;
    private PopupManager popupManager;
   
  
    private void Start()
    {
        clientManager = ClientManager.Instance;
        clientManager.Dispatcher.RegisterHandler("register_result", HandleRegister);
        popupManager = PopupManager.Instance;

    }
    public void RegisterClick()
    {
       var username = usernameInput.text;
        var password = passwordInput.text;
        var email = emailInput.text;
        var comfirmpass = comfirmPass.text;
       if(string.IsNullOrEmpty(username)||
          string.IsNullOrEmpty(password)||
          string.IsNullOrEmpty(comfirmpass)||
          string.IsNullOrEmpty(email))
        {
            popupManager.ShowPopup("Nhập Đầy Đủ thông tin");
            return;
        }
       if(comfirmpass != password)
        {
            popupManager.ShowPopup("Mật khẩu nhập lại k đúng");
            return;
        }
        var registerdata = new BaseMessage
        {
            Action = "register",
            Data = new JObject  {
               ["username"] = username,
                ["password"] = password,
                ["email"] = email,
           }

        };
      
        string json = JsonConvert.SerializeObject(registerdata);
        Debug.Log(json);
        clientManager.Send(json); // Gửi dữ liệu lên server
    }
    public void BackClick()
    {
        LoginPanel.SetActive(true);
        RegisterPanel.SetActive(false);
       
    }
    private void HandleRegister(JObject data)
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

        if (success)
        {
            popupManager.ShowPopup(message, () =>
            {
                RegisterPanel.SetActive(false);
                LoginPanel.SetActive(true);
                usernameInput.text = "";
                passwordInput.text = "";
                comfirmPass.text = "";
                emailInput.text = "";
            });
        }
        else
        {
            popupManager.ShowPopup(message);
        }
    }


}
