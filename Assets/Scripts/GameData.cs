using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerType
{
    Human,
    AI
}

[CreateAssetMenu(fileName = "Game Data", menuName = "Game Data")]
public class GameData : ScriptableObject
{
    [SerializeField] public List<PlayerData> PlayerList = new List<PlayerData>();
    [SerializeField] public string gameResult;

    [System.Serializable]
    public class PlayerData
    {
        public string playerName;
        public int Points;
        public PlayerType Type;

        public void SetPoints(int points)
        {
            Points = points;
        }
    }

}

