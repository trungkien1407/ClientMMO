using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image healthFill;
    public Transform target;
    public Vector3 offset = new Vector3(0, 1, 0);
    public GameObject healthBarVisuals; // Kéo GameObject "HealthBar_Visuals" vào đây
    private Health targetHealth;
    public GameObject targetArrow;
    public TMP_Text Name;

    private float originalScaleX;

    void Awake()
    {
        originalScaleX = Mathf.Abs(transform.localScale.x);
    }

    // Dùng cho quái vật, NPC, Player
    public void SetupForCreature(Transform targetTransform, Health health)
    {
        this.target = targetTransform;
        this.targetHealth = health;
        healthBarVisuals.SetActive(true);
        healthFill.gameObject.SetActive(true);
        Name.gameObject.SetActive(true);
        Name.text = health.TargetName;
    }

    // Dùng riêng cho item, chỉ hiện mũi tên khi được target
    public void SetupForItem(Transform targetTransform, Health health)
    {
        this.target = targetTransform;
        this.targetHealth = health;
        healthBarVisuals.SetActive(false);
       
    }

    void LateUpdate()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            // Tự động vô hiệu hóa nếu target biến mất
            gameObject.SetActive(false);
            return;
        }

        transform.position = target.position + offset;

        if (healthFill.gameObject.activeSelf && targetHealth != null)
        {
            healthFill.fillAmount = Mathf.Clamp01((float)targetHealth.CurrentHealth / targetHealth.MaxHealth);
        }

        Vector3 scale = transform.localScale;
        scale.x = target.localScale.x < 0 ? -originalScaleX : originalScaleX;
        transform.localScale = scale;
    }

    public void SetTargeted(bool isTargeted)
    {
        if (targetArrow != null)
        {
            targetArrow.SetActive(isTargeted);
        }
    }

    public void ResetState()
    {
        SetTargeted(false);
        target = null;
        targetHealth = null;
        gameObject.SetActive(false);
    }
}