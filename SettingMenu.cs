// File: SettingsMenu.cs
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Toggle để điều khiển nhạc nền (BGM)")]
    public Toggle bgmToggle;

    [Tooltip("Toggle để điều khiển hiệu ứng âm thanh (SFX)")]
    public Toggle sfxToggle;

    void Start()
    {
        // Đảm bảo AudioManager đã tồn tại
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager not found in the scene! The settings menu will not work.");
            return;
        }

        // --- BƯỚC QUAN TRỌNG: KHỞI TẠO TRẠNG THÁI CHO UI ---
        // Lấy trạng thái mute hiện tại từ AudioManager và cập nhật cho các Toggle
        // Nếu BGM không bị mute (isBgmMuted = false) -> Toggle sẽ BẬT (isOn = true)
        bgmToggle.isOn = !AudioManager.Instance.isBgmMuted;
        sfxToggle.isOn = !AudioManager.Instance.isSfxMuted;


        // --- BƯỚC QUAN TRỌNG: THÊM LISTENER ---
        // Đăng ký các hàm để chúng được gọi mỗi khi giá trị của Toggle thay đổi
        bgmToggle.onValueChanged.AddListener(OnBgmToggleChanged);
        sfxToggle.onValueChanged.AddListener(OnSfxToggleChanged);
    }

    /// <summary>
    /// Hàm này được gọi tự động khi người dùng click vào BGM Toggle.
    /// </summary>
    /// <param name="isOn">Trạng thái mới của Toggle (true là BẬT, false là TẮT).</param>
    public void OnBgmToggleChanged(bool isOn)
    {
        // Logic ngược: Nếu Toggle BẬT (isOn=true), nghĩa là không Mute (mute=false).
        bool shouldMute = !isOn;
        AudioManager.Instance.MuteBGM(shouldMute);

        // (Tùy chọn) Lưu cài đặt của người dùng
        // PlayerPrefs.SetInt("MuteBGM", shouldMute ? 1 : 0);
    }

    /// <summary>
    /// Hàm này được gọi tự động khi người dùng click vào SFX Toggle.
    /// </summary>
    /// <param name="isOn">Trạng thái mới của Toggle.</param>
    public void OnSfxToggleChanged(bool isOn)
    {
        bool shouldMute = !isOn;
        AudioManager.Instance.MuteSFX(shouldMute);

        // (Tùy chọn) Chơi một âm thanh nhỏ để người dùng biết cài đặt đã thay đổi
        if (!shouldMute)
        {
            AudioManager.Instance.PlaySFX("Click"); // Giả sử bạn có âm thanh này
        }

        // (Tùy chọn) Lưu cài đặt
        // PlayerPrefs.SetInt("MuteSFX", shouldMute ? 1 : 0);
    }

    // Quan trọng: Hủy đăng ký listener khi đối tượng bị hủy để tránh lỗi
    private void OnDestroy()
    {
        if (bgmToggle != null)
        {
            bgmToggle.onValueChanged.RemoveListener(OnBgmToggleChanged);
        }
        if (sfxToggle != null)
        {
            sfxToggle.onValueChanged.RemoveListener(OnSfxToggleChanged);
        }
    }
}