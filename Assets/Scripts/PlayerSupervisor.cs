using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSupervisor : MonoBehaviour
{
    [SerializeField] Ball ball;
    [SerializeField] GameManager gameManager;
    [SerializeField] int activeBlocks;
    private Rigidbody2D ballRB;

    // Increase the game speed every x # of seconds defined here
    [Range(0.1f,15)]
    [SerializeField] float ballSpeedIncreaseIncrementInSeconds = 5;

    // Percentage the game speed increases after each increment defined in ballSpeedIncreaseIncrementInSeconds
    [Range(0,15)]
    [SerializeField] int ballSpeedIncreasePctEachIncrement = 1;

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
        ballRB = ball.GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update()
    {
        if(ball)  // if ball object has not been destroyed yet
        {
            IncreaseBallSpeed();
        }
        
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

    public bool isNextSpeedIncreaseIncrement()
    {
        return (Time.time - previousIncreaseTimeInSeconds) > ballSpeedIncreaseIncrementInSeconds;
    }

    public void setPreviousIncreaseTime()
    {
        previousIncreaseTimeInSeconds = Time.time;
    }

    public void setNewBallVelocity()
    {
        ballRB.velocity *= (1 + (float)ballSpeedIncreasePctEachIncrement/100);
    }

    public void IncreaseBallSpeed()
    {
        if(isNextSpeedIncreaseIncrement())
        {
            setPreviousIncreaseTime();
            setNewBallVelocity();
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
