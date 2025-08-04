using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


    // File: Scripts/Data/StaticItemData.cs
  
    [CreateAssetMenu(fileName = "NewItemData", menuName = "Game Data/Item Data")]
    public class StaticItemData : ScriptableObject
    {
        // Các trường phải khớp với cột trong CSV
        public int ItemID;
        public string Name;
        public string Type;
        public int RequiredLevel;
        public string Description;
        public int BuyPrice;
        public int MaxStackSize;
    }

    

