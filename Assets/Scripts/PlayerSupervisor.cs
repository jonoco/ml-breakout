using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSupervisor : MonoBehaviour
{
    [SerializeField] Ball ball;
    [SerializeField] GameManager gameManager;
    [SerializeField] int activeBlocks;

    // Used for increasing ball velocity throughout game
    [Range(0.1f, 5f)]
    [SerializeField] float currentGameSpeed = 1f; 

    // Increase the game speed every x # of seconds defined here
    [Range(0.1f,15)]
    [SerializeField] float speedIncreaseIncrementInSeconds = 5;

    // Percentage the game speed increases after each increment defined in speedIncreaseIncrementInSeconds
    [Range(0,15)]
    [SerializeField] int speedIncreasePctEachIncrement = 1;

    // To keep track of when the last speed increase was to determine if a new speed incr. is needed
    private float previousIncreaseTimeInSeconds = 0;

    // Frannie's Level Items
    private RandomBlockCreator randomBlockCreator;
    private int points = 0;

    // storing starting number of blocks for game speed increase throughout game
    private int numStartingBlocks;

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
        ball = FindObjectOfType<Ball>();

        // game speed
        SetGameSpeed();
    }

    // Update is called once per frame
    void Update()
    {
        IncreaseGameSpeed();
    }

    public void StartGame()
    {
        LaunchBall();
    }

    public void PauseGame()
    {
        ball.gameObject.SetActive(false);
    }

    void LaunchBall()
    {
        ball.LaunchBall();
    }

    public void LoseColliderHit()
    {
        Destroy(ball.gameObject);
        
        gameManager.LoseGame();
    }

    public void SetGameSpeed()
    {
        Time.timeScale = currentGameSpeed;
    }

    public void IncreaseGameSpeed()
    {
        if(Time.time - previousIncreaseTimeInSeconds > speedIncreaseIncrementInSeconds)
        {
            previousIncreaseTimeInSeconds = Time.time;
            currentGameSpeed *= (1 + (float)speedIncreasePctEachIncrement/100);
            SetGameSpeed();
        }        
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
