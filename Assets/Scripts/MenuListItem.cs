using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using DentedPixel;
using UnityEngine.UI;

public class MenuListItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	
	public RectTransform bgPanel;
	public RectTransform bgPanelSelected;
	public RectTransform text;
	public RectTransform descriptionText;
	public RectTransform descriptionPanel;

	[Header("Settings")]
	public string description;

	private Vector3 startingPosition;
	private Color bgPanelColor;
	private Color bgPanelSelectedColor;
		
	private Color textColor;
	public Color textColorSelected = new Color(0, 0, 0, 1f);
	public Color bgColorSelected = new Color(1f, 1f, 1f, .8f);

	public Color descriptionColor = new Color(1f, 1f, 1f, .4f);

	public float tweenInTime = .3f;
	public float tweenOutTime = .4f;

	static public bool isHidingDescription = false;

	private void Awake() 
	{
		descriptionPanel.gameObject.SetActive(false);
		descriptionText.gameObject.SetActive(false);	
	}

	private void Start()
	{
		startingPosition = transform.position;
		textColor = text.GetComponent<Text>().color; 
		bgPanelColor = bgPanel.GetComponent<Image>().color;
		bgPanelSelectedColor = bgPanelSelected.GetComponent<Image>().color;

		HideDescription();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		LeanTween.move(gameObject, startingPosition + new Vector3(50f, 0, 0), tweenInTime).setEase(LeanTweenType.easeOutExpo);
		LeanTween.colorText(text, textColorSelected, tweenInTime).setEase(LeanTweenType.easeOutExpo);
		
		Color fadeInColor = bgPanelSelectedColor;
		fadeInColor.a = .7f;
		LeanTween.color(bgPanelSelected, fadeInColor, tweenInTime).setEase(LeanTweenType.easeOutExpo);

		Color fadeOut = bgPanelColor;
		fadeOut.a = 0f;
		LeanTween.color(bgPanel, fadeOut, tweenInTime).setEase(LeanTweenType.easeOutExpo);

		DisplayDescription(description);
	}

	private void DisplayDescription(string message)
	{
		if (!descriptionPanel.gameObject.activeSelf)
		{
			descriptionPanel.gameObject.SetActive(true);
			descriptionText.gameObject.SetActive(true);
		}

		if (LeanTween.isTweening(descriptionText))
		{
			LeanTween.cancel(descriptionText);
			LeanTween.cancel(descriptionPanel);
		}

		descriptionText.GetComponent<Text>().text = description;
		LeanTween.colorText(descriptionText, descriptionColor, tweenInTime).setEase(LeanTweenType.easeOutExpo);
		LeanTween.color(descriptionPanel, descriptionColor, tweenInTime).setEase(LeanTweenType.easeOutExpo);
	}

	private void HideDescription()
	{
		LeanTween.colorText(descriptionText, Color.clear, tweenOutTime);
		LeanTween.color(descriptionPanel, Color.clear, tweenInTime)
			.setOnComplete(() => { descriptionText.GetComponent<Text>().text = ""; });
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		LeanTween.move(gameObject, startingPosition, tweenOutTime).setEase(LeanTweenType.easeInOutCubic);
		LeanTween.colorText(text, textColor, tweenOutTime).setEase(LeanTweenType.easeInOutCubic);
		LeanTween.color(bgPanelSelected, bgPanelSelectedColor, tweenOutTime).setEase(LeanTweenType.easeInOutCubic);
		LeanTween.color(bgPanel, bgPanelColor, tweenInTime).setEase(LeanTweenType.easeInOutCubic);
		
		HideDescription();
	}
}
