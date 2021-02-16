using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GameManager : MonoBehaviour
{
    [SerializeField] SceneLoader sceneLoader;
    [SerializeField] UIManager uiManager;
    [SerializeField] PlayerSupervisor playerSupervisor;
    [SerializeField] PlayerSupervisor[] playerSupervisors;
    [SerializeField] AudioClip loseSound;
    [SerializeField] AudioClip winSound;

    public bool trainingMode = false;

    DateTime startTime = DateTime.Now;
    
    // Start is called before the first frame update
    void Awake()
    {
        if (trainingMode)
            playerSupervisors = FindObjectsOfType<PlayerSupervisor>();
        else
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
        }
        else
        {
            AudioManager.Instance.PlaySoundBetweenScenes(loseSound);
            supervisor.PauseGame();
            sceneLoader.LoadSceneDelayed(SceneLoader.SceneNames.EndScreen);
        }
    }

    public void RestartGame(PlayerSupervisor supervisor)
    {
        // Reset any game state then let the player start again
        startTime = DateTime.Now;
        supervisor.ResetState();
    }

    public void UpdatePoints(int points)
    {
        uiManager.UpdatePoints(points);
    }
}
