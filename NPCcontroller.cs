using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Script
{
    public class NPCcontroller : MonoBehaviour
    {
        public GameObject npcPrefabs;
        private Dictionary<int, GameObject> NPCs = new();
        public Dictionary<int, GameObject> npcs => NPCs;
        public Health health;

        private ClientManager clientmanager;


     
        private void Start()
        {
            clientmanager = ClientManager.Instance;
            clientmanager.Dispatcher.RegisterHandler("NPC", NPCInMap);
        }

        public void NPCInMap(JObject data)
        {
            var npcs = data["npc"] as JArray;

            // Nếu không có NPC hoặc danh sách rỗng → clear hết
            if (npcs == null || npcs.Count == 0)
            {
                ClearAllNPCs();
                Debug.LogWarning("No NPCs received, clearing existing NPCs.");
                return;
            }

            foreach (var npc in npcs)
            {
                var npcData = npc.ToObject<NPC>();
                if (NPCs.ContainsKey(npcData.NPCID))
                    continue;

                GameObject npcGO = Instantiate(npcPrefabs, new Vector3(npcData.X, npcData.Y, 0), Quaternion.identity);
                npcGO.name = $"NPC_{npcData.Name}_{npcData.NPCID}";

                var health = npcGO.GetComponent<Health>();
                health.MaxHealth = npcData.Health;
                health.CurrentHealth = npcData.Health;

                GameObject healthBarGO = HealthBarPool.Instance.GetHealthBar();
                healthBarGO.transform.SetParent(npcGO.transform);

                var healthBar = healthBarGO.GetComponent<HealthBar>();
                healthBar.offset = new Vector3(0, 1f, 0);
                healthBar.SetupForCreature(npcGO.transform, health);
                healthBar.Name.text = npcData.Name;

                health.healthBar = healthBar;
                health.SetName(npcData.Name);
                health.SetId(npcData.NPCID);

                NPCs[npcData.NPCID] = npcGO;
            }
        }


        private void ClearAllNPCs()
        {
            foreach (var npc in NPCs.Values)
            {
                Destroy(npc); // huỷ gameObject
            }
            NPCs.Clear(); // clear dictionary
        }
        

    }
}
