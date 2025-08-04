using UnityEngine;

public class IdleMotion : MonoBehaviour
{
    public Transform head;
    public Transform body;
    public Transform pant;
    public Transform weapon;

    public float amplitude = 0.03f;
    public float speed = 15f;

    private Vector3 headStartPos;
    private Vector3 bodyStartPos;
    private Vector3 pantStartPos;
    private Vector3 weaponStartPos;

    private bool isIdle = true;

    void Start()
    {
        headStartPos = head.localPosition;
        bodyStartPos = body.localPosition;
        pantStartPos = pant.localPosition;
        weaponStartPos = weapon.localPosition;
    }

    void Update()
    {
        if (!isIdle) return;

        float offset = Mathf.Sin(Time.time * speed) * amplitude;

        head.localPosition = headStartPos + new Vector3(0, offset / 2, 0);
        body.localPosition = bodyStartPos + new Vector3(0, offset / 3, 0);
        weapon.localPosition = bodyStartPos + new Vector3(0, offset / 3, 0);

        pant.localPosition = pantStartPos; // Chân không nhúc nhích
    }
    public void SetIdle(bool idle)
    {
        isIdle = idle;
        if (!idle)
        {
            // Reset v? trí khi không Idle
            head.localPosition = headStartPos;
            body.localPosition = bodyStartPos;
            pant.localPosition = pantStartPos;
            weapon.localPosition = weaponStartPos;
        }
    }
}