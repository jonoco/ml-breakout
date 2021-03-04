using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] SceneLoader sceneLoader;
    [SerializeField] PlayerSupervisor playerSupervisor;
    [SerializeField] PlayerSupervisor[] playerSupervisors;
    [SerializeField] AudioClip loseSound;
    [SerializeField] AudioClip winSound;
    [SerializeField] GameData gameData;
    [SerializeField] TextMeshProUGUI timeDisplay;
    public bool trainingMode = false;
    DateTime startTime = DateTime.Now;
    public TimeSpan elapsedTime;
    
    // Start is called before the first frame update
    void Awake()
    {
        playerSupervisors = FindObjectsOfType<PlayerSupervisor>();

        // Still need this for training_0 agent performance tracking
        playerSupervisor = FindObjectOfType<PlayerSupervisor>();

        // Clear the GameData of past games' player data
        // and create a new list to store this game's player data.
        gameData.PlayerList = new List<PlayerData>();

        sceneLoader = FindObjectOfType<SceneLoader>();
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime = DateTime.Now - startTime;
        if (timeDisplay)
        {
            timeDisplay.text = elapsedTime.ToString(@"mm\:ss\:ff");
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

    public void PlayerLostBall(PlayerSupervisor supervisor)
    {
        if (trainingMode)
        {
            supervisor.LoseGame();
            RestartGame(supervisor);
            return;
        }

        // Single player games avoid rules
        if (playerSupervisors.Length < 2)
        {
            gameData.gameResult = $"Game Over!";
            AudioManager.Instance.PlaySoundBetweenScenes(loseSound);
            TransitionToEndScreen();
            return;
        }

        // Multiplayer game rules
        switch (gameData.gameEndCondition)
        {
            case GameEndCondition.OnePlayerClearsAllBlocks:
                // This currently uses the same scoreboard as the
                // other play types. It should probably have a separate
                // one that evalutes users on how quickly they break all
                // the blocks instead of how many blocks they break.
                supervisor.ResetPlayState();
                break;
            case GameEndCondition.AllPlayersLoseBall:
                // If all players have lost their ball, transition to the End Screen.
                int activeBalls = 0;
                foreach (Ball ball in GameObject.FindObjectsOfType<Ball>())
                    if (ball.gameObject.activeSelf)
                        ++activeBalls;
                
                if (activeBalls == 0)
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

    public void PlayerClearedBlocks(PlayerSupervisor supervisor)
    {
        if (trainingMode)
        {
            supervisor.WinGame();
            RestartGame(supervisor);
            return;
        }

        // Single player games avoid rules
        if (playerSupervisors.Length < 2)
        {
            gameData.gameResult = $"Game Over!";
            AudioManager.Instance.PlaySoundBetweenScenes(winSound);
            TransitionToEndScreen();
            return;
        }

        // Multiplayer game rules
        switch (gameData.gameEndCondition)
        {
            case GameEndCondition.OnePlayerClearsAllBlocks:
            case GameEndCondition.AllPlayersLoseBall:
            case GameEndCondition.OnePlayerLosesBall:
            default:
                SetWinner(supervisor);
                TransitionToEndScreen();
                break;
        }
    }

    private void TransitionToEndScreen()
    {
        foreach (PlayerSupervisor ps in playerSupervisors)
        {
            ps.PauseGame();
        }
        if (gameData.PlayerList.Count > 1)
        {
            sceneLoader.LoadSceneDelayed(SceneLoader.SceneNames.End_Screen_Two_Player);
        }
        else
        {
            sceneLoader.LoadSceneDelayed(SceneLoader.SceneNames.EndScreen);
        }
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
        supervisor.ResetEnvironmentState();
    }

}
