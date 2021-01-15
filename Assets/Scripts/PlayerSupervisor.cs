using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSupervisor : MonoBehaviour
{

    [SerializeField] GameManager gameManager;
    [SerializeField] int activeBlocks;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        activeBlocks = FindObjectsOfType<Block>().Length;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BlockDestroyed()
    {
        --activeBlocks;
        if (activeBlocks <= 0)
        {
            gameManager.WinGame();
        }
    }
}
