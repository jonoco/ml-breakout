﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GameManager : MonoBehaviour
{
    [SerializeField] SceneLoader sceneLoader;
    [SerializeField] UIManager uiManager;
    [SerializeField] PlayerSupervisor playerSupervisor;

    public bool trainingMode = false;

    DateTime startTime = DateTime.Now;
    
    // Start is called before the first frame update
    void Start()
    {
        playerSupervisor = FindObjectOfType<PlayerSupervisor>();
        sceneLoader = FindObjectOfType<SceneLoader>();
        uiManager = FindObjectOfType<UIManager>();
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
    }

    public void LoseGame()
    {
        if (trainingMode)
        {
            RestartGame();
        }
        else
        {
            playerSupervisor.PauseGame();
            sceneLoader.LoadLoseScreen();
        } 
    }

    public void UpdatePoints(int points)
    {
        uiManager.UpdatePoints(points);
    }

    public void RestartGame()
    {
        // Reset any game state then let the player start again

        playerSupervisor.ResetState();
    }
}
