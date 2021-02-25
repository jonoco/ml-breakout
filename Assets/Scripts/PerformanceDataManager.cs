using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// --- the following are for writing performance files only
using System.IO;  
using System.Text; 
using Unity.MLAgents.Policies; 
using UnityEditor;
using System;

public class PerformanceDataManager : MonoBehaviour
{
    // ------- Game Objects
    [SerializeField] PlayerSupervisor playerSupervisor;

    // ------- Game Parameters & Data
    public bool trackingPerformanceTF = false;

    [Range(1, 100000)]
    public int trackingNumberOfGames = 2000;

    [Range(1,20)]
    [SerializeField] int gameplayTimeScale = 20;

    [SerializeField] private string NeuralNetworkModelName; 
    [SerializeField] private int numGamesPlayed;

    // NOTE: All of these values RELY on the fact that this object
    // is NEVER reset until one stops Unity with the play button.
    // or the editor play is reset programmatically

    // NOTE: all of this list data needs to be OUTSIDE OF playerdata class
    // since that class's data is persistent beyond the end of the game, 
    // unless manually reset.

    [Tooltip("List of all game scores, by individual game, len should match numGames")]
    public List<double> gameScoresList;

    [Tooltip("List of number of blocks broken, by individual game, len should match numGames")]
    public List<double> blocksBrokenList;

    [Tooltip("true=win, false=lose', by individual game, len should match numGames")]
    public List<bool> gameWinStatusList; 

    [Tooltip("Game length in seconds, by individual game, len should match numGames, will be rounded to 2 decimal places")]
    public List<double> gameTimePlayedList;    

    [Tooltip("Count num paddle hits, by individual game, len should match numGames")]
    public List<double> paddleHitCountList;  

    // ------- Private variables
    private string dataDir;
    private int startingNumBlocks;
    private List<string> fileNames;
    private DateTime runTime;
    private PDM_Helper helper = new PDM_Helper();

    void Awake()
    {
        gameScoresList = new List<double>();
        blocksBrokenList = new List<double>();
        gameWinStatusList = new List<bool>();
        gameTimePlayedList = new List<double>();
        paddleHitCountList = new List<double>();
    }

    void Start()
    {
        if(trackingPerformanceTF)
        {  
            // Used as a unique ID for each run, likely this will come in handy
            runTime = System.DateTime.Now;

            // SET TIME SCALE to 20 for performance tracking to speed it up
            Time.timeScale = gameplayTimeScale;

            SetDataDirectory();
            NeuralNetworkModelName = FindObjectOfType<PlayerAgent>().GetComponent<BehaviorParameters>().Model.name;
            fileNames = new List<string>();
            playerSupervisor = FindObjectOfType<PlayerSupervisor>();
        }        
    }

    void Update()
    {
        if(IsPerformanceTrackingOver())
            EndPlayInEditor();
    }

    // In the Editor, Unity calls this default function when playmode is stopped.
    public void OnApplicationQuit()
    {
        if(trackingPerformanceTF)
            WritePlayerDataToTextFiles();
    }

    public bool IsPerformanceTrackingOver(){
        return (
            numGamesPlayed >= trackingNumberOfGames && 
            !playerSupervisor.IsMultiAgent() &&
            trackingPerformanceTF
        );
    }

    public void EndPlayInEditor(){
        #if UNITY_EDITOR
        if(EditorApplication.isPlaying) 
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
        #endif
    }
    
    public void SetStartingNumBlocks(int numBlocks)
    {
        startingNumBlocks = numBlocks;
    }

    public void EndOfGameDataUpdate(bool gameWin, int sec, int ms, int paddleHits, int activeBlocks)
    {
        if(trackingPerformanceTF)
            UpdateDataLists(gameWin, sec, ms, paddleHits, activeBlocks);
            IncrementNumGamesPlayed();
    }

    public void IncrementNumGamesPlayed()
    {
        numGamesPlayed += 1;
    }

    public void SetDataDirectory()
    {
        dataDir = helper.CreateDataDirIfDoesNotExist();
    }

    public void UpdateDataLists(bool winStatus, int sec, int ms, int paddleHits, int activeBlocks)
    {
        gameScoresList.Add((double)playerSupervisor.GetPoints());
        blocksBrokenList.Add((double)(startingNumBlocks - activeBlocks));
        gameWinStatusList.Add(winStatus);
        gameTimePlayedList.Add(helper.GetElapsedTimeDouble(sec, ms));
        paddleHitCountList.Add((double)paddleHits);

    }

    public void WritePlayerDataToTextFiles()
    {
        CreateEmptyCSVFiles();
        WriteSummaryDataFile();
        WriteRawDataFile();
    }
    
