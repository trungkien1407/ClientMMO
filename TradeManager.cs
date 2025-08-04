using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script
{
    public class TradeManager : MonoBehaviour
    {
        public static TradeManager instance;
        public GameObject TradePanel;
        public Button trade;
        public Button close;

        private void Awake()
        {
            instance = this;
            trade.onClick.AddListener(OnTradeClick);
            trade.onClick.AddListener(HideTrade);
           
        }
        public void ShowTradePanel()
        {
            TradePanel.SetActive(true);
        }
        public void HideTrade()
        {
            TradePanel.SetActive(false);
        }
        public void OnTradeClick()
        {
            HideTrade();
            PopupManager.Instance.ShowPopup("Chức năng đang phát triển");
            
        }
     
    }

   
}
