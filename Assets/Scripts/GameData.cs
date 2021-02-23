using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Data", menuName = "Player Data")]
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

