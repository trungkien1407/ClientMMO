using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script
{
    public class MonsterData
    {
        public int SpawnID { get; set; }
        public int MonsterID { get; set; }
        public string MonsterName { get; set; }
        public string MonsterImg { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
        public bool IsAlive { get; set; }
        public int level { get; set; }

        public MonsterData UpdateFromBase(MonsterBase monsterBase)
        {
            this.SpawnID = monsterBase.SpawnID;
            this.MonsterID = monsterBase.MonsterID;
            this.X = monsterBase.X;
            this.Y = monsterBase.Y;
            this.CurrentHP = monsterBase.CurrentHP;
            this.MaxHP = monsterBase.MaxHP;
            this.IsAlive = monsterBase.IsAlive;
            this.level = monsterBase.level;
            return this;
        }


        public class MonsterListWrapper
        {
            public List<MonsterData> monsters;
        }

     
    }
}
