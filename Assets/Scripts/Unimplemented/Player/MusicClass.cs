using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicClass : MonoBehaviour
{
    private AudioSource audioSource;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (GameManager.isMusicPlaying) {
            
            if (audioSource != null) {
                audioSource.Stop();
            }
            Destroy(gameObject);
        } else {
            GameManager.isMusicPlaying = true;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void PlayMusic()
    {
        if (audioSource.isPlaying) return;
        audioSource.Play();
    }

    public void StopMusic()
    {
        if (audioSource != null) {
            audioSource.Stop();
        }
        
    }
}