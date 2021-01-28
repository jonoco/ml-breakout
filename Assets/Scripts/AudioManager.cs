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
        // scenes. Delete any additional SoundManagers instantiated.
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

    // private void Update() {

    //     audioSource = GetComponent<AudioSource>();
    //     Debug.Log(audioSource.name);
    // }

    // Update is called once per frame
    public void PlaySound(AudioClip sound)
    {
        audioSource.clip = sound;
        audioSource.Play();
    }
}
