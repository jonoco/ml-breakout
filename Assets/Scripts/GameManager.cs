using System;
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
    [SerializeField] private GameEndCondition gameEndCondition = GameEndCondition.AllPlayersLoseBall;

    public enum GameEndCondition
    {
        AllPlayersLoseBall,
        OnePlayerLosesBall,
        OnePlayerClearsAllBlocks
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

        // Clear the GameData of past games' player data
        // and create a new list to store this game's player data.
        gameData.PlayerList = new List<PlayerData>();

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
            // Set the player who broke all the blocks as the winner.
            SetWinner(supervisor);
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
        // If there are multiple players, check whether the conditions
        // have been met to end the game.
        if (playerSupervisors.Length > 1)
        {
            switch (gameEndCondition)
            {
                case GameEndCondition.OnePlayerClearsAllBlocks:
                    // This currently uses the same scoreboard as the
                    // other play types. It should probably have a separate
                    // one that evalutes users on how quickly they break all
                    // the blocks instead of how many blocks they break.
                    supervisor.ResetBall();
                    break;
                case GameEndCondition.AllPlayersLoseBall:
                    // If all players have lost their ball, transition to the End Screen.
                    if (GameObject.FindObjectsOfType<Ball>().Length == 0)
                    {
                        SetWinnerToHighestPointEarner();
                        TransitionToEndScreen();
                    }
                    break;
                case GameEndCondition.OnePlayerLosesBall:
                    // Should the winner in this case be the one who broke the most blocks?
                    // Or the one who kept the ball in play longer?
                    SetWinnerToHighestPointEarner();
                    TransitionToEndScreen();
                    break;
            }
        }
        else
        {
            gameData.gameResult = $"Game Over!";
            TransitionToEndScreen();
        }
    }

    private void TransitionToEndScreen()
    {
        foreach (PlayerSupervisor ps in playerSupervisors)
        {
            ps.PauseGame();
        }
        sceneLoader.LoadSceneDelayed(SceneLoader.SceneNames.EndScreen);
    }

    private void SetWinnerToHighestPointEarner()
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
            if (winner.GetPlayerType() == PlayerType.Human)
            {
                gameData.gameResult = $"You Win!";
            }
            else
            {
                gameData.gameResult = $"{winner.GetName()} Wins!";
            }
        }
    }

    private void SetWinner(PlayerSupervisor winner)
    {
        if (winner.GetPlayerType() == PlayerType.Human)
        {
            gameData.gameResult = $"You Win!";
        }
        else
        {
            gameData.gameResult = $"{winner.GetName()} Wins!";
        }
    }

    public void RestartGame(PlayerSupervisor supervisor)
    {
        // Reset any game state then let the player start again
        startTime = DateTime.Now;
        supervisor.ResetState();
    }

}
