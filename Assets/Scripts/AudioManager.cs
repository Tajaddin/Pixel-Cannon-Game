using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;
    public AudioClip victoryMusic;

    [Header("SFX Clips")]
    public AudioClip cannonSelect;
    public AudioClip cannonFire;
    public AudioClip pixelFill;
    public AudioClip levelComplete;
    public AudioClip starEarned;
    public AudioClip buttonClick;
    public AudioClip slotFill;

    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.parent = transform;
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.parent = transform;
            sfxSource = sfxObj.AddComponent<AudioSource>();
        }
    }

    private void LoadSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (musicSource != null)
            musicSource.volume = musicVolume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public void PlayMusic(string musicName)
    {
        AudioClip clip = null;

        switch (musicName)
        {
            case "MainMenu":
                clip = mainMenuMusic;
                break;
            case "Gameplay":
                clip = gameplayMusic;
                break;
            case "Victory":
                clip = victoryMusic;
                break;
        }

        if (clip != null && musicSource != null)
        {
            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void PlaySFX(string sfxName)
    {
        AudioClip clip = null;

        switch (sfxName)
        {
            case "CannonSelect":
                clip = cannonSelect;
                break;
            case "CannonFire":
                clip = cannonFire;
                break;
            case "PixelFill":
                clip = pixelFill;
                break;
            case "LevelComplete":
                clip = levelComplete;
                break;
            case "StarEarned":
                clip = starEarned;
                break;
            case "ButtonClick":
                clip = buttonClick;
                break;
            case "SlotFill":
                clip = slotFill;
                break;
        }

        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void Vibrate()
    {
        if (PlayerPrefs.GetInt("Vibration", 1) == 1)
        {
            #if UNITY_IOS || UNITY_ANDROID
            Handheld.Vibrate();
            #endif
        }
    }
}
