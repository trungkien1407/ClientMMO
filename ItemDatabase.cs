using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game Data/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        public List<StaticItemData> allItems;
    }

}
