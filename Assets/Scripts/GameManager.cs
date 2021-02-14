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

    public bool trainingMode = false;
    
    public bool trackingPerformanceTF = false;

    [Range(1, 100000)]
    public int trackingNumberOfGames = 1;

    DateTime startTime = DateTime.Now;
    public TimeSpan elapsedTime;
    
    // Start is called before the first frame update
    void Awake()
    {
        if (trainingMode)
        {
            playerSupervisors = FindObjectsOfType<PlayerSupervisor>();
            // still need this for single mode training/perf tracking.
            playerSupervisor = FindObjectOfType<PlayerSupervisor>();
        } else
        {
            playerSupervisor = FindObjectOfType<PlayerSupervisor>();
        }
            
        sceneLoader = FindObjectOfType<SceneLoader>();
        uiManager = FindObjectOfType<UIManager>();
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime = DateTime.Now - startTime;
        uiManager.UpdateElapsedTime(elapsedTime.ToString(@"mm\:ss\:ff"));
        
        if(trackingPerformanceTF)
            PerformanceCheckNumGames();
    }

    private void PerformanceCheckNumGames()
    {
        // If played up to number of performance games,
        // end game play in editor window.
        if(playerSupervisor.GetNumGamesPlayed() >= trackingNumberOfGames)
        {
            #if UNITY_EDITOR
            if(EditorApplication.isPlaying) 
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
            #endif
        }
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

    public void WinGame()
    {
        if (!playerSupervisor)
        {
            Debug.LogError("No player supervisor found to manage game");
            return;
        }

        WinGame(playerSupervisor);
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

    public void LoseGame()
    {
        if (!playerSupervisor)
        {
            Debug.LogError("No player supervisor found to manage game");
            return;
        }
        
        LoseGame(playerSupervisor);
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

    public void RestartGame()
    {
        if (!playerSupervisor)
        {
            Debug.LogError("No player supervisor found to manage game");
            return;
        }

        RestartGame(playerSupervisor);
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
