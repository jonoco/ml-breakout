using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.MLAgents.Policies;
using UnityEngine;

public enum PlayerState
{
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
    [SerializeField] TextMeshProUGUI pointsDisplay;

    // Scriptable Object references
    [SerializeField] private GameData gameData;
    [SerializeField] private PlayerData playerData;

    // Player Data Performance Tracking
    [SerializeField] PerformanceDataManager dataManager;
    [SerializeField] MultiBlockCreator multiBlockCreator;

    [SerializeField] GameObject blockGameObjectType;
    [SerializeField] GameObject trainingBlocksGroupType;
    private GameObject trainingBlocksGroup;

    // Using this as a band-aid for now, until i get multi-agent
    // performance implemented
    [SerializeField] bool isMultiTraining = false;

    // Frannie's Level Items
    [SerializeField] private RandomBlockCreator randomBlockCreator;

    // --- Multi block creator
    
    [Header("Multi Block Creator Settings")]

    float screenWidthWorld = 16f;
    float screenHeightWorld = 12f;
    public int numPossibleBlocks = 0;    
    public List<int> availableBlocksIndexList = new List<int>();
    public List<int> chosenBlocksIndexList = new List<int>();
    public List<float> randomBlockXPos = new List<float>();
    public List<float> randomBlockYPos = new List<float>();

    [Header("Game Object Settings/States/Info")]

    private Vector3 ballOffset;         // Starting position of ball
    private Vector3 paddleOffset;       // Starting position of paddle

    private int boundaryHits = 0;
    private int paddleHits = 0;
    private int activeBlocks;

    private GameObject trainingBlocksInstance;
    private PlayerState playerState = PlayerState.Waiting;
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

        if(!multiBlockCreator)
            multiBlockCreator = FindObjectOfType<MultiBlockCreator>();
        
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

        if(!trainingBlocksGroup && gameManager.trainingMode)
            trainingBlocksGroup = GetTrainingBlocksGroupInstance();

        // Check if scene is ready for training
        if (gameManager.trainingMode)
            ResetEnvironmentState();
        else
        {
            if (useRandomBlocks && randomBlockCreator)
                randomBlockCreator.setupBlocks();
                SetBlockSupervisor(randomBlockCreator.transform);

            CountBlocks();
        }

        // Calculate diagonal width
        instanceDiagonalSize = Mathf.Sqrt(Mathf.Pow(instanceHeight, 2) + Mathf.Pow(instanceWidth, 2));

        // Set the playerData's type, name, and points.
        InitializePlayerData();
        gameData.PlayerList.Add(playerData);
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

        if(!isMultiTraining && dataManager.trackingPerformanceTF)
            dataManager.SetStartingNumBlocks(activeBlocks);

        playerState = PlayerState.Playing;
        playStartTime = Time.time;

        LaunchBall();

