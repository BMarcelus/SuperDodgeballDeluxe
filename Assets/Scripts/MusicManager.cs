using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {
    
    public AudioClip menuMusic;

    [Range(0f, 2f)]
    public float musicVolume;

    AudioSource currentMusic;

    void Awake() {
        DontDestroyOnLoad(gameObject);
        currentMusic = gameObject.AddComponent<AudioSource>();
        currentMusic.playOnAwake = false;
        currentMusic.loop = true;
    }

    public void PlayMenuMusic() {
        currentMusic.clip = menuMusic;
        currentMusic.Play();
    }

    void Update() {
        currentMusic.volume = musicVolume;
    }

    public void Stop() {
        currentMusic.Stop();
    }
}
