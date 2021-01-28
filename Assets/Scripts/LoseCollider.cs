using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseCollider : MonoBehaviour
{
    SoundManager soundManager;

    AudioClip loseSound;

    private void Start()
    {
        soundManager = FindObjectOfType<SoundManager>();
    }
    private void OnTriggerEnter2D(Collider2D other) 
    {
        soundManager.PlaySound(loseSound);
        FindObjectOfType<SceneLoader>().LoadLoseScreen(); 
    }
}
