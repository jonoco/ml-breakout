using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI pointsDisplay;
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
}
