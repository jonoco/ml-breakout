using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timeDisplay;

    private void Awake()
    {
        // Find the child elements for this prefab's points and timer locations.
        timeDisplay = gameObject.transform.Find("Time Display").gameObject.GetComponent<TextMeshProUGUI>();
    }

    public void UpdateElapsedTime(string time)
    {
        timeDisplay.text = time;
    }
}
