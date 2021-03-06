﻿using System.Collections.Generic;
using UnityEngine;

public class MultiBlockCreator : MonoBehaviour
{

    [Header("Training Block Settings")]

    [SerializeField] public bool blocksPlacedRandomlyTF;
    
    [SerializeField] public bool numBlocksChosenRandomlyTF; 
    
    [Range(1, 100)]
    [SerializeField] public int numBlocksChoiceIfNotRandom = 20;

    public List<float> staticBlockXPos = new List<float>()
    {
        2f, 4f, 6f, 8f, 10f, 12f, 14f,
        2f, 4f, 6f, 8f, 10f, 12f, 14f,
        2f, 4f, 6f, 8f, 10f, 12f, 14f,
    };
    
    public List<float> staticBlockYPos = new List<float>()
    {
        5.5f, 5.5f, 5.5f, 5.5f, 5.5f, 5.5f, 5.5f,
        7.5f, 7.5f, 7.5f, 7.5f, 7.5f, 7.5f, 7.5f,
        9.5f, 9.5f, 9.5f, 9.5f, 9.5f, 9.5f, 9.5f,
    };

    [Range(1, 21)]
    public int numStaticBlocks = 21;
}
