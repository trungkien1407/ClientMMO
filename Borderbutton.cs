using UnityEngine;

public class Borderbutton : MonoBehaviour
{
    public RectTransform topBorder, bottomBorder, leftBorder, rightBorder;
    public float cornerSize = 20f; // Kích th??c góc

    void UpdateBorders()
    {
        RectTransform buttonRect = GetComponent<RectTransform>();
        float width = buttonRect.rect.width;
        float height = buttonRect.rect.height;

        // C?p nh?t chi?u r?ng c?a vi?n ngang
        topBorder.sizeDelta = new Vector2(width - 2 * cornerSize, topBorder.sizeDelta.y);
        bottomBorder.sizeDelta = new Vector2(width - 2 * cornerSize, bottomBorder.sizeDelta.y);

        // C?p nh?t chi?u cao c?a vi?n d?c
        leftBorder.sizeDelta = new Vector2(leftBorder.sizeDelta.x, height - 2 * cornerSize);
        rightBorder.sizeDelta = new Vector2(rightBorder.sizeDelta.x, height - 2 * cornerSize);
    }

    void Update()
    {
        UpdateBorders();
    }
}
