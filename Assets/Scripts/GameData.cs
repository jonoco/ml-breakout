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
        public string Name;
        public int Points;
        public PlayerType playerType;
    }

    internal void UpdatePoints(string name, int points)
    {
        PlayerList.Find(p => p.Name.Equals(name)).Points = points;
    }
}

