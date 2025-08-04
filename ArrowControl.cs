using UnityEngine;

public class ArrowControl : MonoBehaviour
{
    public float amplitude = 0.1f; // biên độ di chuyển
    public float frequency = 7f; // tần số di chuyển

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos + Vector3.right * Mathf.Sin(Time.time * frequency) * amplitude;
    }
}
