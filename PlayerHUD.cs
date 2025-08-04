using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.GraphicsBuffer;

public class PlayerHUD : MonoBehaviour
{
    public Image healthFill; // gán thanh fill
    public TextMeshProUGUI healthText;
    public Image manaFill;
    public TextMeshProUGUI manaText;
    public Health health;


    public void SetTarget( Health health)
    {
       this.health = health;
    }
    void Update()
    {
        if (health == null) return;
        // Cập nhật phần trăm máu
        healthFill.fillAmount = Mathf.Clamp01((float)health.CurrentHealth / health.MaxHealth);
        healthText.text = health.CurrentHealth.ToString();
        manaText.text = health.CurrentMana.ToString();
        manaFill.fillAmount = Mathf.Clamp01((float)health.CurrentMana / health.MaxMana);
    }
}
