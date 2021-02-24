using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Preparation,   // environment is being prepared
    Waiting,       // environment is ready, waiting for player to begin
    Ready,         // player signaled ready to play
    Playing,       // player is actively playing
}

public class PlayerSupervisor : MonoBehaviour
{
    [SerializeField] Ball ball;
    [SerializeField] Paddle paddle;
    [SerializeField] GameManager gameManager;
    [SerializeField] PlayerAgent playerAgent;
    [SerializeField] PlayerData playerData;
    [SerializeField] GameObject trainingBlocks;

    // Player Data Performance Tracking
    [SerializeField] PerformanceDataManager dataManager;

    // Using this as a band-aid for now, until i get multi-agent
    // performance implemented
    [SerializeField] bool isMultiTraining = false;

    // Frannie's Level Items
    public RandomBlockCreator randomBlockCreator;

    private int points = 0;
    private Vector3 ballOffset;         // Starting position of ball
    private Vector3 paddleOffset;       // Starting position of paddle
    private int boundaryHits = 0;

    private int paddleHits = 0;

    private GameObject trainingBlocksInstance;
    private int activeBlocks;
    private PlayerState playerState = PlayerState.Preparation;
    private float playStartTime;

    [Header("Game Environment Settings")]

    [Tooltip("Use the random block creator instead of scene or training blocks")]
    public bool useRandomBlocks = false;
    public float minPaddlePosX = 1f;
    public float maxPaddlePosX = 15f;
    public float instanceWidth = 16f;
    public float instanceHeight = 12f;
    [HideInInspector] public float instanceDiagonalSize = 0;
   
    [Range(10f, 200f)]
    public float paddleMoveSpeed = 100f;

    [Range(.1f, 10f)]
    public float moveStep = 2f;

    public int boundaryReboundLimit = 10;

    [Tooltip("How often to check for anomalies (0 eliminates check)")]
    [Range(0f, 2f)]
    public float detectionFreq = 1f;

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
        if (!randomBlockCreator)
            randomBlockCreator = FindObjectOfType<RandomBlockCreator>();
        
        if (!ball)
            ball = FindObjectOfType<Ball>();
        ballOffset = ball.transform.localPosition;

        // Performance tracking - has to come before countblocks
        if(!isMultiTraining)
            dataManager = FindObjectOfType<PerformanceDataManager>();
        
        if (!paddle)
            paddle = FindObjectOfType<Paddle>();
        paddleOffset = paddle.transform.localPosition;

        // Check if scene is ready for training
        if (gameManager.trainingMode)
            ResetState();
        else
        {
            if (useRandomBlocks && randomBlockCreator)
                randomBlockCreator.setupBlocks();

            CountBlocks();
        }

