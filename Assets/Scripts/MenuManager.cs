using UnityEngine;
using UnityEngine.UI;
public class MenuManager : MonoBehaviour
{
    public RectTransform descriptionText;
    public RectTransform descriptionNotch;
    
    [Header("Settings")]

    public Color descriptionColor = new Color(1f, 1f, 1f, .4f);
    public float tweenInTime = .3f;
    public float tweenOutTime = .4f;

    private void Awake() 
    {
        if (descriptionNotch)
            descriptionNotch.gameObject.SetActive(false);
        if (descriptionText)
            descriptionText.gameObject.SetActive(false);	
    }

    public void DisplayDescription(string message)
    {
        if (!descriptionNotch.gameObject.activeSelf)
        {
            descriptionNotch.gameObject.SetActive(true);
            descriptionText.gameObject.SetActive(true);
        }

        if (LeanTween.isTweening(descriptionText))
        {
            LeanTween.cancel(descriptionText);
            LeanTween.cancel(descriptionNotch);
        }

        descriptionText.GetComponent<Text>().text = message;
        LeanTween.colorText(descriptionText, descriptionColor, tweenInTime).setEase(LeanTweenType.easeOutExpo);
        LeanTween.color(descriptionNotch, descriptionColor, tweenInTime).setEase(LeanTweenType.easeOutExpo);
    }

    public void HideDescription()
    {
        LeanTween.colorText(descriptionText, Color.clear, tweenOutTime);
        LeanTween.color(descriptionNotch, Color.clear, tweenInTime)
            .setOnComplete(() => { descriptionText.GetComponent<Text>().text = ""; });
    }
}
