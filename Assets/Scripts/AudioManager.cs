using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    public static AudioManager Instance = null;
    private void Awake()
    {
        // Set the first SoundManager created to persist between
        // scenes. Make any additional AudioManagers delete themselves.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        DontDestroyOnLoad(audioSource);
    }

    // This function plays sounds that continue playing
    // through scene transitions.
    public void PlaySoundBetweenScenes(AudioClip sound)
    {
        audioSource.clip = sound;
        audioSource.Play();
    }

    // This function plays the given sound without cancelling any
    // sounds already being played by the audioSource. It's ideal for
    // frequent environmental sounds that are likely to overlap.
    public void PlaySound(AudioClip sound)
    {
        audioSource.PlayOneShot(sound);
    }
}
