using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;
using Assets.Script;

public class LogoutHandler : MonoBehaviour
{
    private bool isQuittingHandled = false;
    ClientManager clientManager;
    private void Awake()
    {
        Application.wantsToQuit += WantsToQuit;
    }

    private void Start()
    {
        clientManager = ClientManager.Instance;
       
       // clientManager.Dispatcher.RegisterHandler("duplicate_login", DuplicateLogin);
    }
    private bool WantsToQuit()
    {
        if (!isQuittingHandled)
        {
            isQuittingHandled = true;
            StartCoroutine(HandleQuit());
            return false; // Tạm thời chặn quit
        }
        return true;
    }

    private IEnumerator HandleQuit()
    {
        Debug.Log("[LogoutHandler] Sending Leavermap...");

        if (GameManager.instance != null)
        {
            GameManager.instance.SendLeavermap();
        }

        // Đợi 0.2 giây để chắc chắn gói tin được gửi
        yield return new WaitForSeconds(0.2f);

        Debug.Log("[LogoutHandler] Proceeding to quit");
        Application.Quit();
    }

}
