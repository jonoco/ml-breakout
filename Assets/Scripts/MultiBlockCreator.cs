using System.Collections;
using System.Collections.Generic;
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

    float screenWidthWorld = 16f;
    float screenHeightWorld = 12f;
    public int numPossibleBlocks = 0;
    
    public List<int> availableBlocksIndexList = new List<int>();
    public List<int> chosenBlocksIndexList = new List<int>();

    public List<float> randomBlockXPos = new List<float>();
    public List<float> randomBlockYPos = new List<float>();

    [Range(1, 21)]
    public int numStaticBlocks = 21;

    // these min/max X/Y coords will be converted to float in PlayerSupervisor.
    // screen is 16 (x) by 12 (y) , or 4x3.

    [Range(2, 11)]
    [SerializeField] public int minBlockYPosition = 2;

    [Range(2, 11)]
    [SerializeField] public int maxBlockYPosition = 11;

    [Range(0, 15)]
    [SerializeField] public int minBlockXPosition = 0;

    [Range(0, 15)]
    [SerializeField] public int maxBlockXPosition = 15;

    [Header("THESE SETTINGS ARE NOT FUNCTIONAL YET")]

    [SerializeField] public bool randomBlockLengthTF; 
    
    [Range(1, 15)]
    [SerializeField] public int maxBlockLength = 1; 

    [Range(1, 11)]
    [SerializeField] public int minBlockLength; 

    [SerializeField] bool randomBlockHeightTF; 
    
    [Range(1, 15)]
    [SerializeField] public int maxBlockHeight = 1; 

    [Range(1, 11)]
    [SerializeField] public int minBlockHeight; 

    Random rnd = new Random();

    public void FillRandomLists()
    {
        for(int x = 0; x < screenWidthWorld; x++)
        {
            // starting y at 2 so we don't interfere w/
            // paddle and ball positions across bottom of screen
            for(int y = 2; y < screenHeightWorld; y++)
            {
                randomBlockXPos.Add(x+0.5f);
                randomBlockYPos.Add(y+0.5f);
                numPossibleBlocks += 1;
                availableBlocksIndexList.Add(numPossibleBlocks-1);
            }
        }
    }

    public Vector2 GetRandomBlockVector()
    {
        int randBlockIndex = GetRandomBlockIndex();
        AddIndexToChosenBlockIndexList(randBlockIndex);
        RemoveIndexFromList(randBlockIndex);    

        return new Vector2(randomBlockXPos[randBlockIndex], 
                           randomBlockYPos[randBlockIndex]);
    }

    public void AddIndexToChosenBlockIndexList(int newBlockIndex)
    {
        chosenBlocksIndexList.Add(newBlockIndex);
    }

    public void RemoveIndexFromList(int index)
    {
        availableBlocksIndexList.RemoveAt(index);
    }

    public int GetRandomBlockIndex()
    {
        int randIndex = Random.Range(0, availableBlocksIndexList.Count);
        return randIndex;        
    }

    public void EmptyRandomLists()
    {   
        chosenBlocksIndexList.Clear();
        availableBlocksIndexList.Clear();
    }


}
