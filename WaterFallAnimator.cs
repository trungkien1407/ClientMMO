using UnityEngine;

public class WaterfallAnimator : MonoBehaviour
{
    public Sprite[] frames; // Gán 4 frame sprite vào đây trong Inspector
    public float frameRate = 0.2f; // tốc độ chuyển đổi
    private SpriteRenderer sr;
    private int currentFrame;
    private float timer;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (frames.Length > 0)
            sr.sprite = frames[0];
    }

    void Update()
    {
        if (frames.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            timer = 0f;
            currentFrame = (currentFrame + 1) % frames.Length;
            sr.sprite = frames[currentFrame];
        }
    }
}
