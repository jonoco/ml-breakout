using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSupervisor : MonoBehaviour
{

    [SerializeField] GameManager gameManager;
    [SerializeField] int activeBlocks;
    
    // Frannie's Level Items
    private RandomBlockCreator randomBlockCreator;

    private int points = 0;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        // the code will check whether or not to execute
        // based on the block.name assigned in the Inspector Window
        // in the RandomBlockCreator empty child object
        randomBlockCreator = FindObjectOfType<RandomBlockCreator>();
        randomBlockCreator.setupBlocks();
    
        activeBlocks = FindObjectsOfType<Block>().Length;
    }

    void Update()
    {

    }

    public void BlockDestroyed(int pointValue)
    {
        points += pointValue;
        gameManager.UpdatePoints(points);

        --activeBlocks;
        if (activeBlocks <= 0)
        {
            gameManager.WinGame();
        }
    }
}
