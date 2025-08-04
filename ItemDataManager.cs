

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Script
{
    public class ItemDataManager : MonoBehaviour
    {
        public static ItemDataManager Instance { get; private set; }

        // CÓ THỂ BẠN ĐÃ THAM CHIẾU ĐẾN ItemDatabase Ở ĐÂY
        public ItemDatabase _itemDatabase; 

        // Dictionary để truy cập nhanh
        private Dictionary<int, StaticItemData> _itemDictionary;

        void Awake() // <--- Dòng ~23
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeDatabase(); // <--- Gọi hàm gây lỗi
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }


        void InitializeDatabase()
        {
            _itemDictionary = new Dictionary<int, StaticItemData>();

            // *** DÒNG 28 RẤT CÓ THỂ LÀ DÒNG NÀY ***
            foreach (var itemData in _itemDatabase.allItems) // <--- LỖI Ở ĐÂY!
            {
                if (!_itemDictionary.ContainsKey(itemData.ItemID))
                {
                    _itemDictionary.Add(itemData.ItemID, itemData);
                }
            }
        }

        public StaticItemData GetItemData(int itemID)
        {
            _itemDictionary.TryGetValue(itemID, out StaticItemData data);
            return data;
        }
    }
}