        // Calculate diagonal width
        instanceDiagonalSize = Mathf.Sqrt(Mathf.Pow(instanceHeight, 2) + Mathf.Pow(instanceWidth, 2)); 
    }

    /// <summary>
    /// Counts all Block objects inside the supervisor's tree.
    /// </summary>
    private void CountBlocks()
    {
        activeBlocks = 0;

        CountChildBlocks(transform);
    }

    /// <summary>
    /// Recursive Block counter.
    /// </summary>
    /// <param name="transform"></param>
    private void CountChildBlocks(Transform transform)
    {
        foreach(Transform child in transform)
        {
            if (child.gameObject.GetComponent<Block>() && child.gameObject.activeSelf)
                ++activeBlocks;
            
            if (child.childCount > 0)
                CountChildBlocks(child);
        }
    }

    public void PlayerReady()
    {
        if (playerState == PlayerState.Waiting)
        {
            playerState = PlayerState.Ready;
            gameManager.StartGame(this);
        }
    }

    public void StartGame()
    {
        // Only start if the player is ready
        if (playerState != PlayerState.Ready)
            return;
        
        if (gameManager.trainingMode && !trainingBlocks)
            Debug.LogError("trainingBlocks reference missing");

        if(!isMultiTraining && dataManager.trackingPerformanceTF)
            dataManager.SetStartingNumBlocks(activeBlocks);

        playerState = PlayerState.Playing;
        playStartTime = Time.time;

        LaunchBall();

        if (detectionFreq > 0)
            StartCoroutine(DetectBallAnomaly());
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
        if (playerAgent)
            playerAgent.LoseBall();

        LoseGame();
    }


    public bool IsMultiAgent()
    {
        return isMultiTraining;
    }

    public void UpdatePlayerPerformanceData(bool gameWinTF)
    {
        // in our current code logic, this if stmt needs to happen
        // BEFORE game manager calls lose game below b/c GM has an if(Trainingmode)
        // in the losegame method and training mode is technically all of the time.
        // and if trainingmode is true, resetstate is called
        // which resets the paddleHits prematurely for this data set.
        dataManager.EndOfGameDataUpdate( 
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
        playerState = PlayerState.Preparation;

        if(!isMultiTraining && dataManager.trackingPerformanceTF)
            UpdatePlayerPerformanceData(false);
        
        playerData.gameResult = "Game Over!";
        ball.gameObject.SetActive(false);
        
        if (playerAgent)
        {
            playerAgent.TotalPlayTime(Time.time - playStartTime);
            playerAgent.LoseGame();
        }

        gameManager.LoseGame(this);

        StopAllCoroutines();
    }

    public void WinGame()
    {
        playerState = PlayerState.Preparation;
        
        if(!isMultiTraining && dataManager.trackingPerformanceTF)
            UpdatePlayerPerformanceData(true);

        playerData.gameResult = "You Win!";
        ball.gameObject.SetActive(false);

         if (playerAgent)
         {
            playerAgent.TotalPlayTime(Time.time - playStartTime);
            playerAgent.WinGame();
         }

        gameManager.WinGame(this);

        StopAllCoroutines();
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

        ball.transform.localPosition = ballOffset;        
        paddle.transform.localPosition = paddleOffset;
        paddle.smoothMovementChange = 0f;
        
        if (useRandomBlocks && randomBlockCreator)
        {
            randomBlockCreator.setupBlocks();
            SetBlockSupervisor(randomBlockCreator.transform);
        }
        else
        {
            RespawnTrainingBlocks();
            SetBlockSupervisor(trainingBlocksInstance.transform);
        }
        
        CountBlocks();

        // Immediately begin the timeout
        if (gameManager.trainingMode && playerAgent.timeLimit > 0)
            StartCoroutine(Timeout());

        playerState = PlayerState.Waiting;
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

            if (originalMag < 0.5f)
            {
                Debug.Log("Ball halt check");

                ResetBall();
                return;
            }

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

    void ResetBall()
    {
        ball.ResetBall();
        ball.transform.localPosition = ballOffset;
        ball.LaunchBall();
    }

    private IEnumerator DetectBallAnomaly()
    {
        while (true)
        {
            // Re-launch a stuck ball
            if (ball.GetComponent<Rigidbody2D>().velocity.magnitude == 0)
            {
                Debug.Log("Ball lockup check");

                ResetBall();
            }

            if (ball.transform.localPosition.x > 50f || 
                ball.transform.localPosition.x < -50f ||
                ball.transform.localPosition.y > 50f ||
                ball.transform.localPosition.y < -50f)
            {
                Debug.Log("Ball bounds check");

                ResetBall();
            }
                
            yield return new WaitForSeconds(detectionFreq);
        }
    }

    private IEnumerator Timeout()
    {   
        yield return new WaitForSeconds(playerAgent.timeLimit);
        
        Debug.Log("Timeout check");

        playerAgent.Timeout();
        LoseGame();
    }

    private void RespawnTrainingBlocks()
    {
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
    }

    private void SetBlockSupervisor(Transform transform)
    {
        foreach(Transform child in transform)
        {
            Block block = child.gameObject.GetComponent<Block>();
            if (block)
                block.playerSupervisor = this;

            if (child.childCount > 0)
                SetBlockSupervisor(child);
        } 
    }
}

