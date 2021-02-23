using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    [SerializeField] SceneLoader sceneLoader;
    [SerializeField] UIManager uiManager;
    [SerializeField] PlayerSupervisor playerSupervisor;
    [SerializeField] PlayerSupervisor[] playerSupervisors;
    [SerializeField] AudioClip loseSound;
    [SerializeField] AudioClip winSound;

    [SerializeField] GameMode gameMode = GameMode.UntilLose;

    public enum GameMode
    {
        UntilLose,
        UntilWin
    }
    public bool trainingMode = false;

    DateTime startTime = DateTime.Now;
    public TimeSpan elapsedTime;
    
    // Start is called before the first frame update
    void Awake()
    {
        if (trainingMode)
        {
            playerSupervisors = FindObjectsOfType<PlayerSupervisor>();
        }
   
        // Still need this for training_0 agent performance tracking
        playerSupervisor = FindObjectOfType<PlayerSupervisor>();
            
        sceneLoader = FindObjectOfType<SceneLoader>();
        uiManager = FindObjectOfType<UIManager>();
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime = DateTime.Now - startTime;
        uiManager.UpdateElapsedTime(elapsedTime.ToString(@"mm\:ss\:ff"));
    }
    
    public void StartGame()
    {   
        if (!playerSupervisor)
        {
            Debug.LogError("No player supervisor found to manage game");
            return;
        }
        
        StartGame(playerSupervisor);
    }

    public void StartGame(PlayerSupervisor supervisor)
    {
        supervisor.StartGame();   
    }

    public void WinGame(PlayerSupervisor supervisor)
    {
        if (trainingMode)
        {
            RestartGame(supervisor);
        }
        if (!trainingMode)
        {
            AudioManager.Instance.PlaySoundBetweenScenes(winSound);
            supervisor.PauseGame();
            sceneLoader.LoadSceneDelayed(SceneLoader.SceneNames.EndScreen);
        } 
    }

    public void LoseGame(PlayerSupervisor supervisor)
    {
        if (trainingMode)
        {
            RestartGame(supervisor);
            return;
        }
        switch(gameMode)
        {
            case GameMode.UntilWin:
                AudioManager.Instance.PlaySoundBetweenScenes(loseSound);
                RestartGame(supervisor);
                break;
            case GameMode.UntilLose:
                AudioManager.Instance.PlaySoundBetweenScenes(loseSound);
                supervisor.PauseGame();
                sceneLoader.LoadSceneDelayed(SceneLoader.SceneNames.EndScreen);
                break;
        }
    }

    public void RestartGame(PlayerSupervisor supervisor)
    {
        // Reset any game state then let the player start again
        startTime = DateTime.Now;
        supervisor.ResetState();
    }

    public void UpdatePoints(int points, PlayerSupervisor supervisor)
    {
        uiManager.UpdatePoints(points, supervisor.PlayerNumber);
    }
}
