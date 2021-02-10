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
    private int startingNumBlocks;
    [SerializeField] PlayerData playerData;
    [SerializeField] GameObject trainingBlocks;

    private int numGamesPlayed;  // For agent inference perf tracking
    
    // Frannie's Level Items
    private RandomBlockCreator randomBlockCreator;
    private int points = 0;

    private Vector3 ballOffset;         // Starting position of ball
    private Vector3 paddleOffset;       // Starting position of paddle

    private int boundaryHits = 0;

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

    // --------------------------------------------------
    // Data tracking
    // --------------------------------------------------

    public void UpdatePlayerDataLists(bool winStatus)
    {
        // update at end of game
        playerData.gameScoresList.Add(playerData.points);
        playerData.blocksBrokenList.Add(startingNumBlocks - activeBlocks);
        playerData.gameWinStatusList.Add(winStatus);
        print(playerData.gameScoresList);
    }

    public int GetNumGamesPlayed()
    {
        return numGamesPlayed;
    }

    // Unity Default Function
    // In the Editor, Unity calls this message when playmode is stopped.
    // sources:
    // https://docs.unity3d.com/Manual/ExecutionOrder.html
    // https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationQuit.html
    void OnApplicationQuit()
    {
        WritePlayerDataToTextFile();
    }

    void WritePlayerDataToTextFile()
    {
        Debug.Log("Final");
    }
    // --------------------------------------------------

    public void LoseColliderHit()
    {
        LoseGame();
    }

    public void LoseGame()
    {
        playerData.gameResult = "Game Over!";
        ball.gameObject.SetActive(false);
        gameManager.LoseGame();

        StopAllCoroutines();

        if(gameManager.trackingPerformanceTF)
            UpdatePlayerDataLists(false);
            numGamesPlayed++;
    
        if (playerAgent)
            playerAgent.LoseGame();
    }

    public void WinGame()
    {
        playerData.gameResult = "You Win!";
        gameManager.WinGame();

        if(gameManager.trackingPerformanceTF)
            UpdatePlayerDataLists(true);
            numGamesPlayed++;

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
        points = 0;
        gameManager.UpdatePoints(points);

        ball.gameObject.SetActive(true);
        ball.ResetBall();
        ball.transform.position = ballOffset;
        
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

