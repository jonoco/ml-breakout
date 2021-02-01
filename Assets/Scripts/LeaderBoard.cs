using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class LeaderBoard : MonoBehaviour
{
    // Start is called before the first frame update
    private List<Score> scores;
    private List<Transform> scoreTransformList;
    const string PREFS_KEY = "LeaderBoard";
    const int SCORES_LIMIT = 3;

    // Wrapper for Score list because Json cannot directly convert a list and
    // instead takes an object that contains a list.
    private class ScoresWrapper
    {
        public List<Score> scoresList;
    }

    [System.Serializable]
    private class Score
    {
        public string initials;
        public int points;
        public bool newScore;
    }

    void Start()
    {
        // Load the scores and sort them in descending order.
        scores = LoadScores();
        scores.Sort((x, y) => y.points.CompareTo(x.points));

        CheckForNewHighScore();

        SetupLeaderboardUI();
    }


    private List<Score> LoadScores()
    {
        // Attempt retrieving a LeaderBoard PlayerPref.
        string json = PlayerPrefs.GetString(key: PREFS_KEY, defaultValue: "");
        if (!json.Equals(""))
        {
            // If one is found, return the scoresList.
            ScoresWrapper scoresWrapper = JsonUtility.FromJson<ScoresWrapper>(json);
            return scoresWrapper.scoresList;
        }

        // If one is not found, return a default scoresList.
        return new List<Score>() {
            new Score { initials = "AAA", points = 100 },
            new Score { initials = "BBB", points = 50 },
            new Score { initials = "CCC", points = 20 },
        };
    }

    private void AddScore(string initials, int points)
    {
        Score score = new Score { initials = initials, points = points};

        string json = PlayerPrefs.GetString("highScoreTable");
        ScoresWrapper leaderboard = JsonUtility.FromJson<ScoresWrapper>(json);

        leaderboard.scoresList.Add(score);
        string jsonOut = JsonUtility.ToJson(leaderboard);
        PlayerPrefs.SetString("highScoreTable", json);
        PlayerPrefs.Save();
    }

    private void SaveScores()
    {
        // Make sure no newScore bools are saved as true in the db
        ScoresWrapper scoresWrapper = new ScoresWrapper { scoresList = scores };
        string json = JsonUtility.ToJson(scoresWrapper);
        PlayerPrefs.SetString(PREFS_KEY, json);
        PlayerPrefs.Save();
    }

    private void CheckForNewHighScore()
    {
        // Check if the player's score belongs on the leaderboard.
        // int playerPoints = FindObjectOfType<PlayerSupervisor>().GetPoints();
        int playerPoints = 200;
        int index = scores.FindIndex((score) => score.points < playerPoints);
        scores.Insert(index, new Score { initials = "", points = playerPoints, newScore = true });
        while (scores.Count > SCORES_LIMIT)
        {
            scores.RemoveAt(SCORES_LIMIT);
        }
    }

    private void SetupLeaderboardUI()
    {
        Transform scoresContainer = transform.Find("Scores");

        // Get templates for each score display and score input rows, then hide them.
        Transform scoreDisplayTemplate = scoresContainer.Find("Score Display");
        scoreDisplayTemplate.gameObject.SetActive(false);
        Transform scoreInputTemplate = scoresContainer.Find("Score Input");
        scoreInputTemplate.gameObject.SetActive(false);

        // Calculate the distance each score row should be separated by.
        float verticalOffset = scoreDisplayTemplate.GetComponent<RectTransform>().rect.height;

        // Render a row for each score in the given list.
        int scoreCount = 0;
        foreach (Score score in scores)
        {
            Transform scoreTransform;
            if (score.newScore)
            {
                scoreTransform = Instantiate(scoreInputTemplate, scoresContainer);
                scoreTransform.GetComponentInChildren<TMP_InputField>().text = "";
                scoreTransform.Find("Points").GetComponent<TextMeshProUGUI>().text = score.points.ToString();
            } 
            else
            {
                scoreTransform = Instantiate(scoreDisplayTemplate, scoresContainer);
                scoreTransform.Find("Initials").GetComponent<TextMeshProUGUI>().text = score.initials;
                scoreTransform.Find("Points").GetComponent<TextMeshProUGUI>().text = score.points.ToString();
            }
            
            scoreTransform.position = new Vector2(scoreTransform.position.x,
                                                scoreTransform.position.y - verticalOffset * scoreCount);
            scoreTransform.gameObject.SetActive(true);



            scoreCount++;
        }
    }


}
