using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    Waiting,
    Ready,
    Playing,
}

public class PlayerSupervisor : MonoBehaviour
{
    [SerializeField] Ball ball;
    [SerializeField] Paddle paddle;
    [SerializeField] GameManager gameManager;
    [SerializeField] PlayerAgent playerAgent;
    [SerializeField] PlayerData playerData;
    [SerializeField] PerformanceDataManager dataManager;
    [SerializeField] MultiBlockCreator multiBlockCreator;

    [SerializeField] GameObject blockGameObjectType;
    [SerializeField] GameObject trainingBlocksGroupType;
    private GameObject trainingBlocksGroup;

    // Using this as a band-aid for now, until i get multi-agent
    // performance implemented
    [SerializeField] bool isMultiTraining = false;

    // Frannie's Level Items
    private RandomBlockCreator randomBlockCreator;
    
    private int points = 0;

    private Vector3 ballOffset;         // Starting position of ball
    private Vector3 paddleOffset;       // Starting position of paddle

    private int boundaryHits = 0;
    private int paddleHits = 0;
    private int activeBlocks;

    private PlayerState playerState = PlayerState.Waiting;

    private List<Transform> blockTransformsList = new List<Transform>();

    [Header("Game Environment Settings")]

    public float minPaddlePosX = 1f;
    public float maxPaddlePosX = 15f;
    public float instanceWidth = 16f;
    public float instanceHeight = 12f;
   
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

        if(!multiBlockCreator)
            multiBlockCreator = FindObjectOfType<MultiBlockCreator>();
        
        if (!playerAgent)
            playerAgent = FindObjectOfType<PlayerAgent>();

        // the code will check whether or not to execute
        // based on the block.name assigned in the Inspector Window
        // in the RandomBlockCreator empty child object
        randomBlockCreator = FindObjectOfType<RandomBlockCreator>();
        if (randomBlockCreator)
            randomBlockCreator.setupBlocks();
        
        if (!ball)
            ball = FindObjectOfType<Ball>();
        ballOffset = ball.transform.localPosition;

        // Performance tracking - has to come before countblocks
        if(!isMultiTraining)
            dataManager = FindObjectOfType<PerformanceDataManager>();
        
        if (!paddle)
            paddle = FindObjectOfType<Paddle>();
        paddleOffset = paddle.transform.localPosition;

        if(!trainingBlocksGroup && gameManager.trainingMode)
            trainingBlocksGroup = GetTrainingBlocksGroupInstance();

