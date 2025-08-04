using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script
{
    public class PopupManager : MonoBehaviour
    {
        public static PopupManager Instance { get; private set; }

        [Header("UI Elements")]
        public GameObject popupPanel;
        public TMP_Text messageText;
        public Button closeButton;

        private Action onCloseCallback;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            popupPanel.SetActive(false);

            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnCloseClicked()
        {
            HidePopup();
            onCloseCallback?.Invoke();
            onCloseCallback = null;
        }

        /// <summary>
        /// Hiển thị popup với nội dung và hành động khi người dùng đóng popup (tùy chọn).
        /// </summary>
        public void ShowPopup(string message, Action onClose = null)
        {
            messageText.text = message;
            onCloseCallback = onClose;
            popupPanel.SetActive(true);
        }

        public void HidePopup()
        {
            popupPanel.SetActive(false);
        }
    }
}
