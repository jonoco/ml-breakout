﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static GameData;

public class GameManager : MonoBehaviour
{
    [SerializeField] SceneLoader sceneLoader;
    [SerializeField] UIManager uiManager;
    [SerializeField] PlayerSupervisor playerSupervisor;
    [SerializeField] PlayerSupervisor[] playerSupervisors;
    [SerializeField] AudioClip loseSound;
    [SerializeField] AudioClip winSound;
    [SerializeField] GameData gameData;

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

        gameData.PlayerList = new List<PlayerData>();
        foreach (PlayerSupervisor ps in playerSupervisors)
        {
            gameData.PlayerList.Add(new PlayerData() {
                Name = ps.PlayerName,
                Points = ps.GetPoints(),
                playerType = ps.playerType
            });
            Debug.Log($"Adding {ps.PlayerName} to data.");
        }

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
            foreach (PlayerSupervisor ps in playerSupervisors)
            {
                ps.PauseGame();
            }
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

        AudioManager.Instance.PlaySoundBetweenScenes(loseSound);
        switch(gameMode)
        {
            case GameMode.UntilWin:
                // Not yet implemented. It seems like high scores here would be
                // elapsed time instead of points - since playing until win always
                // ends with breaking every block. But the PerformanceDataManager
                // currently relies on elapsed time being reset each time a ball is lost.
                break;
            case GameMode.UntilLose:
                supervisor.LostBall = true;
                // If all players have lost their ball, transition to the End Screen.
                if (GameObject.FindObjectsOfType<Ball>().Length == 0)
                {
                    foreach (PlayerSupervisor ps in playerSupervisors)
                    {
                        ps.PauseGame();
                    }
                    SaveHighestScoreToGameResult();
                    sceneLoader.LoadSceneDelayed(SceneLoader.SceneNames.EndScreen);
                }
                break;
        }
    }

    private void SaveHighestScoreToGameResult()
    {
        // If all of the PlayerSupervisors have equal points, report a tie.
        if (Array.TrueForAll(playerSupervisors, ps => ps.GetPoints() == playerSupervisors[0].GetPoints()))
        {
            gameData.gameResult = $"It's a tie!";
        }
        // Otherwise, report the highest scoring player's name.
        else
        {
            PlayerSupervisor winner = playerSupervisors[0];
            for (int i = 1; i < playerSupervisors.Length; i++)
            {
                if (playerSupervisors[i].GetPoints() > winner.GetPoints())
                {
                    winner = playerSupervisors[i];
                }
            }
            if (winner.playerType == PlayerType.Human)
            {
                gameData.gameResult = $"You Win!";
            }
            else
            {
                gameData.gameResult = $"{winner.PlayerName} Wins!";
            }
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
        gameData.UpdatePoints(supervisor.PlayerName, points);
    }
}
