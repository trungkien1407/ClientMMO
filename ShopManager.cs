using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script
{
    public class ShopManager : MonoBehaviour
    {
        public GameObject Shop;
        public GameObject shopAction;
        public GameObject npcDialog;
        public Button shopOpen;
        public Button shopClose;
        public Button item1;
        public Button item2;
        public Button buy;
        public Button buyClose;
        public TMP_Text txtname;
        public TMP_Text des;
        private int itemID;


        private void Awake()
        {
            shopOpen.onClick.AddListener(OnShopClick);
            shopClose.onClick.AddListener(OnShopClose);
            item1.onClick.AddListener(OnItem1Click);
            item2.onClick.AddListener(OnItem2Click);
            buy.onClick.AddListener(OnBuyClick);
            buyClose.onClick.AddListener(OnBuyCloseClick);
        }

        private void OnShopClick()
        {
            Shop.SetActive(true);
            npcDialog.SetActive(false);
        }
        private void OnShopClose()
        {
            Shop.SetActive(false);
        }
        private void OnItem1Click()
        {
            txtname.text = "Bình HP";
            des.text = "Giúp Hồi HP";
            itemID = 103;
            shopAction.SetActive(true);
        }
        private void OnItem2Click()
        {
            txtname.text = "Bình HP";
            des.text = "Giúp Hồi HP";
            itemID = 104;
            shopAction.SetActive(true);
        }
        private void OnBuyClick()
        {
            var message = new BaseMessage
            {
                Action = "buy_item",
                Data = new Newtonsoft.Json.Linq.JObject
                {
                  
                    ["itemID"] = itemID,
                }
            };
            OnBuyCloseClick();

            ClientManager.Instance.Send(JsonConvert.SerializeObject(message));
        }
        private void OnBuyCloseClick()
        {
            shopAction.SetActive(false);
        }

    }
}
