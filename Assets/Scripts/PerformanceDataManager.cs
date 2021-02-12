using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// --- the following are for writing performance files only
using System.IO;  
using System.Text; 
using Unity.MLAgents.Policies; 


public class PerformanceDataManager : MonoBehaviour
{
    [SerializeField] private string nnModelName; 
    [SerializeField] private int numGamesPlayed;  // For agent inference perf tracking
    private string dataDir;
    private int startingNumBlocks;
    private double gameTimeStart = 0;
    private List<string> fileNames;

    [SerializeField] PlayerData playerData;
    
    public void ResetValues()
    {
        startingNumBlocks = 0;
        UpdateGameTimeStart();
    }

    public void SetStartingNumBlocks(int numBlocks)
    {
        startingNumBlocks = numBlocks;
    }
    
    void Start()
    {
        SetDataDirectory();
        nnModelName = FindObjectOfType<PlayerAgent>().GetComponent<BehaviorParameters>().Model.name;
        fileNames = new List<string>();
    }

    public void EndOfGameDataUpdate(bool dataIsTrackedTF, bool gameWin, int sec, int ms, int paddleHits, int activeBlocks)
    {
        if(dataIsTrackedTF)
            UpdatePlayerDataLists(gameWin, sec, ms, paddleHits, activeBlocks);
            IncrementNumGamesPlayed();
    }

    public void IncrementNumGamesPlayed()
    {
        numGamesPlayed += 1;
    }

    public void UpdateGameTimeStart()
    {
        gameTimeStart = Time.time;
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

    public void UpdatePlayerDataLists(bool winStatus, int sec, int ms, int paddleHits, int activeBlocks)
    {
        // append new data to playerData lists at end of game
        playerData.gameScoresList.Add(playerData.points);
        playerData.blocksBrokenList.Add(startingNumBlocks - activeBlocks);
        playerData.gameWinStatusList.Add(winStatus);
        playerData.gameTimePlayedList.Add(GetElapsedTimeDouble(sec, ms));
        playerData.paddleHitCountList.Add(paddleHits);
    }

    public double GetElapsedTimeDouble(int sec, int ms)
    {
        return System.Math.Round(((double)sec/60 + (double)ms/1000), 2);
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
    public void OnApplicationQuit()
    {
        WritePlayerDataToTextFiles();
    }

    public void WritePlayerDataToTextFiles()
    {
        CreateEmptyCSVFiles();
        WriteSummaryDataFile();
        WriteRawDataFile();
        Debug.Log(nnModelName);
    }


    // Data:
    // score, blocks broken, paddle hits, time played, win or lose
    public void WriteSummaryDataFile()
    {
        var csv = new StringBuilder();
        double avgScore = GetIntAverage(playerData.gameScoresList);
        double avgBlocksHit = GetIntAverage(playerData.blocksBrokenList);
        double avgPaddleHit = GetIntAverage(playerData.paddleHitCountList);
        double avgTimePlayed = GetDoubleAvg(playerData.gameTimePlayedList);
        double winPct = GetWinPct(playerData.gameWinStatusList);
        


    }

    public double GetWinPct(List<bool> games)
    {
        int numWins = 0;
        foreach(bool game in games)
        {
            if(game)
                numWins += 1;
        }
        return (double)numWins/(double)games.Count;
    }

    public double GetIntAverage(List<int> nums)
    {
        int sum = 0;
        foreach(int i in nums){
            sum += i;
        }
        return (double)sum/(double)nums.Count;
    }

    public double GetDoubleAvg(List<double> nums)
    {
        double sum = 0;
        foreach(double i in nums){
            sum += i;
        }
        return sum/(double)nums.Count;
    }

    public void WriteRawDataFile()
    {


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



}