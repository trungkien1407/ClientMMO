using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Script
{
    public class MonsterBase
    {
        public int SpawnID { get; set; }
        public int MonsterID { get; set; }
        public string MonsterName { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public int CurrentHP { get; set; }
        public int MaxHP { get; set; }
        public bool IsAlive { get; set; }
        public int level { get; set; }
    }

  
}
