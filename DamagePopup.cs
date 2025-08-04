using UnityEngine;
using TMPro;
using Assets.Script;

public class DamagePopup : MonoBehaviour
{
    public TextMeshPro text;
    public float moveUpSpeed = 1f;
    public float fadeSpeed = 1f;

    private float timer = 1f;

    public void Setup(int damage)
    {
        text.text = $"-{damage}";
        text.color = Color.red;
        timer = 1f;
    }

    void Update()
    {
        transform.position += Vector3.up * moveUpSpeed * Time.deltaTime;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            DamagePopupPool.Instance.ReturnPopup(gameObject);
        }

        Color c = text.color;
        c.a -= fadeSpeed * Time.deltaTime;
        text.color = c;
    }
}
