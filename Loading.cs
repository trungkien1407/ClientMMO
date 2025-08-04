    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;

    public class Loading : MonoBehaviour
    {
        public static Loading Instance;  // Singleton

        public Sprite[] frames;              // Animation frames
        public float frameRate = 0.033f;     // 30fps
        public GameObject loadPanel;         // Panel hiển thị loading
        public GameObject loginPanel;
        // Panel login (nếu có)
        private Image imageComponent;
        private int currentFrame = 0;
        private float timer = 0f;
        private bool isLoading = false;

        private void Awake()
        {
            // Đảm bảo Singleton Instance
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);  // Nếu đã có instance, phá hủy cái này đi.
            }

            loadPanel.SetActive(true);
       
        }

    IEnumerator Start()
    {
        yield return new WaitUntil(() => ClientManager.Instance != null);

        ClientManager.Instance.OnConnected += () =>
        {
            if(loadPanel != null)
            // Chạy coroutine đợi 0.5s rồi mới hiển thị login
            StartCoroutine(DelayShowLogin());
        };

        imageComponent = GetComponent<Image>();
        if (imageComponent == null || frames == null || frames.Length == 0)
        {
            Debug.LogError("⚠️ Loading setup is invalid!");
        }

        StartLoading();
    }

    IEnumerator DelayShowLogin()
    {
        
        yield return new WaitForSeconds(0.5f);
        ShowLoginAfterLoading();
    }

    void Update()
        {
            if (isLoading && imageComponent != null && frames.Length > 0)
            {
                timer += Time.deltaTime;
                if (timer >= frameRate)
                {
                    currentFrame = (currentFrame + 1) % frames.Length;
                    imageComponent.sprite = frames[currentFrame];
                timer %= frameRate;

            }
        }
        }

    // Gọi khi cần hiển thị loading
    public void StartLoading()
    {
        loadPanel.SetActive(true);
        Debug.Log("🌀 StartLoading: showing loadPanel");
        isLoading = true;
        currentFrame = 0;
        timer = 0f;

        if (loadPanel != null)
            loadPanel.SetActive(true);

        if (loginPanel != null)
            loginPanel.SetActive(false); // Ẩn loginPanel trong lúc loading
        
    }

    // Gọi khi loading hoàn tất (ẩn panel)
    public void EndLoading()
        {
            isLoading = false;
            if (loadPanel != null)
                loadPanel.SetActive(false);
        }

        // Ví dụ: gọi sau khi kết nối xong để hiển thị login panel
        public void ShowLoginAfterLoading()
        {
            EndLoading();

            if (loginPanel != null)
                loginPanel.SetActive(true);

            Debug.Log("✅ Loading complete. Showing login.");
        }
    }
