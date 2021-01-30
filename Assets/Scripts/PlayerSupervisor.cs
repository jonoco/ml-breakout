using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSupervisor : MonoBehaviour
{
    [SerializeField] Ball ball;
    [SerializeField] Paddle paddle;
    [SerializeField] GameManager gameManager;
    [SerializeField] PlayerAgent playerAgent;
    [SerializeField] int activeBlocks;
    [SerializeField] GameObject trainingBlocks;
    
    // Frannie's Level Items
    private RandomBlockCreator randomBlockCreator;
    
    private int points = 0;

    private Vector3 ballOffset;         // Starting position of ball
    private Vector3 paddleOffset;       // Starting position of paddle

    private int boundaryHits = 0;

    public int boundaryReboundLimit = 10;
    [SerializeField] Vector2 reboundForceMin = new Vector2(-.5f, -.5f);

    [SerializeField] Vector2 reboundForceMax = new Vector2(.5f, .5f);

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        playerAgent = FindObjectOfType<PlayerAgent>();

        // the code will check whether or not to execute
        // based on the block.name assigned in the Inspector Window
        // in the RandomBlockCreator empty child object
        randomBlockCreator = FindObjectOfType<RandomBlockCreator>();
        randomBlockCreator.setupBlocks();
        
        ball = FindObjectOfType<Ball>();
        ballOffset = ball.transform.position;

        paddle = FindObjectOfType<Paddle>();
        paddleOffset = paddle.transform.position;

        CountBlocks();

        // Check if scene is ready for training
        if (gameManager.trainingMode)
            ResetState();
    }

    void CountBlocks()
    {
        activeBlocks = FindObjectsOfType<Block>().Length;
    }

    public void PlayerReady()
    {
        gameManager.StartGame();
    }

    public void StartGame()
    {
        if (gameManager.trainingMode)
        {
            if (!trainingBlocks)
            {
                Debug.LogError("trainingBlocks reference missing");
                return;
            }
            else
            {
                LaunchBall();
            }     
        }
        else
        {
            LaunchBall();
        }
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
        ball.gameObject.SetActive(false);

        gameManager.LoseGame();
        
        if (playerAgent)
            playerAgent.LoseGame();
    }

    public void BlockDestroyed(int pointValue)
    {
        boundaryHits = 0;

        if (playerAgent)
            playerAgent.BlockHit();

        points += pointValue;
        gameManager.UpdatePoints(points);

        --activeBlocks;
        if (activeBlocks <= 0)
        {
            gameManager.WinGame();

            if (playerAgent)
                playerAgent.WinGame();
        }
    }

    /// <summary>
    /// Game environment reset for training.  
    /// </summary>
    public void ResetState()
    {
        boundaryHits = 0;
        paddle.transform.position = paddleOffset;
        ball.transform.position = ballOffset;
        ball.gameObject.SetActive(true);
        
        GameObject tb = GameObject.FindGameObjectWithTag("TrainingBlock");
        if (tb)
            Destroy(tb);

        foreach(Block block in FindObjectsOfType<Block>())
        {
            Destroy(block.gameObject);
        }

        Instantiate(trainingBlocks);
        CountBlocks();
    }

    public void BoundaryHit()
    {
        ++boundaryHits;
        if (boundaryHits >= boundaryReboundLimit)
        {
            Debug.Log("Ball rebound check");

            float randForceX = UnityEngine.Random.Range(reboundForceMin.x, reboundForceMin.y);
            float randForceY = UnityEngine.Random.Range(reboundForceMax.x, reboundForceMax.y);
            Vector2 force = new Vector2(randForceX, randForceY);
            ball.GetComponent<Rigidbody2D>().AddForce(force);
        }
    }

    public void PaddleHit()
    {
        boundaryHits = 0;
    }
}

