using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// --- the following are for writing performance files only
using System.IO;  


// Regular C# class, will not be attached to a game object.
public class PDM_Helper
{

    public string CreateDataDirIfDoesNotExist()
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

    public void CreateEmptyFiles(string dir, List<string> fileNames)
    {
        // source: https://stackoverflow.com/questions/802541/creating-an-empty-file-in-c-sharp
        foreach(string file in fileNames){
            File.CreateText(CreateFilePath(dir, file)).Close();
        }
    }

    public int FindHighestFileNum(string dir)
    {
        int maxFileNum = 0;
        string[] files = System.IO.Directory.GetFiles(dir, "*.csv");
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

    public string CreateFilePath(string fileDir, string fileName)
    {
        return (fileDir + "/" + fileName);
    }

    public bool ExistingPerformanceFiles(string dir){
        return new System.IO.DirectoryInfo(dir).GetFiles().Length > 0;
    }

    public string BuildFileName(string modelname, string category, string fileNum)
    {
        return category + "_._" + modelname + "_-_" + fileNum + ".csv";
    }  

    public string GetNextFileNumString(string dir){
        int currHighestFileNum = FindHighestFileNum(dir);
        string newFileNum = AddLeadingZeroIfSingleDigit(currHighestFileNum + 1);
        return newFileNum;
    }

    public string ConvertBoolToInt(bool winTF)
    {
        if(winTF){
            return 1.ToString();
        } else {
            return 0.ToString();
        }
    }

   public double GetElapsedTimeDouble(int sec, int ms)
    {
        return System.Math.Round(((double)sec/60 + (double)ms/1000), 2);
    }
    
    public string AddLeadingZeroIfSingleDigit(int num)
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

    public double GetWinPct(List<bool> games)
    {
        int numWins = 0;
        foreach(bool game in games)
        {
            if(game)
                numWins += 1;
        }
        if(numWins == 0)
        {
            return 0.0000;
        }
        else
        {
            return System.Math.Round((float)numWins/(float)games.Count,4);
        }
    }

    public double GetListMin(List<double> nums)
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

    public double GetListMax(List<double> nums)
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

    public double GetListAvg(List<double> nums)
    {
        double sum = 0;
        foreach(double i in nums){
            sum += i;
        }
        return System.Math.Round((float)sum/(float)nums.Count,2);
    }


}
