using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Script;


    public class CharacterBase 
    {
        public int CharacterID { get; set; }
        public string Name { get; set; }
        public int Gender { get; set; }  
        public int HeadID { get; set; } 
        public int BodyID { get; set; }  // id thân
        public int WeaponID { get; set; }  // id vũ khí mạc định là chưa có
        public int PantID { get; set; } // id giày 
        public int Class { get; set; }  // mặc định là chưa vào class, sau khi chơi sẽ chọn
        public int Level { get; set; }   // Mặc định level 1 khi tạo nhân vật
        public int Exp { get; set; }  // Mặc định Exp là 0 khi tạo nhân vật
        public int Health { get; set; }  // Mặc định 100 máu
        public int CurrentHealth { get; set; }
        public int CurrentMana { get; set; }
        public int Mana { get; set; } // Mặc định 100 mana
        public int Strength { get; set; }   // Mặc định chỉ số sức mạnh
        public int Intelligence { get; set; } // Mặc định chỉ số trí tuệ
        public int Dexterity { get; set; }   // Mặc định chỉ số nhanh nhẹn
        public float X { get; set; } // Mặc định vị trí X
        public float Y { get; set; }  // Mặc định vị trí Y
        public int MapID { get; set; }   // Mặc định bản đồ là MapID 1
        public int Gold { get; set; }   // Mặc định vàng của nhân vật là 1000
        public DateTime CreationDate { get; set; }
        public DateTime LastPlayTime { get; set; }
        public CharacterBase UpdateCharacter(CharacterData characterBase)
        {
            CharacterID = characterBase.CharacterID;
            Name = characterBase.Name;
            Gender = characterBase.Gender;
            HeadID = characterBase.HeadID;
            BodyID = characterBase.BodyID;
            WeaponID = characterBase.WeaponID;
            PantID = characterBase.PantID;
            Class = characterBase.Class;
            Level = characterBase.Level;
            Exp = characterBase.Exp;
            Health = characterBase.Health;
            CurrentHealth = characterBase.CurrentHealth;
            CurrentMana = characterBase.CurrentMana;
            Mana = characterBase.Mana;
            Strength = characterBase.Strength;
            Intelligence = characterBase.Intelligence;
            Dexterity = characterBase.Dexterity;
            X = characterBase.X;
            Y = characterBase.Y;
            MapID = characterBase.MapID;
            Gold = characterBase.Gold;
            CreationDate = characterBase.CreationDate;
            LastPlayTime = characterBase.LastPlayTime;
            return this;
        }
    }
 

