using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Data", menuName = "Player Data")]
public class PlayerData : ScriptableObject
{
    public int points;
    public string gameResult;
    
    public bool isHumanPlayer; // no functionality for this yet.

  // NOTE: All of these values RELY on the fact that this object
    // is NEVER reset until one stops Unity with the play button.

    [Tooltip("Number of games played in a single performance-tracking session")]
    public int numGames;  

    [Tooltip("List of all game scores, by individual game, len should match numGames")]
    public List<int> gameScoresList;

    [Tooltip("List of number of blocks broken, by individual game, len should match numGames")]
    public List<int> blocksBrokenList;

    [Tooltip("true=win, false=lose', by individual game, len should match numGames")]
    public List<bool> gameWinStatusList; 

    [Tooltip("Game length in seconds, by individual game, len should match numGames, will be rounded to 2 decimal places")]
    public List<double> gameTimePlayedList;    

    void Awake()
    {
        gameScoresList = new List<int>();
        blocksBrokenList = new List<int>();
        gameWinStatusList = new List<bool>();
        gameTimePlayedList = new List<double>();
    }

}

