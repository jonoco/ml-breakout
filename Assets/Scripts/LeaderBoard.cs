using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using static GameData;

public class LeaderBoard : MonoBehaviour
{
    [SerializeField] GameData gameData;
    const string PREFS_KEY = "LeaderBoard";
    const int SCORES_DISPLAY_LIMIT = 3;
    private List<Score> scores;
    private List<Transform> scoreTransforms;


    // Wrapper for Score list because Json cannot directly convert a list and
    // instead takes an object that contains a list.
    private class ScoresWrapper
    {
        public List<Score> scoresList;
    }

    // The Score class represents a single score entry.
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
        return GenerateDefaultScores();
    }

    private List<Score> GenerateDefaultScores()
    {
        return new List<Score>() {
            new Score { initials = "AAA", points = 100 },
            new Score { initials = "BBB", points = 50 },
            new Score { initials = "CCC", points = 20 },
        };
    }

    public void AddNewScore()
    {
        Transform scoreInputRow = scoreTransforms.Find(t => t.name.Contains("Score Input"));

        // Retrieve the input initials. If none were entered, do not save.
        String inputInitials = scoreInputRow.GetComponentInChildren<TMP_InputField>().text;
        if (inputInitials.Equals(""))
        {
            return;
        }

        // Inactivate the Save Button and text box.
        scoreInputRow.Find("Initials Input").gameObject.SetActive(false);
        scoreInputRow.GetComponentInChildren<Button>().gameObject.SetActive(false);

        // Activate the plain text initials display and set it's text equal to the given initials.
        GameObject initialsDisplay = scoreInputRow.Find("Initials Display").gameObject;
        initialsDisplay.SetActive(true);
        initialsDisplay.GetComponent<TextMeshProUGUI>().text = inputInitials;

        // Update the initials property of the edited score object and mark it no longer new.
        Score inputScore = scores.Find(s => s.newScore == true);
        inputScore.initials = inputInitials;
        inputScore.newScore = false;

        // Commit the updated Score list to PlayerPrefs.
        SaveScores();
    }

    private void SaveScores()
    {
        ScoresWrapper scoresWrapper = new ScoresWrapper { scoresList = scores };
        string json = JsonUtility.ToJson(scoresWrapper);
        PlayerPrefs.SetString(PREFS_KEY, json);
        PlayerPrefs.Save();
    }

    private void CheckForNewHighScore()
    {
        // Check if the player's score places within the SCORES_DISPLAY_LIMIT
        // of existing high scores.
        PlayerData humanPlayerData = gameData.PlayerList.Find(ps => ps.playerType == PlayerType.Human);
        if (humanPlayerData == null)
        {
            return;
        }

        int index = scores.FindIndex((score) => score.points < humanPlayerData.Points);
        if (index != -1)
        {
            scores.Insert(index, new Score { initials = "", points = humanPlayerData.Points, newScore = true });

            // If a high score was added to the list, reduce the lowest scores until the
            // list's size to matches the SCORES_DISPLAY_LIMIT.
            while (scores.Count > SCORES_DISPLAY_LIMIT)
            {
                scores.RemoveAt(SCORES_DISPLAY_LIMIT);
            }
        }
    }

    // Dynamic creation of score transforms based on Code Monkey tutorial "High Score Table
    // with Saving and Loading": https://youtu.be/iAbaqGYdnyI.
    private void SetupLeaderboardUI()
    {
        transform.Find("Game Result").GetComponent<Text>().text = gameData.gameResult;
        Transform scoresContainer = transform.Find("Scores");

        // Get templates for score display and score input rows then hide them.
        Transform scoreDisplayTemplate = scoresContainer.Find("Score Display");
        scoreDisplayTemplate.gameObject.SetActive(false);
        Transform scoreInputTemplate = scoresContainer.Find("Score Input");
        scoreInputTemplate.gameObject.SetActive(false);

        // Calculate the distance each score row should be separated by.
        float verticalOffset = scoreDisplayTemplate.GetComponent<RectTransform>().rect.height;

        // Render a row for each score in the given list.
        scoreTransforms = new List<Transform>();
        foreach (Score score in scores)
        {
            Transform scoreTransform;
            if (score.newScore)
            {
                // Create a Score Input row.
                scoreTransform = Instantiate(scoreInputTemplate, scoresContainer);
                scoreTransform.Find("Points").GetComponent<TextMeshProUGUI>().text = score.points.ToString();
            }
            else
            {
                // Create a Score Display row.
                scoreTransform = Instantiate(scoreDisplayTemplate, scoresContainer);
                scoreTransform.Find("Initials").GetComponent<TextMeshProUGUI>().text = score.initials;
                scoreTransform.Find("Points").GetComponent<TextMeshProUGUI>().text = score.points.ToString();
            }

            // Adjust the row's vertical position:
            scoreTransform.position = new Vector2(scoreTransform.position.x,
                                                scoreTransform.position.y - verticalOffset * scoreTransforms.Count);
            scoreTransform.gameObject.SetActive(true);

            // Add the score to the list of transforms for easier manipulation later.
            scoreTransforms.Add(scoreTransform);
        }
    }

    public void ResetScores()
    {
        foreach(Transform score in scoreTransforms)
        {
            Destroy(score.gameObject);
        }

        scores = GenerateDefaultScores();
        SaveScores();
        SetupLeaderboardUI();
    }
}
