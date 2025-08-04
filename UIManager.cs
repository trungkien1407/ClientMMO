using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        public GameObject npcDialogPanel;
        public GameObject npcQuestPanel;
        public GameObject npcButtonlayout;
        public GameObject skillPanel;
        public TMP_Text npcDialogText;
        public Button questButton;
        public Button shopButton;
        public Button closeButton;
        public Button acceptButton;
        public Button declineButton;
        private ClientManager clientManager;
        public Button questPanelButton;
        public GameObject questPanel;
        public TMP_Text questDesText;
        public Button questCloseButton;
        public Button inventoryToggleButton;
        public Button inventoryCloseButton;
        public GameObject menuPanel;
        public Button OpenMenu;
        public Button CloseMenu;

        // 👉 NEW: Notification system
        public GameObject notificationPanel;
        public TMP_Text notificationText;
        public Button notificationCloseButton;
        public Sprite kiemSkill, tieuSkill;
        public Button skillSelect, closeSkill;
        public Image skillImage;
        public TMP_Text skillText;
        private int currentNpcId;
        private int pendingCompleteQuestId = 0; // 👈 Track quest cần complete
        public Button Skillbutton;
        public GameObject inventoryPanel;
        [SerializeField] private ItemActionPopup _itemActionPopup;
        private enum AcceptButtonMode
        {
            None,
            ReceiveReward,
            AcceptNextQuest,
            CompleteQuest // 👈 NEW mode
        }

        private AcceptButtonMode currentAcceptMode = AcceptButtonMode.None;

        private void Awake()
        {
            Instance = this;
            questButton.onClick.AddListener(OnQuestClick);
            OpenMenu.onClick.AddListener(OnMenuClick);
            CloseMenu.onClick.AddListener(OnCloseMenuClick);
            shopButton.onClick.AddListener(OnShopClick);
            Skillbutton.onClick.AddListener(OnSkillClick);
            closeButton.onClick.AddListener(CloseDialog);
            acceptButton.onClick.AddListener(AcceptButton);
            declineButton.onClick.AddListener(DeclineButton);
            inventoryToggleButton.onClick.AddListener(ToggleInventoryPanel);
            inventoryCloseButton.onClick.AddListener(CloseInventory);
            skillSelect.onClick.AddListener(OnSkillSelect);
            closeSkill.onClick.AddListener(CloseSkill);

            // 👉 NEW: Notification button
            if (notificationCloseButton != null)
                notificationCloseButton.onClick.AddListener(CloseNotification);
           
        }
     
        private void OnDestroy()
        {
            // Luôn hủy đăng ký
            ItemSlotUI.OnAnySlotClicked -= HandleSlotClick;
        }

        public void OnMenuClick()
        {
            menuPanel.SetActive(true);
        }
        public void OnCloseMenuClick()
        {
            menuPanel.SetActive(false);
        }
        private void HandleSlotClick(ClientInventoryItem item, bool isEquipmentSlot)
        {
            // ...và sau đó nó sẽ ra lệnh cho popup hiển thị.
            if (_itemActionPopup != null)
            {
                _itemActionPopup.Show(item, isEquipmentSlot);
            }
            else
            {
                Debug.LogError("ItemActionPopup is not assigned in UIManager inspector!");
            }
        }
        private void Start()
        {
            clientManager = ClientManager.Instance;
            clientManager.Dispatcher.RegisterHandler("active_quests", ActiveQuestHandle);
            clientManager.Dispatcher.RegisterHandler("quest_rewarded", HandleQuestRewarded);
            clientManager.Dispatcher.RegisterHandler("quest_in_progress", HandleQuestInProgress);
            clientManager.Dispatcher.RegisterHandler("npc_quest_start", HandleQuestStart);
            clientManager.Dispatcher.RegisterHandler("npc_no_quest", HandleNoQuest);
            clientManager.Dispatcher.RegisterHandler("quest_progress_update", HandleQuestProgressUpdate);
            clientManager.Dispatcher.RegisterHandler("quest_ready_to_complete", HandleQuestReadyToComplete);
            clientManager.Dispatcher.RegisterHandler("quest_can_complete", HandleQuestCanComplete);
            clientManager.Dispatcher.RegisterHandler("quest_next", HandleQuestNext);
            clientManager.Dispatcher.RegisterHandler("character_skill", HandleGetSkill);

            questPanelButton.onClick.AddListener(OnOpenQuestPanel);
            questCloseButton.onClick.AddListener(CloseQuestPanel);
            ItemSlotUI.OnAnySlotClicked += HandleSlotClick;
        }

        public void OpenNPCDialog(int npcId)
        {
            currentNpcId = npcId;
            npcDialogPanel.SetActive(true);
            npcDialogText.text = "Con cần gì ở ta ...";

          
            npcButtonlayout.SetActive(true);
            
            npcQuestPanel.SetActive(false);
          
            acceptButton.gameObject.SetActive(false);
        }
        private void ToggleInventoryPanel()
        {
            // Bật/tắt panel
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        }
        public void UpdateDialogText(string text)
        {
            npcDialogText.text = text;
        }

        private void OnQuestClick()
        {
            npcButtonlayout.SetActive(false);
            npcQuestPanel.SetActive(true);
            var message = new BaseMessage
            {
                Action = "talk_quest",
                Data = new JObject
                {
                    ["npcId"] = currentNpcId,
                }
            };

            clientManager.Send(JsonConvert.SerializeObject(message));
        }
        private void OnSkillClick()
        {
            skillPanel.SetActive(true);
            skillText.enabled = false;
        }
        private void OnSkillSelect()
        {
            skillText.enabled = true;
        }
        private void CloseSkill()
        {
            skillText.enabled = false;
            skillPanel.SetActive(false);
        }

            private void HandleGetSkill(JObject data)
            {
                 var skills = data["skills"] as JArray;
            if (skills == null) return;
            
            foreach (var skill in skills)
            {
                var skillID = skill["SkillID"].ToObject<int>();
                var skillSprite = skillID == 101 ? kiemSkill : tieuSkill;
                skillImage.sprite = skillSprite;
                skillText.text = skillID == 101 ? "Đây là chiêu thức cơ bản của phái kiếm." : "Đây là chiêu thức cơ bản của phái tiêu";
            }

            }
        private void OnShopClick()
        {
            // Shop logic here
        }

        private void CloseDialog()
        {
            npcDialogPanel.SetActive(false);
            // Reset trạng thái
            pendingCompleteQuestId = 0;
            currentAcceptMode = AcceptButtonMode.None;
        }

        private void AcceptButton()
        {
            JObject data = new JObject
            {
                ["npcId"] = currentNpcId
            };

            string action = currentAcceptMode switch
            {
                AcceptButtonMode.ReceiveReward => "accept_quest_reward",
                AcceptButtonMode.AcceptNextQuest => "accept_next_quest",
                AcceptButtonMode.CompleteQuest => "talk_quest", 
                _ => null
            };

          
            if (currentAcceptMode == AcceptButtonMode.CompleteQuest)
            {
                data["type"] = "complete_quest";
                data["questId"] = pendingCompleteQuestId;
            }

            if (action != null)
            {
                var message = new BaseMessage
                {
                    Action = action,
                    Data = data
                };

                clientManager.Send(JsonConvert.SerializeObject(message));
            }

            acceptButton.gameObject.SetActive(false);
            currentAcceptMode = AcceptButtonMode.None;
        }

        private void DeclineButton()
        {
            npcQuestPanel.SetActive(false);
            npcButtonlayout.SetActive(true);
            npcDialogPanel.SetActive(false);

            // Reset trạng thái
            pendingCompleteQuestId = 0;
            currentAcceptMode = AcceptButtonMode.None;
        }

        private void OnOpenQuestPanel()
        {
            questPanel.SetActive(true);
            questDesText.text = "Đang tải nhiệm vụ...";

            var message = new BaseMessage
            {
                Action = "talk_quest",
                Data = new JObject
                {
                    ["type"] = "get_active",
                    ["npcId"] = currentNpcId
                }
            };
            clientManager.Send(JsonConvert.SerializeObject(message));
        }

        public void CloseQuestPanel()
        {
            questPanel.SetActive(false);
        }

        // 👉 NEW: Show notification
        private IEnumerator ShowNotification(string message)
        {
            yield return new WaitForSeconds(0.5f);
            CloseDialog();
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(true);
                if (notificationText != null)
                    notificationText.text = message;
            }
            else
            {
               
               
                Debug.Log($"[Quest Notification] {message}");
            }
        }

     
        private void CloseNotification()
        {
            if (notificationPanel != null)
                notificationPanel.SetActive(false);
        }

       
        private void HandleQuestReadyToComplete(JObject data)
        {
            int questId = data["questId"]?.ToObject<int>() ?? 0;
            string message = data["message"]?.ToString() ?? "Nhiệm vụ đã hoàn thành! Hãy quay về NPC để nhận thưởng.";

           StartCoroutine(ShowNotification(message));

            // Optional: Update quest panel if it's open
            if (questPanel.activeSelf)
            {
                OnOpenQuestPanel(); // Refresh quest list
            }
        }

    
        private void HandleQuestCanComplete(JObject data)
        {
            int questId = data["questId"]?.ToObject<int>() ?? 0;
            string message = data["message"]?.ToString() ?? "Xuất sắc! Bạn đã hoàn thành nhiệm vụ.";
            bool canComplete = data["canComplete"]?.ToObject<bool>() ?? false;

            npcDialogText.text = message;

            if (canComplete)
            {
             
                acceptButton.gameObject.SetActive(true);
                acceptButton.GetComponentInChildren<TMP_Text>().text = "Nhận thưởng";
                currentAcceptMode = AcceptButtonMode.CompleteQuest;
                pendingCompleteQuestId = questId;
            }
        }

   
        private void HandleQuestNext(JObject data)
        {
            JObject quest = data["quest"] as JObject;
            string questName = quest?["Name"]?.ToString() ?? "Quest mới";

           StartCoroutine(ShowNotification($"Bạn đã nhận nhiệm vụ tiếp theo: {questName}"));

          
            npcDialogText.text = $"Con đã nhận nhiệm vụ tiếp theo: {questName}";

          
            if (questPanel.activeSelf)
            {
                OnOpenQuestPanel();
            }
        }

        private void HandleQuestProgressUpdate(JObject data)
        {
            int questId = data["questId"]?.ToObject<int>() ?? 0;
            var objectives = data["objectives"] as JArray;

            if (objectives == null) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<b>Tiến độ nhiệm vụ {questId}</b>\n");

            foreach (var obj in objectives)
            {
                string desc = obj["Description"]?.ToString() ?? "???";
                int current = obj["Current"]?.ToObject<int>() ?? 0;
                int required = obj["Required"]?.ToObject<int>() ?? 0;

                sb.AppendLine($"- {desc}: {current}/{required}");
            }

            questDesText.text = sb.ToString();
        }

        public void ActiveQuestHandle(JObject data)
        {
            var quests = data["quests"] as JArray;

            if (quests == null || quests.Count == 0)
            {
                questDesText.text = "Bạn chưa nhận nhiệm vụ nào, hãy tìm npc Ông già để nhận nhiệm vụ";
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<b>Đang hoạt động: {quests.Count}</b>\n");

            foreach (var quest in quests)
            {
                string name = quest["Name"]?.ToString() ?? "???";
                string desc = quest["Description"]?.ToString() ?? "Không có mô tả.";
                string status = quest["Status"]?.ToString() ?? "Unknown";

                sb.AppendLine($"<b>{name}</b>");
                sb.AppendLine(desc);

              
                string statusText = status switch
                {
                    "InProgress" => " Đang thực hiện",
                    "PendingReward" => " Chờ nhận thưởng",
                    "Completed" => " Hoàn thành",
                    _ => "❓ Không rõ"
                };
                sb.AppendLine($"<color=yellow>Trạng thái: {statusText}</color>");

                var objectives = quest["Objectives"] as JArray;
                if (objectives != null)
                {
                    sb.AppendLine("<b>Mục tiêu:</b>");
                    foreach (var obj in objectives)
                    {
                        int current = obj["Current"]?.Value<int>() ?? 0;
                        int required = obj["Required"]?.Value<int>() ?? 1;
                        bool isComplete = obj["IsComplete"]?.Value<bool>() ?? false;

                       
                        sb.AppendLine($" Tiêu diệt: {current}/{required}");
                    }
                }
                sb.AppendLine("------------------------");
            }

            questDesText.text = sb.ToString();
        }

        private void HandleQuestRewarded(JObject data)
        {
            string message = data["message"]?.ToString() ?? "Bạn đã nhận thưởng thành công!";
            npcDialogText.text = message;

            // Reset buttons
            acceptButton.gameObject.SetActive(false);
            currentAcceptMode = AcceptButtonMode.None;
            pendingCompleteQuestId = 0;

            // Show notification
            StartCoroutine(ShowNotification(message));

            // Refresh quest panel if open
            if (questPanel.activeSelf)
            {
                OnOpenQuestPanel();
            }
        }

        private void HandleQuestInProgress(JObject data)
        {
            string message = data["message"]?.ToString();
            npcDialogText.text = message;

            acceptButton.gameObject.SetActive(false); // Không cần accept
        }

        private void HandleQuestStart(JObject data)
        {
            JObject quest = data["quest"] as JObject;
            string questName = quest?["Name"]?.ToString();
            npcDialogText.text = $"Con đã nhận nhiệm vụ: {questName}";

            acceptButton.gameObject.SetActive(false);

            // Show notification
            StartCoroutine(ShowNotification($"Đã nhận nhiệm vụ: {questName}"));
        }

        private void HandleNoQuest(JObject data)
        {
            string message = data["message"]?.ToString();
            npcDialogText.text = message;

            acceptButton.gameObject.SetActive(false);
        }
        public void CloseInventory()
        {
            inventoryPanel.SetActive(false);
        }

        // 👉 REMOVED: HandleQuestCompleted - không còn sử dụng
        // private void HandleQuestCompleted(JObject data) { ... }
    }
}