using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;
namespace Assets.Script
{
    public class SkillManager : MonoBehaviour
    {
        public static SkillManager Instance;
        public Dictionary<int, SkillData> skillDictionary;
        private Dictionary<int, float> skillCooldowns = new Dictionary<int, float>();
        public List<SkillData> skillDataList;
        public CharacterData character;

        public void Awake()
        {
            Instance = this;
            skillDictionary = new Dictionary<int, SkillData>();
            SetSkills(skillDataList);
        }

        public bool CanUseSkill(int Skillid)
        {
            if (skillCooldowns.TryGetValue(Skillid, out float lastUsed))
            {
                if (Time.time < lastUsed + skillDictionary[Skillid].cooldown)
                    return false;
            }
            return true;
        }

        // Method này bạn sẽ gọi với target đã chọn từ hệ thống targeting của bạn
        public void PlaySkill(int skillid, Vector2 playerPosition, bool facingRight, Transform selectedTarget,bool islocal)
        {
            if (!CanUseSkill(skillid))
            {
                Debug.Log($"Skill {skillid} đang trong cooldown!");
                return;
            }

            if (selectedTarget == null)
            {
                Debug.Log("Không có target được chọn!");
                return;
            }

            if (!skillDictionary.TryGetValue(skillid, out var skillData))
            {
                Debug.LogWarning($"Skill {skillid} không tồn tại!");
                return;
            }

            var effectInstance = SkillPool.Instance.GetSkillEffect();
            effectInstance.Init(skillData, playerPosition, facingRight, selectedTarget,islocal);
            skillCooldowns[skillid] = Time.time;
        }

        public void SetSkills(List<SkillData> skills)
        {
            skillDataList = skills;
            skillDictionary.Clear();
            foreach (var skill in skills)
            {
                skillDictionary[skill.skillID] = skill;
            }
        }
        
    }
}
