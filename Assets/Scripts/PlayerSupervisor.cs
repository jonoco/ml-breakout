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
    
    // Frannie's Level Items
    private RandomBlockCreator randomBlockCreator;
    private int points = 0;

    private Vector3 ballOffset;         // Starting position of ball
    private Vector3 paddleOffset;       // Starting position of paddle

    private int boundaryHits = 0;

    public GameObject trainingBlocksInstance;

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
        
        if (!playerAgent)
            playerAgent = FindObjectOfType<PlayerAgent>();

        // the code will check whether or not to execute
        // based on the block.name assigned in the Inspector Window
        // in the RandomBlockCreator empty child object
        randomBlockCreator = FindObjectOfType<RandomBlockCreator>();
        randomBlockCreator.setupBlocks();
        
        if (!ball)
            ball = FindObjectOfType<Ball>();
        ballOffset = ball.transform.localPosition;

        if (!paddle)
            paddle = FindObjectOfType<Paddle>();
        paddleOffset = paddle.transform.localPosition;

        // Check if scene is ready for training
        if (gameManager.trainingMode)
            ResetState();
        else
            CountBlocks();
    }

    void CountBlocks()
    {
        activeBlocks = 0;

        // TODO need to count only blocks in each player's environment to
        //  work for general multiplayer use; Maybe keep all blocks in a
        //  container for easier tracking
        if (gameManager.trainingMode)
        {
            foreach(Transform child in trainingBlocksInstance.transform)
            {
                if (child.gameObject.GetComponent<Block>() && child.gameObject.activeSelf)
                    ++activeBlocks;
            }
        }
        else
        {
            foreach (Block block in FindObjectsOfType<Block>())
            {
                if (block.gameObject.activeSelf)
                    ++activeBlocks;
            }
        }
    }

    public void PlayerReady()
    {
        gameManager.StartGame(this);
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

    public void LoseColliderHit()
    {
        LoseGame();
    }

    public void LoseGame()
    {
        playerData.gameResult = "Game Over!";
        ball.gameObject.SetActive(false);
        gameManager.LoseGame(this);

        StopAllCoroutines();
        
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
        playerData.points = points;

        --activeBlocks;
        if (activeBlocks <= 0)
        {
            playerData.gameResult = "You Win!";
            gameManager.WinGame(this);

            if (playerAgent)
                playerAgent.WinGame();
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
        ball.transform.localPosition = ballOffset;
        
        paddle.transform.localPosition = paddleOffset;
        
        // Destroy training blocks, then the block holder
        if (trainingBlocksInstance)
        {
            foreach(Transform child in trainingBlocksInstance.transform)
            {
                if (child.gameObject.GetComponent<Block>())
                    child.gameObject.SetActive(false);
                    Destroy(child.gameObject);
            }

            Destroy(trainingBlocksInstance);
        }

        trainingBlocksInstance = Instantiate(trainingBlocks, transform);
        
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

