using System;
using System.Collections;
using System.Text;
using UnityEngine;
using NativeWebSocket;
using Assets.Script;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UnityEngine.UIElements;

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instance { get; private set; }

    private WebSocket websocket;
    private MessageDispatcher dispatcher = new MessageDispatcher();
    public MessageDispatcher Dispatcher => dispatcher;
    private string serverUrl = "ws://127.0.0.1:14445";
    private bool isConnecting = false;
    private bool isQuitting = false;

    public event Action<string> OnMessageReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;
    private int reconnectAttempts = 0;
    private const int maxReconnectAttempts = 3;
    public enum ClientState
    {
        Loading,
        Login,
        InGame
    }
    public ClientState CurrentState { get; set; } = ClientState.Loading;

    public bool IsConnected => websocket != null && websocket.State == WebSocketState.Open;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Connect();
        OnMessageReceived += dispatcher.Dispatch;
        StartCoroutine(CheckConnectionTimeout(15f));
    }


    public async void Connect()
    {
        if (isConnecting || websocket != null && websocket.State == WebSocketState.Open)
            return;

        isConnecting = true;

        websocket = new WebSocket(serverUrl);

        websocket.OnOpen += () =>
        {
            if (isQuitting) return;

            Debug.Log("✅ Connected to server");

            CurrentState = ClientState.Login; // đã kết nối -> đang ở login

            OnConnected?.Invoke();
        };


        websocket.OnError += (e) =>
        {
            if (isQuitting) return;
            Debug.LogError("❌ WebSocket Error: " + e);
        };

        websocket.OnClose += (e) =>
        {
            if (isQuitting) return;

            Debug.LogWarning("⚠️ WebSocket closed with code: " + e);
            OnDisconnected?.Invoke();

            if (CurrentState == ClientState.InGame)
            {
                reconnectAttempts = 0;
                StartCoroutine(Reconnect());
            }
        };




        websocket.OnMessage += (bytes) =>
        {
            if (isQuitting) return;

            string message = Encoding.UTF8.GetString(bytes);
           Debug.Log("📨 Received: " + message);
            OnMessageReceived?.Invoke(message);
        };

        await websocket.Connect();
        isConnecting = false;
    }
    public void SetClientState(ClientState newState)
    {
        CurrentState = newState;
    }


    private IEnumerator Reconnect()
    {
        while (reconnectAttempts < maxReconnectAttempts)
        {
            if (isConnecting) // nếu đang kết nối thì đợi
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            reconnectAttempts++;
            Debug.Log($"🔁 Thử kết nối lại lần {reconnectAttempts}...");

            yield return new WaitForSeconds(5f);

            Connect();

            // đợi thêm vài giây để kết nối hoàn tất
            yield return new WaitForSeconds(3f);

            if (IsConnected)
            {
                Debug.Log("✅ Kết nối lại thành công.");
                reconnectAttempts = 0;
                yield break;
            }
        }

        // Đảm bảo chỉ gọi popup **một lần**
        if (!isQuitting)
        {
            ShowReconnectFailedPopup();
        }
    }

    private void ShowReconnectFailedPopup()
    {
        PopupManager.Instance.ShowPopup(
            "Mất kết nối với máy chủ.\nBạn sẽ được chuyển về màn hình đăng nhập.",
            () =>
            {
                LoginManager.Instance.ClearDisconect();
            }
        );
    }



    private IEnumerator CheckConnectionTimeout(float timeout)
    {
        yield return new WaitForSeconds(timeout);

        if (!IsConnected && CurrentState == ClientState.Loading)
        {
            ShowErrorAndQuit(); // chỉ thoát game khi đang ở màn hình loading
        }
    }


    private void ShowErrorAndQuit()
    {
        Debug.LogError("Không thể kết nối đến server sau 10 giây. Thoát game...");
        PopupManager.Instance.ShowPopup("Không thể kết nối tới server", () =>
        {
            // TODO: Hiện popup ở đây (nếu có hệ thống UI sẵn)
            // ví dụ: UIManager.Instance.ShowPopup("Không thể kết nối đến server. Thoát game.");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
        );
    }

    public async void Send(string message)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(message);
        }
        else
        {
            Debug.LogWarning("WebSocket is not open. Cannot send.");
        }
    }

    private async void OnApplicationQuit()
    {
        isQuitting = true;

        if (websocket != null)
        {
            await websocket.Close();
        }

    }

    private void Update()
    {
        if (!isQuitting)
        {
            websocket?.DispatchMessageQueue();
        }
    }
}