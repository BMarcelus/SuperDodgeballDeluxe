using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {
    
    public AudioClip introMusClip, menuMusClip;

    [Range(0f, 2f)]
    public float musicVolume;

    AudioSource introSource, musicSource;

    void Awake() {
        DontDestroyOnLoad(gameObject);
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        introSource = gameObject.AddComponent<AudioSource>();
        introSource.playOnAwake = false;
        introSource.loop = false;
    }

    public void PlayIntroMusic() {
        musicSource.clip = introMusClip;
        musicSource.Play();
    }

    public void PlayMenuMusic() {
        musicSource.clip = menuMusClip;
        musicSource.Play();
    }

    void Update() {
        introSource.volume = musicVolume;
        musicSource.volume = musicVolume;
    }

    public void StopIntro() {
        introSource.Stop();
    }

    public void StopMusic() {
        musicSource.Stop();
    }
}
