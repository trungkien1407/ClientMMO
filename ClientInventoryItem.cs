using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    public class ClientInventoryItem 
    {
        public int InventoryItemID { get; set; }
        public int ItemID { get; set; }
        public int Quantity { get; set; }
        public int SlotNumber { get; set; }
        public int IsLocked { get; set; }

    }
    public class ClientEquipmentData
    {
        // Dùng JsonProperty để đảm bảo tên khớp với JSON từ server, bất kể quy tắc đặt tên ở client
        [JsonProperty("WeaponSlot_InventoryItemID")]
        public int? WeaponSlot_InventoryItemID { get; set; }

        [JsonProperty("AoSlot_InventoryItemID")]
        public int? AoSlot_InventoryItemID { get; set; }

        [JsonProperty("QuanSlot_InventoryItemID")]
        public int? QuanSlot_InventoryItemID { get; set; }

        [JsonProperty("GiaySlot_InventoryItemID")]
        public int? GiaySlot_InventoryItemID { get; set; }

        [JsonProperty("GangTaySlot_InventoryItemID")]
        public int? GangTaySlot_InventoryItemID { get; set; }

        [JsonProperty("BuaSlot_InventoryItemID")]
        public int? BuaSlot_InventoryItemID { get; set; }
    }