        if (gameManager.trainingMode && detectionFreq > 0)
            StartCoroutine(DetectBallAnomaly());
    }

    internal PlayerType GetPlayerType()
    {
        return playerData.Type;
    }

    public void PauseGame()
    {
        ball.gameObject.SetActive(false);
    }

    public string GetName()
    {
        return playerData.playerName;
    }

    void LaunchBall()
    {
        ball.LaunchBall();
    }

    public void LoseColliderHit()
    {
        ball.gameObject.SetActive(false);

        if (playerAgent)
            playerAgent.LoseBall();

        gameManager.PlayerLostBall(this);
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
        playerState = PlayerState.Waiting;

        if(!isMultiTraining && dataManager.trackingPerformanceTF)
            UpdatePlayerPerformanceData(false);
        
        ball.gameObject.SetActive(false);
        
        if (playerAgent)
        {
            playerAgent.TotalPlayTime(Time.time - playStartTime);
            playerAgent.LoseGame();
        }

        StopAllCoroutines();
    }

    public void WinGame()
    {
        playerState = PlayerState.Waiting;
        
        if(!isMultiTraining && dataManager.trackingPerformanceTF)
            UpdatePlayerPerformanceData(true);

        ball.gameObject.SetActive(false);

        if (playerAgent)
        {
            playerAgent.TotalPlayTime(Time.time - playStartTime);
            playerAgent.WinGame();
        }

        StopAllCoroutines();
    }

    public void BlockDestroyed(int pointValue)
    {
        boundaryHits = 0;

        if (playerAgent)
            playerAgent.BlockHit();

        playerData.Points += pointValue;
        UpdatePointsUI();

        --activeBlocks;
        if (activeBlocks <= 0)
        {
            gameManager.PlayerClearedBlocks(this);
        }
    }

    public int GetPoints()
    {
        return playerData.Points;
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
            Quaternion.identity, transform); 
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
        FillRandomLists();

        for(int i = 0; i < GetNumRandomTrainingBlocks(); i++)
        {
            InstantiateRandombBlockGameObject();
        }           
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
        }      
    }
    
    public int GetNumRandomTrainingBlocks()
    {
        int blockNum = 0;
        if(multiBlockCreator.numBlocksChosenRandomlyTF)
        {
            blockNum = (int)Random.Range(5f, 100f);
        }
        else
        {
            blockNum = multiBlockCreator.numBlocksChoiceIfNotRandom;
        }
        return blockNum;
    }

    public void InstantiateRandombBlockGameObject()
    {
        Vector2 randPos = GetRandomBlockVector();

        GameObject block = Instantiate(
            blockGameObjectType,
            new Vector2(
                randPos[0] + this.transform.parent.transform.position.x,
                randPos[1] + this.transform.parent.transform.position.y
            ),
            Quaternion.identity,
            trainingBlocksGroup.transform);
        
        if(block == null)
        {
            Debug.Log("null block");
        }

        if(block)
            block.GetComponent<Block>().playerSupervisor = this;
    }

    public void FillRandomLists()
    {
        for(int x = 0; x < screenWidthWorld; x++)
        {
            // starting y at 2 so we don't interfere w/
            // paddle and ball positions across bottom of screen
            for(int y = 2; y < screenHeightWorld; y++)
            {
                randomBlockXPos.Add(x+0.5f);
                randomBlockYPos.Add(y+0.5f);
                numPossibleBlocks += 1;
                availableBlocksIndexList.Add(numPossibleBlocks-1);
            }
        }
    }

    public Vector2 GetRandomBlockVector()
    {
        int randBlockIndex = GetRandomBlockIndex();
        AddIndexToChosenBlockIndexList(randBlockIndex);
        RemoveIndexFromAvailableBlockList(randBlockIndex);    

        return new Vector2(randomBlockXPos[randBlockIndex], 
                           randomBlockYPos[randBlockIndex]);
    }

    public void AddIndexToChosenBlockIndexList(int newBlockIndex)
    {
        chosenBlocksIndexList.Add(newBlockIndex);
    }

    public void RemoveIndexFromAvailableBlockList(int index)
    {
        availableBlocksIndexList.RemoveAt(index);
    }

    public int GetRandomBlockIndex()
    {
        int randIndex = Random.Range(0, availableBlocksIndexList.Count);
        return randIndex;        
    }

    public void EmptyRandomLists()
    {   
        chosenBlocksIndexList.Clear();
        availableBlocksIndexList.Clear();
    }

    public void DestroyTrainingBlocks()
    {
        EmptyRandomLists();
        numPossibleBlocks = 0;

        if(trainingBlocksGroup)
        {
            foreach(Transform child in trainingBlocksGroup.transform)
            {
                if (child.gameObject.GetComponent<Block>())
                {
                    child.gameObject.SetActive(false);
                    Destroy(child.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Game environment reset for training.  
    /// </summary>
    public void ResetEnvironmentState()
    {
        boundaryHits = 0;
        paddleHits = 0;
        playerData.Points = 0;
        UpdatePointsUI();

        ResetPlayState();
        
        if (useRandomBlocks && randomBlockCreator)
        {
            randomBlockCreator.setupBlocks();
            SetBlockSupervisor(randomBlockCreator.transform);
        }
        else
        {
            DestroyTrainingBlocks();
            CreateTrainingBlocks(); 
        }
        
        CountBlocks();

        // Immediately begin the timeout
        if (gameManager.trainingMode && playerAgent.timeLimit > 0)
            StartCoroutine(Timeout());
    }

    public void ResetBall()
    {
        ball.ResetBall();
        ball.transform.localPosition = ballOffset;
    }

    public void ResetPaddle()
    {
        paddle.transform.localPosition = paddleOffset;
        paddle.smoothMovementChange = 0f;
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

    public void SetPlayerType()
    {
        if (!playerAgent)
        {
            playerData.Type = PlayerType.Human;
            return;
        }

        BehaviorParameters behavior = playerAgent.GetComponent<BehaviorParameters>();
        switch (behavior.BehaviorType)
        {
            case BehaviorType.HeuristicOnly:
                playerData.Type = PlayerType.Human;
                break;
            case BehaviorType.InferenceOnly:
                playerData.Type = PlayerType.AI;
                break;
            case BehaviorType.Default:
                if (behavior.Model != null)
                {
                    playerData.Type = PlayerType.AI;
                }
                else
                {
                    playerData.Type = PlayerType.AI;
                }
                break;
        }
    }

    public void InitializePlayerData()
    {
        if (playerData.playerName.Equals(""))
        {
            playerData.playerName = "Missing Name";
        }
        playerData.Points = 0;
        SetPlayerType();
    }
    public void UpdatePointsUI()
    {
        if (pointsDisplay)
            pointsDisplay.text = $"Points: {GetPoints()}";
    }

    /// <summary>
    /// Reset play state without affecting environment state.
    /// </summary>
    public void ResetPlayState()
    {
        ResetBall();
        ResetPaddle();
        
        playerState = PlayerState.Waiting;
    }
}