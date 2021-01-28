using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    private void Awake()
    {
        // Set the first SoundManager created to persist between
        // scenes. Delete any additional SoundManagers instantiated.
        if (FindObjectsOfType<SoundManager>().Length != 0)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    public void PlaySound(AudioClip sound)
    {
        audioSource.PlayOneShot(sound);
    }
}
