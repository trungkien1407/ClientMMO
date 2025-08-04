// Tạo một file mới tên là Sound.cs
using UnityEngine;

[System.Serializable] // Để có thể thấy và chỉnh sửa trong Inspector
public class Sound
{
    public string name; // Tên để gọi âm thanh, ví dụ: "PlayerAttack", "MonsterHit"

    public AudioClip clip; // File âm thanh

    [Range(0f, 1f)]
    public float volume = 0.75f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;

    public bool loop = false;

    [HideInInspector] // Không cần hiển thị trong Inspector
    public AudioSource source;
}