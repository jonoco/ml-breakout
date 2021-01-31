using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI pointsDisplay;
    [SerializeField] TextMeshProUGUI timeDisplay;

    private void Start()
    {
        // Find the child elements for this prefab's points and timer locations.
        pointsDisplay = gameObject.transform.Find("Points Display").gameObject.GetComponent<TextMeshProUGUI>();
        timeDisplay = gameObject.transform.Find("Time Display").gameObject.GetComponent<TextMeshProUGUI>();
    }

    public void UpdateElapsedTime(string time)
    {
        timeDisplay.text = time;
    }

    public void UpdatePoints(int points)
    {
        pointsDisplay.text = $"Points: {points}";
    }
}