        // Check if scene is ready for training
        if (gameManager.trainingMode)
            ResetState();
        else
            CountBlocks();
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
        if (playerState == PlayerState.Waiting)
        {
            playerState = PlayerState.Ready;
            gameManager.StartGame(this);
        }
    }

    public void StartGame()
    {

        if(!isMultiTraining && dataManager.trackingPerformanceTF)
            dataManager.SetStartingNumBlocks(activeBlocks);
        
        // Only start if the player is ready
        if (playerState != PlayerState.Ready)
            return;

        playerState = PlayerState.Playing;

        LaunchBall();

        if (detectionFreq > 0)
            StartCoroutine(DetectBallLockup());

        if (gameManager.trainingMode && playerAgent.timeLimit > 0)
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
        if(!isMultiTraining && dataManager.trackingPerformanceTF)
            UpdatePlayerPerformanceData(false);
        
        playerState = PlayerState.Waiting;

        playerData.gameResult = "Game Over!";
        ball.gameObject.SetActive(false);
        
        if (playerAgent)
            playerAgent.LoseGame();

        gameManager.LoseGame(this);

        StopAllCoroutines();
    }

    public void WinGame()
    {
        playerState = PlayerState.Waiting;
        
        if(!isMultiTraining && dataManager.trackingPerformanceTF)
            UpdatePlayerPerformanceData(true);

        playerData.gameResult = "You Win!";
        ball.gameObject.SetActive(false);

         if (playerAgent)
            playerAgent.WinGame();

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

    /*
    Variables:
    1) # of blocks - either random # or set # you define above
    2) min/max border values
    3) random block lengths? true/false + min/max
    4) random block heights? true/false + min/max
    */

    public GameObject GetTrainingBlocksGroupInstance()
    {
        GameObject trainingBlocksGroup = Instantiate(
            trainingBlocksGroupType, new Vector2(0, 0),
            Quaternion.identity, this.transform.parent.transform); 
        return trainingBlocksGroup;
    }

    public void CreateTrainingBlocks()
    {
        if(multiBlockCreator.blocksPlacedRandomlyTF)
        {
            CreateRandomTrainingBlocks();
        }
        else
        {
            CreateStaticTrainingBlocks();
        }
    }

    public void CreateRandomTrainingBlocks()
    {
        int blockNum = 0;

        if(multiBlockCreator.numBlocksChosenRandomlyTF)
        {
            blockNum = (int)Random.Range(5f, 50f);
        }
        else
        {
            blockNum = multiBlockCreator.numBlocksChoiceIfNotRandom;
        }
                    
        for(int i = 0; i < blockNum; i++)
        {
            GameObject block = Instantiate(
                blockGameObjectType,
                GetNonOverlapping2DVectorPosition(),
                Quaternion.identity,
                trainingBlocksGroup.transform);
            
            if(block == null)
            {
                Debug.Log("null block");
            }

            if(block)
                block.GetComponent<Block>().playerSupervisor = this; 
                blockTransformsList.Add(block.transform); 
        }           
    }

    public Vector2 GetNonOverlapping2DVectorPosition()
    {   
        Vector2 currPos = GetRandom2DVectorPosition();
        while(IsOverlapping(currPos)){
            currPos = GetRandom2DVectorPosition();
        }
        return currPos;
    }

    public bool IsOverlapping(Vector2 newBlockPos)
    {
        foreach(Transform block in blockTransformsList)
        {
            Vector2 existingBlockPos = block.position;
            float distanceBetweenBlocks = Vector2.Distance(newBlockPos, existingBlockPos);
            // overlapping in a world where the blocks are all size 1 means
            // that the blocks have same position.
            // no overlap means the distance is 1 or greater. 
            if(distanceBetweenBlocks < 1){
                return true;
            }
        }
        return false;
    }

    public Vector2 GetRandom2DVectorPosition()
    {
        /* ASSUMPTIONS for now:
        Each block is 1x1 (world size) so their midpoints (i.e. their positions)
        will be on the 0.5 lines. Thus, below, adding 0.5 to the 
        randomly chosen x and y values below, 
        so their ranges are really 0.5 - 15.5 (X) and 2.5 - 11.5 (Y) by increments of 0.5.
        In the Y direction, it starts at 2 as we need to allow room for the paddle
        and ball to move and not hit a ball before launch. Get an error in these cases, 
        so avoiding them altogether.
        This logic will ALL have to be adjusted if we allow random block sizes or
        sizes other than unit 1. This is the easiest way to keep track of their positions
        to loop through to make sure they aren't all overlapping when randomly placed.
        */
        float randX = (int)(Random.Range((float)multiBlockCreator.minBlockXPosition, 
                                         (float)multiBlockCreator.maxBlockXPosition)) +
                        0.5f + 
                        this.transform.parent.transform.position.x;

        float randY = (int)(Random.Range((float)multiBlockCreator.minBlockYPosition,
                                         (float)multiBlockCreator.maxBlockYPosition)) + 
                        0.5f +
                        this.transform.parent.transform.position.y;

        return new Vector2(randX, randY);
    }

    public void CreateStaticTrainingBlocks()
    {
        for(int i = 0; i < multiBlockCreator.numStaticBlocks; i++)
        {
            GameObject block = Instantiate(blockGameObjectType, 
                new Vector2(
                    multiBlockCreator.staticBlockXPos[i] + this.transform.parent.transform.position.x,
                    multiBlockCreator.staticBlockYPos[i] + this.transform.parent.transform.position.y
                ), 
                Quaternion.identity,
                trainingBlocksGroup.transform
            );
            if(block)
                block.GetComponent<Block>().playerSupervisor = this; 
                blockTransformsList.Add(block.transform);          
        }      
    }
    
    public void DestroyTrainingBlocks()
    {
        if(trainingBlocksGroup)
        {
            foreach(Transform child in trainingBlocksGroup.transform)
            {
                if (child.gameObject.GetComponent<Block>())
                    child.gameObject.SetActive(false);
                    Destroy(child.gameObject);
            }
        }
    }

    public void EmptyBlockTransformList()
    {
        if(blockTransformsList.Count > 0)
            blockTransformsList.Clear();
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
    
        DestroyTrainingBlocks();
        EmptyBlockTransformList();
        CreateTrainingBlocks();        
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
        yield return new WaitForSeconds(playerAgent.timeLimit);
        
        Debug.Log("Timeout check");

        playerAgent.Timeout();
        LoseGame();
    }
}

