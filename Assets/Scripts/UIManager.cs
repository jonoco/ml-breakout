using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{

    [SerializeField] List<TextMeshProUGUI> pointsDisplays;
    [SerializeField] TextMeshProUGUI timeDisplay;

    private void Awake()
    {
        // Find the child elements for this prefab's points and timer locations.
        timeDisplay = gameObject.transform.Find("Time Display").gameObject.GetComponent<TextMeshProUGUI>();
        gameObject.GetComponentsInChildren<TextMeshProUGUI>(false, pointsDisplays);
        pointsDisplays.Remove(timeDisplay);
    }

    public void UpdateElapsedTime(string time)
    {
        timeDisplay.text = time;
    }

    public void UpdatePoints(int points, int playerNumber)
    {
        Debug.Log(pointsDisplays[playerNumber - 1].name);
        pointsDisplays[playerNumber - 1].text = $"Points: {points}";
    }
}
