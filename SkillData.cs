using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EffectSlash
{
    public Sprite sprite; // Hiệu ứng tĩnh

    [Header("Animation (nếu có)")]
    public List<Sprite> animationFrames; // Các frame ảnh PNG
    public float frameRate = 10f;

    public Vector2 offset;
    public float rotation;
    public float delay;
    public float duration = 0.3f;

    [Header("Movement")]
    public bool moveToTarget = false;
    public Vector3 targetOffset;

    [Header("Collision")]
    public Vector2 hitboxSize = new Vector2(1f, 1f); // Kích thước hitbox
    public float damage = 10f; // Sát thương
}

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skill System/Skill Data")]
public class SkillData : ScriptableObject
{
    public int skillID;
    public string skillName;
    public float cooldown = 1f;
    public float skillRange = 0f;
    public List<EffectSlash> slashes;

    [Header("UI")]
    public Sprite skillIcon;
    public Sprite skillBackground;
}
public class EffectSlashRuntime
{
    public EffectSlash baseData;
    public Vector3 calculatedOffset;

    public EffectSlashRuntime(EffectSlash baseData, Vector3 calculatedOffset)
    {
        this.baseData = baseData;
        this.calculatedOffset = calculatedOffset;
    }
}
public class SkillRuntime
{
    public SkillData baseData;
    public int level;
    public float cooldown; // Được tính dựa trên cấp độ hoặc dữ liệu từ server

    public List<EffectSlashRuntime> runtimeSlashes;

    public SkillRuntime(SkillData baseData, int level, float serverCooldown = -1f)
    {
        this.baseData = baseData;
        this.level = level;

        // Ưu tiên cooldown server gửi về, nếu không thì lấy từ ScriptableObject
        this.cooldown = serverCooldown > 0 ? serverCooldown : baseData.cooldown;

        // Tính toán offset/damage dựa theo cấp độ nếu cần
        runtimeSlashes = new List<EffectSlashRuntime>();
        foreach (var slash in baseData.slashes)
        {
            // Ví dụ: damage tăng theo level
            var clone = new EffectSlash
            {
                sprite = slash.sprite,
                animationFrames = new List<Sprite>(slash.animationFrames),
                frameRate = slash.frameRate,
                offset = slash.offset,
                rotation = slash.rotation,
                delay = slash.delay,
                duration = slash.duration,
                moveToTarget = slash.moveToTarget,
                targetOffset = slash.targetOffset,
                hitboxSize = slash.hitboxSize,
                damage = CalculateDamageByLevel(slash.damage, level)
            };

            runtimeSlashes.Add(new EffectSlashRuntime(clone, clone.offset));
        }
    }

    private float CalculateDamageByLevel(float baseDamage, int level)
    {
        return baseDamage + (level - 1) * 5f; // ví dụ mỗi cấp tăng 5 damage
    }
}


