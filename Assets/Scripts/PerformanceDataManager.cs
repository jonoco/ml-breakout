﻿using System.Collections.Generic;
using UnityEngine;
// --- the following are for writing performance files only
using System.IO;
using System.Text;
using Unity.MLAgents.Policies;
using UnityEditor;
using System;

public class PerformanceDataManager : MonoBehaviour
{
    public bool trackingPerformanceTF = false;

    [Range(1, 100000)]
    public int trackingNumberOfGames = 2000;

    [Range(1,20)]
    [SerializeField] int gameplayTimeScale = 20;

    [SerializeField] private string nnModelName; 
    [SerializeField] private int numGamesPlayed;  // For agent inference perf tracking
    private string dataDir;
    private int startingNumBlocks;
    private List<string> fileNames;

    private DateTime runTime;

    public bool isHumanPlayer; // no functionality for this yet.
    [SerializeField] PlayerSupervisor playerSupervisor;

    // NOTE: All of these values RELY on the fact that this object
    // is NEVER reset until one stops Unity with the play button.
    // or the editor play is reset programmatically

    // NOTE: all of this list data needs to be OUTSIDE OF playerdata class
    // since that class's data is persistent beyond the end of the game, 
    // unless manually reset.

    [Tooltip("List of all game scores, by individual game, len should match numGames")]
    public List<int> gameScoresList;

    [Tooltip("List of number of blocks broken, by individual game, len should match numGames")]
    public List<int> blocksBrokenList;

    [Tooltip("true=win, false=lose', by individual game, len should match numGames")]
    public List<bool> gameWinStatusList; 

    [Tooltip("Game length in seconds, by individual game, len should match numGames, will be rounded to 2 decimal places")]
    public List<double> gameTimePlayedList;    

    [Tooltip("Count num paddle hits, by individual game, len should match numGames")]
    public List<int> paddleHitCountList;  

    void Awake()
    {
        gameScoresList = new List<int>();
        blocksBrokenList = new List<int>();
        gameWinStatusList = new List<bool>();
        gameTimePlayedList = new List<double>();
        paddleHitCountList = new List<int>();
    }

    void Update()
    {
        if(trackingPerformanceTF)
            PerformanceCheckNumGames();
    }

    private void PerformanceCheckNumGames()
    {
        if(numGamesPlayed >= trackingNumberOfGames && 
           !playerSupervisor.IsMultiAgent() &&
           trackingPerformanceTF)
        {
            #if UNITY_EDITOR
            if(EditorApplication.isPlaying) 
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
            #endif
        }
    }
    