    public void WriteSummaryDataFile()
    {
        var csv = new StringBuilder();
        csv.AppendLine(HeaderSummaryNewLine());
        csv.AppendLine(MinNewLine());
        csv.AppendLine(AverageNewLine());
        csv.AppendLine(MaxNewLine());
        File.WriteAllText(helper.CreateFilePath(dataDir, fileNames[0]), csv.ToString());
    }

    public void WriteRawDataFile()
    {
        var csv = new StringBuilder();
        csv.AppendLine(HeaderRawNewLine());
        for(int i = 0; i < numGamesPlayed; i++)
        {
            csv.AppendLine(RawDataLine(i)); 
        }
        File.WriteAllText(helper.CreateFilePath(dataDir, fileNames[1]), csv.ToString());
    }

    public string HeaderSummaryNewLine()
    {
        return string.Format(
            "{0},{1},{2},{3},{4},{5},{6},{7},{8}", 
            "ModelName",
            "NumGamesPlayed",
            "ID_RunTime",
            "Statistic",
            "Score",
            "BlockHits",
            "PaddleHits",
            "TimePlayed",
            "WinPercentage"
        );
    }

    public string AverageNewLine()
    {
        string score = helper.GetListAvg(gameScoresList).ToString();
        string blocksHit = helper.GetListAvg(blocksBrokenList).ToString();
        string paddleHits = helper.GetListAvg(paddleHitCountList).ToString();
        string timePlayed = helper.GetListAvg(gameTimePlayedList).ToString();
        string winPct = helper.GetWinPct(gameWinStatusList).ToString();
    
        return string.Format(
            "{0},{1},{2},{3},{4},{5},{6},{7},{8}", 
            NeuralNetworkModelName,
            numGamesPlayed,
            runTime,
            "Average",
            score,
            blocksHit,
            paddleHits,
            timePlayed,
            winPct
        );
    }

    public string MinNewLine()
    {
        string score = helper.GetListMin(gameScoresList).ToString();
        string blocksHit = helper.GetListMin(blocksBrokenList).ToString();
        string paddleHits = helper.GetListMin(paddleHitCountList).ToString();
        string timePlayed = helper.GetListMin(gameTimePlayedList).ToString();
        string winPct = helper.GetWinPct(gameWinStatusList).ToString();
    
        return string.Format(
            "{0},{1},{2},{3},{4},{5},{6},{7},{8}", 
            NeuralNetworkModelName,
            numGamesPlayed,
            runTime,
            "Minimum",
            score, 
            blocksHit,
            paddleHits,
            timePlayed,
            winPct
        );
    }

    public string MaxNewLine()
    {
        string score = helper.GetListMax(gameScoresList).ToString();
        string blocksHit = helper.GetListMax(blocksBrokenList).ToString();
        string paddleHits = helper.GetListMax(paddleHitCountList).ToString();
        string timePlayed = helper.GetListMax(gameTimePlayedList).ToString();
        string winPct = helper.GetWinPct(gameWinStatusList).ToString();
    
        return string.Format(
            "{0},{1},{2},{3},{4},{5},{6},{7},{8}",  
            NeuralNetworkModelName,
            numGamesPlayed,
            runTime,
            "Maximum",
            score,
            blocksHit,
            paddleHits,
            timePlayed,
            winPct
        );
    }

    public string HeaderRawNewLine()
    {
        return string.Format(
            "{0},{1},{2},{3},{4},{5},{6},{7},{8}", 
            "ModelName",
            "GameNumber",
            "ID_RunTime", 
            "NumGamesPlayed",
            "Score", 
            "BlockHits",
            "PaddleHits",
            "TimePlayed",
            "WinStatus"
        );
    }

    public string RawDataLine(int idx)
    {
        return string.Format(
            "{0},{1},{2},{3},{4},{5},{6},{7},{8}", 
            NeuralNetworkModelName, 
            (idx+1).ToString(), 
            numGamesPlayed, 
            runTime,
            gameScoresList[idx].ToString(), 
            blocksBrokenList[idx].ToString(),
            paddleHitCountList[idx].ToString(),
            gameTimePlayedList[idx].ToString(), 
            helper.ConvertBoolToInt(gameWinStatusList[idx])
        );
    }

    public void CreateEmptyCSVFiles()
    {
        AddFileNamesToList();
        helper.CreateEmptyFiles(dataDir, fileNames);
    }

    public void AddFileNamesToList()
    {
        if(!helper.ExistingPerformanceFiles(dataDir)){
            _listAdd(NeuralNetworkModelName, "01");
        } else{
            _listAdd(NeuralNetworkModelName, helper.GetNextFileNumString(dataDir));
        }
    }

    public void _listAdd(string modelName, string fileNum)
    {
        fileNames.Add(helper.BuildFileName(NeuralNetworkModelName, "summary", fileNum));
        fileNames.Add(helper.BuildFileName(NeuralNetworkModelName, "raw", fileNum));
    }

}