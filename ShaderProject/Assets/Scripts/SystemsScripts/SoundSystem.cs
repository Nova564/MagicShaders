using System.Collections.Generic;
using UnityEngine;

public class SoundSystem : MonoBehaviour
{
    public static SoundSystem Instance;

    List<AudioSource> musicSources;
    List<AudioSource> sfxSources;

    [Header("Settings")]
    public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;

    private AudioClip lastSFXClip;

    void Awake()
    {
        // Singleton
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

        // Init Music
        musicSources = new List<AudioSource>();
        for (int i = 0; i < 5; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.volume = musicVolume;
            musicSources.Add(source);
        }

        // Init SFX
        sfxSources = new List<AudioSource>();
        for (int i = 0; i < 5; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = sfxVolume;
            sfxSources.Add(source);
        }
    }

    // -------- MUSIC ----------
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        AudioSource freeSource = musicSources.Find(s => !s.isPlaying);

        if (freeSource == null)
            return;

        freeSource.clip = clip;
        freeSource.loop = loop;
        freeSource.volume = musicVolume;
        freeSource.Play();
    }

    public void StopMusic(AudioClip clip)
    {
        foreach (AudioSource source in musicSources)
        {
            if (source.isPlaying && source.clip == clip)
                source.Stop();
        }
    }

    // -------- SFX + Pool dynamique ----------
    public void PlaySFX(AudioClip clip, float volume)
    {
        AudioSource freeSource = sfxSources.Find(s => !s.isPlaying);

        // Si aucune source libre → on en ajoute une
        if (freeSource == null)
        {
            freeSource = gameObject.AddComponent<AudioSource>();
            freeSource.playOnAwake = false;
            freeSource.volume = sfxVolume;
            sfxSources.Add(freeSource);
        }

        freeSource.clip = clip;
        freeSource.volume = volume;
        freeSource.Play();
    }

    public void PlaySFX(List<AudioClip> clips, float volume)
    {
        if (clips == null || clips.Count == 0)
        {
            Debug.LogWarning("No AudioClips provided!");
            return;
        }

        AudioClip chosenClip;

        if (clips.Count == 1)
        {
            chosenClip = clips[0];
        }
        else
        {
            int attempts = 0;
            do
            {
                chosenClip = clips[Random.Range(0, clips.Count)];
                attempts++;
            }
            while (chosenClip == lastSFXClip && attempts < 5);
        }

        lastSFXClip = chosenClip;
        PlaySFX(chosenClip, volume);
    }

    public void StopSound(AudioClip clip)
    {
        foreach (AudioSource source in sfxSources)
        {
            if (source.isPlaying && source.clip == clip)
                source.Stop();
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        foreach (var source in musicSources)
        {
            source.volume = volume;
        }

    }
}
