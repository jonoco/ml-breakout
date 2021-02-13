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
    [SerializeField] PlayerData playerData;
    [SerializeField] GameObject trainingBlocks;

    // Player Data Performance Tracking
    private PerformanceDataManager dataManager;

    // Frannie's Level Items
    private RandomBlockCreator randomBlockCreator;
    private int points = 0;

    private Vector3 ballOffset;         // Starting position of ball
    private Vector3 paddleOffset;       // Starting position of paddle

    private int boundaryHits = 0;
    private int paddleHits = 0;

    // Public fields
    public int boundaryReboundLimit = 10;

    [Tooltip("How often to check for anomalies (0 eliminates check)")]
    [Range(0f, 2f)]
    public float detectionFreq = 1f;

    [Tooltip("Limit to wait before ending training (0 eliminates timeout)")]
    [Range(0f, 600f)]
    public float timeLimit = 0f;

    [Tooltip("Angle that ball will ricochet off ceiling to prevent juggling")]
    [Range(0f, 5f)]
    public float ceilingReboundAngle = 0f;

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

        // Performance tracking - has to come before countblocks
        dataManager = FindObjectOfType<PerformanceDataManager>();

        CountBlocks();

        // Check if scene is ready for training
        if (gameManager.trainingMode)
            ResetState();
    }

    void CountBlocks()
    {
        activeBlocks = 0;
        foreach (Block block in FindObjectsOfType<Block>())
        {
            if (block.gameObject.activeSelf)
                ++activeBlocks;
        }
        dataManager.SetStartingNumBlocks(activeBlocks);
    }

    public void PlayerReady()
    {
        gameManager.StartGame();
    }

    public void StartGame()
    {
        if (gameManager.trainingMode && !trainingBlocks)
        {
            Debug.LogError("trainingBlocks reference missing");
            return;
        }

        dataManager.SetStartingNumBlocks(activeBlocks);

        LaunchBall();

        if (detectionFreq > 0)
            StartCoroutine(DetectBallLockup());

        if (gameManager.trainingMode && timeLimit > 0)
            StartCoroutine(Timeout());
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
        LoseGame();
    }

    public int GetNumGamesPlayed()
    {
        return dataManager.GetNumGamesPlayed();
    }

    public void UpdatePlayerPerformanceData(bool gameWinTF)
    {
        // in our current code logic, this if stmt needs to happen
        // BEFORE game manager calls lose game below b/c GM has an if(Trainingmode)
        // in the losegame method and training mode is technically all of the time.
        // and if trainingmode is true, resetstate is called
        // which resets the paddleHits prematurely for this data set.
        dataManager.EndOfGameDataUpdate(
                gameManager.trackingPerformanceTF, 
                gameWinTF,
                // multiplying by timeScale here as ccan increase this in Project Settings
                // in the editor for faster performance runs
                (int)(gameManager.elapsedTime.TotalSeconds * Time.timeScale),
                (int)(gameManager.elapsedTime.TotalMilliseconds * Time.timeScale),
                paddleHits,
                activeBlocks);
    }

    public void LoseGame()
    {
        UpdatePlayerPerformanceData(false);

        playerData.gameResult = "Game Over!";
        ball.gameObject.SetActive(false);
        gameManager.LoseGame();

        StopAllCoroutines();
    
        if (playerAgent)
            playerAgent.LoseGame();
    }

    public void WinGame()
    {
        playerData.gameResult = "You Win!";
        gameManager.WinGame();

        UpdatePlayerPerformanceData(true);
        Debug.Log("WIN!");

        if (playerAgent)
            playerAgent.WinGame();
    }

    public void BlockDestroyed(int pointValue)
    {
        boundaryHits = 0;

        if (playerAgent)
            playerAgent.BlockHit();

        points += pointValue;
        gameManager.UpdatePoints(points);
        playerData.points = points;

        --activeBlocks;
        if (activeBlocks <= 0)
        {
            WinGame();
        }
    }

    public int GetPoints()
    {
        return points;
    }

    /// <summary>
    /// Game environment reset for training.  
    /// </summary>
    public void ResetState()
    {
        boundaryHits = 0;
        paddleHits = 0;
        points = 0;
        gameManager.UpdatePoints(points);

        ball.gameObject.SetActive(true);
        ball.ResetBall();
        ball.transform.position = ballOffset;

        dataManager.ResetValues();

        paddle.transform.position = paddleOffset;
        
        GameObject tb = GameObject.FindGameObjectWithTag("TrainingBlock");
        if (tb)
            Destroy(tb);

        foreach(Block block in FindObjectsOfType<Block>())
        {
            block.gameObject.SetActive(false);
            Destroy(block.gameObject);
        }

        Instantiate(trainingBlocks);
        CountBlocks();
    }

    public void BoundaryHit(BoundaryName boundaryName)
    {
        Rigidbody2D ballRB = ball.GetComponent<Rigidbody2D>();

        ++boundaryHits;
        if (boundaryHits >= boundaryReboundLimit)
        {
            Debug.Log("Ball rebound check");

            // Rebound 45 degrees up and away from wall
            Vector2 newVelocity = Vector2.one;
            newVelocity *= ballRB.velocity.magnitude / newVelocity.magnitude;
            newVelocity.x *= ballRB.velocity.x > 0 ? 1 : -1;
            ballRB.velocity = newVelocity;
            
            boundaryHits = 0;
        }
        else if (boundaryName == BoundaryName.Ceiling && ballRB.velocity.x == 0 && ceilingReboundAngle > 0f)
        {
            Debug.Log("Ceiling rebound check");

            // Rebound ceilingReboundAngle degrees off the ceiling
            float originalMag = ballRB.velocity.magnitude;
            Vector2 newVelocity = ballRB.velocity;
            newVelocity.x = ballRB.velocity.y * Mathf.Tan(ceilingReboundAngle);
            newVelocity *= originalMag / newVelocity.magnitude;
            newVelocity.x *= UnityEngine.Random.Range(0,2) == 0 ? 1 : -1;
            ballRB.velocity = newVelocity;
        }
    }

    public void PaddleHit()
    {
        paddleHits++;
        boundaryHits = 0;
        playerAgent.PaddleHit();
    }



    private IEnumerator DetectBallLockup()
    {
        while (true)
        {
            // Re-launch a stuck ball
            if (ball.GetComponent<Rigidbody2D>().velocity.magnitude == 0)
            {
                Debug.Log("Ball lockup check");

                ball.ResetBall();
                ball.transform.position = ballOffset;
                ball.LaunchBall();
            }
                
            yield return new WaitForSeconds(detectionFreq);
        }
    }

    private IEnumerator Timeout()
    {   
        yield return new WaitForSeconds(timeLimit);
        
        Debug.Log("Timeout check");

        playerAgent.Timeout();
        LoseGame();
    }
}

