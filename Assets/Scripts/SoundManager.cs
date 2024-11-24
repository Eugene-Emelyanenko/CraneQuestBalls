using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;

    [Range(0f, 1f)] public float volume;
    [Range(0.1f, 3f)] public float pitch;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public List<Sound> sounds = new List<Sound>();

    public AudioSource sfxSource;
    public AudioSource musicSource;
    private Sound backGroundSound;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        backGroundSound = FindSound("Background");
        PlayBackgroundMusic();
    }

    private void PlayBackgroundMusic()
    {
        musicSource.clip = backGroundSound.clip;
        musicSource.volume = backGroundSound.volume;
        musicSource.pitch = backGroundSound.pitch;
        musicSource.loop = true;
        musicSource.Play();
    }
    
    public void PlayClip(string name)
    {
        Sound sound = FindSound(name);
        sfxSource.volume = sound.volume;
        sfxSource.pitch = sound.pitch;
        sfxSource.PlayOneShot(sound.clip);
    }

    private Sound FindSound(string name) => sounds.Find(sound => sound.name == name);
}