    public void SetStartingNumBlocks(int numBlocks)
    {
        startingNumBlocks = numBlocks;
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
            nnModelName = FindObjectOfType<PlayerAgent>().GetComponent<BehaviorParameters>().Model.name;
            fileNames = new List<string>();
            playerSupervisor = FindObjectOfType<PlayerSupervisor>();
        }
        
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
        dataDir = CreateDataDirIfDoesNotExist();
    }

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

    public void UpdateDataLists(bool winStatus, int sec, int ms, int paddleHits, int activeBlocks)
    {
        // append new data to playerData lists at end of game
        gameScoresList.Add(playerSupervisor.GetPoints());
        blocksBrokenList.Add(startingNumBlocks - activeBlocks);
        gameWinStatusList.Add(winStatus);
        gameTimePlayedList.Add(GetElapsedTimeDouble(sec, ms));
        paddleHitCountList.Add(paddleHits);

    }

    public double GetElapsedTimeDouble(int sec, int ms)
    {
        return System.Math.Round(((double)sec/60 + (double)ms/1000), 2);
    }

    // Unity Default Function
    // In the Editor, Unity calls this message when playmode is stopped.
    // sources:
    // https://docs.unity3d.com/Manual/ExecutionOrder.html
    // https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationQuit.html
    public void OnApplicationQuit()
    {
        if(trackingPerformanceTF)
            WritePlayerDataToTextFiles();
    }

    public void WritePlayerDataToTextFiles()
    {
        CreateEmptyCSVFiles();
        WriteSummaryDataFile();
        WriteRawDataFile();
    }

    // Data:
    // score, blocks broken, paddle hits, time played, win or lose
    // source: https://stackoverflow.com/questions/18757097/writing-data-into-csv-file-in-c-sharp
    public void WriteSummaryDataFile()
    {
        var csv = new StringBuilder();
        csv.AppendLine(HeaderSummaryNewLine());
        csv.AppendLine(MinNewLine());
        csv.AppendLine(AverageNewLine());
        csv.AppendLine(MaxNewLine());
        File.WriteAllText(CreateFilePath(dataDir, fileNames[0]), csv.ToString());
    }

    public string HeaderSummaryNewLine()
    {
        return string.Format(
            "{0},{1},{2},{3},{4},{5},{6},{7},{8}", 
            "ModelName", "NumGamesPlayed", "ID_RunTime", "Statistic",
            "Score", "BlockHits", "PaddleHits",
            "TimePlayed", "WinPercentage"
        );
    }

    public string AverageNewLine()
    {
        string score = GetIntAverage(gameScoresList).ToString();
        string blocksHit = GetIntAverage(blocksBrokenList).ToString();
        string paddleHits = GetIntAverage(paddleHitCountList).ToString();
        string timePlayed = GetDoubleAvg(gameTimePlayedList).ToString();
        string winPct = GetWinPct(gameWinStatusList).ToString();
    
        return string.Format(
            "{0},{1},{2},{3},{4},{5},{6},{7},{8}", 
            nnModelName, numGamesPlayed, runTime, "Average",
            score, blocksHit, paddleHits,
            timePlayed, winPct
        );
    }

    public string MinNewLine()
    {
        string score = GetIntMin(gameScoresList).ToString();
        string blocksHit = GetIntMin(blocksBrokenList).ToString();
        string paddleHits = GetIntMin(paddleHitCountList).ToString();
        string timePlayed = GetDoubleMin(gameTimePlayedList).ToString();
        string winPct = GetWinPct(gameWinStatusList).ToString();
    
        return string.Format(
            "{0},{1},{2},{3},{4},{5},{6},{7},{8}", 
            nnModelName, numGamesPlayed, runTime, "Minimum",
            score, blocksHit, paddleHits,
            timePlayed, winPct
        );
    }

    public string MaxNewLine()
    {
        string score = GetIntMax(gameScoresList).ToString();
        string blocksHit = GetIntMax(blocksBrokenList).ToString();
        string paddleHits = GetIntMax(paddleHitCountList).ToString();
        string timePlayed = GetDoubleMax(gameTimePlayedList).ToString();
        string winPct = GetWinPct(gameWinStatusList).ToString();
    
        return string.Format(
            "{0},{1},{2},{3},{4},{5},{6},{7},{8}",  
            nnModelName, numGamesPlayed, runTime, "Maximum",
            score, blocksHit, paddleHits,
            timePlayed, winPct
        );
    }

    public string HeaderRawNewLine()
    {
        return string.Format(
            "{0},{1},{2},{3},{4},{5},{6},{7},{8}", 
            "ModelName", "GameNumber", "ID_RunTime", "NumGamesPlayed",
            "Score", "BlockHits", "PaddleHits",
            "TimePlayed", "WinStatus"
        );
    }

    public string RawDataLine(int idx)
    {
        return string.Format(
            "{0},{1},{2},{3},{4},{5},{6},{7},{8}", 
            nnModelName, (idx+1).ToString(),  numGamesPlayed, runTime,
            gameScoresList[idx].ToString(), blocksBrokenList[idx].ToString(), paddleHitCountList[idx].ToString(),
            gameTimePlayedList[idx].ToString(), WinConvertBoolToInt(gameWinStatusList[idx])
        );
    }

    public string WinConvertBoolToInt(bool winTF)
    {
        if(winTF){
            return 1.ToString();
        } else {
            return 0.ToString();
        }
    }

    public void WriteRawDataFile()
    {
        var csv = new StringBuilder();
        csv.AppendLine(HeaderRawNewLine());
        for(int i = 0; i < numGamesPlayed; i++)
        {
            csv.AppendLine(RawDataLine(i)); 
        }
        File.WriteAllText(CreateFilePath(dataDir, fileNames[1]), csv.ToString());
    }

    public void CreateEmptyCSVFiles()
    {
        CreateFileNames();
        CreateEmptyFiles();
    }

    public void CreateFileNames()
    {
        var currFiles = new System.IO.DirectoryInfo(dataDir).GetFiles();
        if(currFiles.Length == 0)
        {
            fileNames.Add(BuildFileName("01", "summary"));
            fileNames.Add(BuildFileName("01", "raw"));
            
        } 
        else 
        {
            int currHighestFileNum = FindHighestFileNum();
            string newFileNum = AddLeadingZeroIfSingleDigit(currHighestFileNum+1);
            fileNames.Add(BuildFileName(newFileNum, "summary"));
            fileNames.Add(BuildFileName(newFileNum, "raw"));
        }   
    }

    public void CreateEmptyFiles()
    {
        // source: https://stackoverflow.com/questions/802541/creating-an-empty-file-in-c-sharp
        File.CreateText(CreateFilePath(dataDir, fileNames[0])).Close();
        File.CreateText(CreateFilePath(dataDir, fileNames[1])).Close();
    }

    string BuildFileName(string fileNum, string category)
    {
        return category + "_._" + nnModelName + "_-_" + fileNum + ".csv";
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
        string[] stringSeparator = new string[] { "_-_" };
        foreach(string file in files)
        {
            //file format = typefile_numfile2digits.csv
            int fileNum = System.Int32.Parse(
                Path.GetFileName(file).ToString().
                Split(stringSeparator, System.StringSplitOptions.None)[1].Split('.')[0]
            );

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

      public double GetWinPct(List<bool> games)
    {
        int numWins = 0;
        foreach(bool game in games)
        {
            if(game)
                numWins += 1;
        }
        Debug.Log("Wins: " + numWins + ", Games: " + games.Count);
        if(numWins == 0)
        {
            return 0.0000;
        }
        else
        {
            return System.Math.Round((float)numWins/(float)games.Count,4);
        }
    }

    public int GetIntMin(List<int> nums)
    {
        int min = 0;
        bool firstElement = true;
        foreach(int i in nums)
        {
            if(firstElement)
            {
                min = i;
            }
            else if (i < min)
            {
                min = i;
            }
            firstElement = false;
        }
        return min;
    }

    public double GetDoubleMin(List<double> nums)
    {
        double min = 0;
        bool firstElement = true;
        foreach(double i in nums)
        {
            if(firstElement)
            {
                min = i;
            }
            else if (i < min)
            {
                min = i;
            }
            firstElement = false;
        }
        return min;
    }

        public int GetIntMax(List<int> nums)
    {
        int max = 0;
        bool firstElement = true;
        foreach(int i in nums)
        {
            if(firstElement)
            {
                max = i;
            }
            else if (i > max)
            {
                max = i;
            }
            firstElement = false;
        }
        return max;
    }

    public double GetDoubleMax(List<double> nums)
    {
        double max = 0;
        bool firstElement = true;
        foreach(double i in nums)
        {
            if(firstElement)
            {
                max = i;
            }
            else if (i > max)
            {
                max = i;
            }
            firstElement = false;
        }
        return max;
    }

    public double GetIntAverage(List<int> nums)
    {
        int sum = 0;
        foreach(int i in nums){
            sum += i;
        }
        return System.Math.Round((float)sum/(float)nums.Count,2);
    }

    public double GetDoubleAvg(List<double> nums)
    {
        double sum = 0;
        foreach(double i in nums){
            sum += i;
        }
        return System.Math.Round((float)sum/(float)nums.Count,2);
    }

}