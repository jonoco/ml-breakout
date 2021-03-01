using System.Collections.Generic;
using UnityEngine;

public enum PlayerType
{
    Human,
    AI
}

public enum GameEndCondition
{
    AllPlayersLoseBall,
    OnePlayerLosesBall,
    OnePlayerClearsAllBlocks
}

[CreateAssetMenu(fileName = "Game Data", menuName = "Game Data")]
public class GameData : ScriptableObject
{
    [SerializeField] public GameEndCondition gameEndCondition;
    [SerializeField] public List<PlayerData> PlayerList = new List<PlayerData>();
    [SerializeField] public string gameResult;

}

