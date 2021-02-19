using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiBlockCreator : MonoBehaviour
{

    // ---------------------------------------------------------

    private List<float> staticBlockXPos = new List<float>()
    {
        2f, 4f, 6f, 8f, 10f, 12f, 14f,
        2f, 4f, 6f, 8f, 10f, 12f, 14f,
        2f, 4f, 6f, 8f, 10f, 12f, 14f,
    };
    
    private List<float> staticBlockYPos = new List<float>()
    {
        5.5f, 5.5f, 5.5f, 5.5f, 5.5f, 5.5f, 5.5f,
        7.5f, 7.5f, 7.5f, 7.5f, 7.5f, 7.5f, 7.5f,
        9.5f, 9.5f, 9.5f, 9.5f, 9.5f, 9.5f, 9.5f,
    };

    private int numStaticBlocks = 21;

    [Header("Training Block Settings")]

    [SerializeField] bool blocksPlacedRandomlyTF;
    
    [SerializeField] bool numBlocksChosenRandomlyTF; 
    
    [Range(1, 100)]
    [SerializeField] int numBlocksChoiceIfNotRandom = 20;

    public int minLeftBorderForBlockPlacement;
    public int maxRightBorderForBlockPlacement;
    public int minBottomBorderForBlockPlacement;
    public int maxTopBorderForBlockPlacement;

    [SerializeField] bool randomBlockLengthTF; 
    
    [Range(1, 15)]
    [SerializeField] int maxBlockLength = 20; 

    [Range(1, 11)]
    [SerializeField] int minBlockLength; 

    [SerializeField] bool randomBlockHeightTF; 
    
    [Range(1, 15)]
    [SerializeField] int maxBlockHeight = 20; 

    [Range(1, 11)]
    [SerializeField] int minBlockHeight; 

    [SerializeField] GameObject blockGameObject;
    [SerializeField] GameObject trainingBlocksGroup;

    // ---------------------------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
