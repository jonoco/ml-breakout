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
        playerSupervisor = FindObjectOfType<PlayerSupervisor>();
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
        playerSupervisor.StartGame();
    }

    public void WinGame()
    {
        if (!trainingMode)
        {
            AudioManager.Instance.PlaySoundBetweenScenes(winSound);
            playerSupervisor.PauseGame();
            sceneLoader.LoadSceneDelayed(SceneLoader.SceneNames.EndScreen);
        } 
    }

    public void LoseGame()
    {
        if (trainingMode)
        {
            RestartGame();
        }
        else
        {
            AudioManager.Instance.PlaySoundBetweenScenes(loseSound);
            playerSupervisor.PauseGame();
            sceneLoader.LoadSceneDelayed(SceneLoader.SceneNames.EndScreen);
        }
    }

    public void UpdatePoints(int points)
    {
        uiManager.UpdatePoints(points);
    }

    public void RestartGame()
    {
        // Reset any game state then let the player start again
        startTime = DateTime.Now;
        playerSupervisor.ResetState();
    }
}
