// File: AudioManager.cs
using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Singleton quản lý tất cả các hoạt động liên quan đến âm thanh trong game.
/// Phiên bản đơn giản: Mỗi tên âm thanh tương ứng với một file audio duy nhất.
/// </summary>
public class AudioManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static AudioManager Instance { get; private set; }

    [Header("Audio Lists")]
    public Sound[] soundEffects;
    public Sound[] backgroundMusic;

    [Header("Master Volume Control")]
    public bool isSfxMuted = false;
    public bool isBgmMuted = false;

    private Dictionary<string, Sound> sfxDictionary;
    private Dictionary<string, Sound> bgmDictionary;

    void Awake()
    {
        // --- Cài đặt Singleton ---
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // --- Khởi tạo AudioSource và gán Clip ngay từ đầu ---
        sfxDictionary = new Dictionary<string, Sound>();
        foreach (Sound s in soundEffects)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip; // Gán clip trực tiếp
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.playOnAwake = false;
            sfxDictionary[s.name] = s;
        }

        bgmDictionary = new Dictionary<string, Sound>();
        foreach (Sound s in backgroundMusic)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip; // Gán clip trực tiếp
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.playOnAwake = false;
            bgmDictionary[s.name] = s;
        }
    }

    #region Public Control Methods

    /// <summary>
    /// Phát một hiệu ứng âm thanh (SFX) bằng tên của nó.
    /// </summary>
    public void PlaySFX(string name)
    {
        if (isSfxMuted) return;

        if (sfxDictionary.TryGetValue(name, out Sound sound))
        {
            if (sound.clip == null)
            {
                Debug.LogWarning("Sound: " + name + " is missing an AudioClip!");
                return;
            }

            // Nếu âm thanh cần lặp lại (ví dụ: tiếng bước chân), dùng Play().
            if (sound.loop)
            {
                sound.source.Play();
            }
            // Nếu là hiệu ứng một lần, dùng PlayOneShot để không ngắt các âm thanh khác.
            else
            {
                sound.source.PlayOneShot(sound.clip, sound.volume);
            }
        }
        else
        {
            Debug.LogWarning("Sound Effect: " + name + " not found!");
        }
    }

    /// <summary>
    /// Phát một bản nhạc nền (BGM).
    /// </summary>
    public void PlayBGM(string name)
    {
        StopAllBGM();

        if (bgmDictionary.TryGetValue(name, out Sound sound))
        {
            if (sound.clip != null)
            {
                sound.source.mute = isBgmMuted;
                sound.source.Play();
            }
        }
        else
        {
            Debug.LogWarning("Background Music: " + name + " not found!");
        }
    }

    /// <summary>
    /// Dừng một hiệu ứng âm thanh đang phát (hữu ích cho các âm thanh loop).
    /// </summary>
    public void StopSFX(string name)
    {
        if (sfxDictionary.TryGetValue(name, out Sound sound))
        {
            sound.source.Stop();
        }
    }

    public void StopAllBGM()
    {
        foreach (var bgm in bgmDictionary.Values)
        {
            bgm.source.Stop();
        }
    }

    #endregion

    #region Mute Control Methods

    public void MuteSFX(bool mute)
    {
        isSfxMuted = mute;
        if (mute)
        {
            foreach (var sfx in sfxDictionary.Values)
            {
                if (sfx.source.isPlaying && sfx.loop)
                {
                    sfx.source.Stop();
                }
            }
        }
    }

    public void MuteBGM(bool mute)
    {
        isBgmMuted = mute;
        foreach (var bgm in bgmDictionary.Values)
        {
            bgm.source.mute = mute;
        }
    }

    #endregion
}