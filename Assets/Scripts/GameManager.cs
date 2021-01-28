using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GameManager : MonoBehaviour
{
    [SerializeField] SceneLoader sceneLoader;
    [SerializeField] UIManager uiManager;

    [SerializeField] PlayerSupervisor playerSupervisor;
    [SerializeField] AudioClip loseSound;
    [SerializeField] AudioClip winSound;

    SoundManager soundManager;
    DateTime startTime = DateTime.Now;
    
    // Start is called before the first frame update
    void Start()
    {
        playerSupervisor = FindObjectOfType<PlayerSupervisor>();
        sceneLoader = FindObjectOfType<SceneLoader>();
        uiManager = FindObjectOfType<UIManager>();
        soundManager = FindObjectOfType<SoundManager>();
    }

    // Update is called once per frame
    void Update()
    {
        TimeSpan elapsedTime = DateTime.Now - startTime;
        uiManager.UpdateElapsedTime(elapsedTime.ToString(@"mm\:ss\:ff"));
    }

    public void StartGame()
    {
        playerSupervisor.StartGame();
    }

    public void WinGame()
    {
        playerSupervisor.PauseGame();
        sceneLoader.LoadWinScreen();
        soundManager.PlaySound(winSound);
    }

    public void LoseGame()
    {
        soundManager.PlaySound(loseSound);
        playerSupervisor.PauseGame();
        sceneLoader.LoadLoseScreen();
    }

    public void UpdatePoints(int points)
    {
        uiManager.UpdatePoints(points);
    }
}
