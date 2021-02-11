using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;  // for writing performance files.
using System.Text; // for writing per files.

public class PlayerSupervisor : MonoBehaviour
{
    [SerializeField] Ball ball;
    [SerializeField] Paddle paddle;
    [SerializeField] GameManager gameManager;
    [SerializeField] PlayerAgent playerAgent;
    [SerializeField] int activeBlocks;
    private int startingNumBlocks;
    private double gameTimeStart = 0;
    [SerializeField] PlayerData playerData;
    [SerializeField] GameObject trainingBlocks;

    private int numGamesPlayed;  // For agent inference perf tracking
    private string dataDir;
    
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

        CountBlocks();

        SetDataDirectory();

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
    // Data tracking - START
    // --------------------------------------------------

    public void UpdatePlayerDataLists(bool winStatus)
    {
        // append new data to playerData lists at end of game
        playerData.gameScoresList.Add(playerData.points);
        playerData.blocksBrokenList.Add(startingNumBlocks - activeBlocks);
        playerData.gameWinStatusList.Add(winStatus);
        playerData.gameTimePlayedList.Add(GetElapsedTimeDouble());
        playerData.paddleHitCountList.Add(paddleHits);
    }

    public double GetElapsedTimeDouble()
    {
        int seconds = (int)gameManager.elapsedTime.TotalSeconds;
        int milliseconds = (int)gameManager.elapsedTime.TotalMilliseconds;
        return System.Math.Round(((double)seconds/60 + (double)milliseconds/1000), 2);
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
        CreateCSVFiles();
        FindHighestFileNum();
    }



    void WriteDataToCSVFile()
    {

        // THINGS TO KEEP IN MIND
        // NUMBER OF GAMES PLAYED IN FILE NAME?
        // PLAYER AGENT SCRIPT NAME?
        // MODEL NAME?
        // IT WOULD BE GREAT IF THESE COULD BE INCLUDED IN THE FILE
        // SO WE WOULDN'T HAVE TO FIGURE THAT OUT LATER!

    }


    void CreateCSVFiles()
    {
        
        var currFiles = new System.IO.DirectoryInfo(dataDir).GetFiles();
        
        if(currFiles.Length == 0)
        {
            // two files are created per performance run, 1 for summary, 1 for raw data
            CreateEmptyFile(CreateFilePath(dataDir, "summarydata_01.csv"));
            CreateEmptyFile(CreateFilePath(dataDir, "rawdata_01.csv"));
        } 
        else 
        {
            int currHighestFileNum = FindHighestFileNum();
            string newFileNum = AddLeadingZeroIfSingleDigit(currHighestFileNum+1);
            CreateEmptyFile(CreateFilePath(dataDir, "summarydata_" + (newFileNum) + ".csv"));
            CreateEmptyFile(CreateFilePath(dataDir, "rawdata_" + (newFileNum) + ".csv"));
        }

    }

    string AddLeadingZeroIfSingleDigit(int num)
    {
        if(num <=9)
        {
            return "0" + num.ToString();
        }
        else 
        {
            return num.ToString();
        }
    }

    int FindHighestFileNum()
    {
        int maxFileNum = 0;
        string[] files = System.IO.Directory.GetFiles(dataDir, "*.csv");
        foreach(string file in files)
        {
            //file format = typefile_numfile2digits.csv
            int fileNum = System.Int32.Parse((Path.GetFileName(file).ToString().Split('_')[1].Split('.')[0]));
            if(fileNum > maxFileNum)
            {
                maxFileNum = fileNum;
            }
        }
        return maxFileNum;
    }

    string CreateFilePath(string fileDir, string fileName)
    {
        return (fileDir + "/" + fileName);
    }

    void CreateEmptyFile(string filename)
    {
        // source: https://stackoverflow.com/questions/802541/creating-an-empty-file-in-c-sharp
        File.CreateText(filename).Close();
    }

    // called in Start, sets performance_directory to var dataDir
    void SetDataDirectory()
    {
        dataDir = CreateDataDirIfDoesNotExist();
    }

    // Returns performance_tracking directory
    string CreateDataDirIfDoesNotExist()
    {
        // Application.dataPath returns ./Assets folder of current project
        // System.IO.DirectoryInfo(path).Parent returns parent of input path to get us to 
        // project folder (putting folder here
        // this so data doesn't import into unity through assets folder)
        // source: https://stackoverflow.com/questions/6875904/how-do-i-find-the-parent-directory-in-c/29409005
        string assetsPath = Application.dataPath;
        string projPath = new System.IO.DirectoryInfo(assetsPath).Parent.ToString();
        string dir =  projPath + "/performance_tracking";
        if(!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        return dir;
    }

    void IncrementPaddleHits()
    {
        paddleHits+=1;
    }

    // --------------------------------------------------
    // Data tracking - END
    // --------------------------------------------------


    public void LoseColliderHit()
    {
        LoseGame();
    }

    public void LoseGame()
    {
        // in our current code logic, this if stmt needs to happen
        // BEFORE game manager calls lose game below b/c GM has an if(Trainingmode)
        // in the losegame method and training mode is technically all of the time.
        // and if trainingmode is true, resetstate is called
        // which resets the paddleHits
        if(gameManager.trackingPerformanceTF)
            UpdatePlayerDataLists(false);
            numGamesPlayed++;

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
        paddleHits = 0;
        points = 0;
        gameManager.UpdatePoints(points);

        ball.gameObject.SetActive(true);
        ball.ResetBall();
        ball.transform.position = ballOffset;

        gameTimeStart = Time.time;
        
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
        IncrementPaddleHits(); // for performance tracking
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

