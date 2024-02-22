using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicClass : MonoBehaviour
{
    private AudioSource audioSource;
    public static MusicClass instance = null;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        PlayMusic();
        instance = this;
    }

    public void PlayMusic()
    {
        if (instance != null) {
          Destroy(gameObject);
          return;
        }
        audioSource.Play();
        DontDestroyOnLoad(transform.gameObject);
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }
}
