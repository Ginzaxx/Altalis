using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[System.Serializable]
public class SoundGroup
{
    public string name;
    public AudioClip[] clips;
}

[System.Serializable]
public class BgmGroup
{
    public string name;
    public AudioClip clip;
}

[System.Serializable]
public class AmbienceGroup
{
    public string name;
    public AudioClip clip;
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Daftar Suara (SFX)")]
    public SoundGroup[] soundGroups;

    [Header("Daftar Background Music (BGM)")]
    public BgmGroup[] bgmGroups;

    [Header("Daftar Suara Ambience (3D Looping)")]
    public AmbienceGroup[] ambienceGroups;

    [Header("Volume Control")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float ambienceVolume = 1f;

    [Header("3D Settings")]
    public bool use3DSound = true;
    public float minDistance = 1f;
    public float maxDistance = 25f;

    private AudioSource bgmSource;

    // 🔊 Sekarang tiap nama bisa punya beberapa sumber (List)
    private readonly Dictionary<string, List<AudioSource>> ambienceSources = new();

    private void Awake()
    {
        if (!Application.isPlaying) return;

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f; // BGM selalu 2D
    }

    // 🔊 Play Sound (SFX)
    public static void PlaySound(string groupName, float volume = 1f, Vector3? position = null)
    {
        if (instance == null) return;

        SoundGroup group = Array.Find(instance.soundGroups, g => g.name == groupName);
        if (group == null || group.clips == null || group.clips.Length == 0)
        {
            Debug.LogWarning($"[SoundManager] Sound group '{groupName}' tidak ditemukan atau kosong!");
            return;
        }

        AudioClip clip = group.clips[Random.Range(0, group.clips.Length)];
        GameObject temp = new GameObject($"SFX_{groupName}");
        temp.transform.position = position ?? Camera.main.transform.position;

        AudioSource src = temp.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = volume * instance.sfxVolume;

        if (instance.use3DSound)
        {
            src.spatialBlend = 1f;
            src.minDistance = instance.minDistance;
            src.maxDistance = instance.maxDistance;
        }

        src.Play();
        Destroy(temp, clip.length);
    }

    // 🎵 Play BGM
    public static void PlayBGM(string groupName)
    {
        if (instance == null) return;

        BgmGroup group = Array.Find(instance.bgmGroups, g => g.name == groupName);
        if (group == null || group.clip == null)
        {
            Debug.LogWarning($"[SoundManager] BGM '{groupName}' tidak ditemukan atau kosong!");
            return;
        }

        instance.bgmSource.clip = group.clip;
        instance.bgmSource.volume = instance.bgmVolume;
        instance.bgmSource.Play();
    }

    public static void StopBGM()
    {
        if (instance == null) return;
        instance.bgmSource.Stop();
    }

    // 🌫️ Play Ambience (looping 3D, bisa banyak)
    public static AudioSource PlayAmbience(string groupName, Vector3? position = null)
    {
        if (instance == null) return null;

        AmbienceGroup group = Array.Find(instance.ambienceGroups, g => g.name == groupName);
        if (group == null || group.clip == null)
        {
            Debug.LogWarning($"[SoundManager] Ambience '{groupName}' tidak ditemukan atau kosong!");
            return null;
        }

        GameObject go = new GameObject($"Ambience_{groupName}");
        go.transform.position = position ?? Camera.main.transform.position;

        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = group.clip;
        src.loop = true;
        src.volume = instance.ambienceVolume;
        src.playOnAwake = false;
        src.spatialBlend = 1f;
        src.minDistance = instance.minDistance;
        src.maxDistance = instance.maxDistance;
        src.rolloffMode = AudioRolloffMode.Logarithmic;
        src.Play();

        if (!instance.ambienceSources.ContainsKey(groupName))
            instance.ambienceSources[groupName] = new List<AudioSource>();

        instance.ambienceSources[groupName].Add(src);
        return src;
    }

    // ❌ Hentikan satu ambience per nama (semua instance-nya)
    public static void StopAmbience(string groupName)
    {
        if (instance == null) return;

        if (instance.ambienceSources.TryGetValue(groupName, out var list))
        {
            foreach (var src in list)
                if (src != null)
                    Destroy(src.gameObject);

            list.Clear();
            instance.ambienceSources.Remove(groupName);
        }
    }

    // ❌ Hentikan semua ambience
    public static void StopAllAmbience()
    {
        if (instance == null) return;

        foreach (var list in instance.ambienceSources.Values)
        {
            foreach (var src in list)
                if (src != null)
                    Destroy(src.gameObject);
        }

        instance.ambienceSources.Clear();
    }

    // ⚙️ Volume Control
    public static void SetSFXVolume(float v)
    {
        if (instance == null) return;
        instance.sfxVolume = Mathf.Clamp01(v);
    }

    public static void SetBGMVolume(float v)
    {
        if (instance == null) return;
        instance.bgmVolume = Mathf.Clamp01(v);
        instance.bgmSource.volume = instance.bgmVolume;
    }

    public static void SetAmbienceVolume(float v)
    {
        if (instance == null) return;
        instance.ambienceVolume = Mathf.Clamp01(v);

        foreach (var list in instance.ambienceSources.Values)
            foreach (var src in list)
                if (src != null)
                    src.volume = instance.ambienceVolume;
    }
}

    // 🧩 Contoh Pemakaian
    /*
        // 🔊 Play 3D SFX di posisi tertentu
        SoundManager.PlaySound("Explosion", 1f, transform.position);

        // 🔊 Play 2D SFX (UI, button, dsb)
        SoundManager.PlaySound("ButtonClick", 0.8f);

        // 🎵 Play / Stop BGM
        SoundManager.PlayBGM("MainTheme");
        SoundManager.StopBGM();

        // 🌫️ Play ambience 3D yang looping di posisi tertentu
        SoundManager.PlayAmbience("Rain", transform.position);

        // 🌫️ Play ambience di sekitar kamera (2D feel)
        SoundManager.PlayAmbience("Wind");

        // ❌ Hentikan satu ambience
        SoundManager.StopAmbience("Rain");

        // ❌ Hentikan semua ambience
        SoundManager.StopAllAmbience();
    */
