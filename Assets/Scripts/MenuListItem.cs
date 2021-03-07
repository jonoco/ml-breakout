using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using DentedPixel;
using UnityEngine.UI;

public class MenuListItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	
	public RectTransform bgPanel;
	public RectTransform bgPanelSelected;
	public RectTransform text;
	public MenuManager menuManager;

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

	private void Start()
	{
		menuManager = FindObjectOfType<MenuManager>();
		if (menuManager)
		{
			menuManager.HideDescription();
			tweenInTime = menuManager.tweenInTime;
			tweenOutTime = menuManager.tweenOutTime;
		}

		startingPosition = transform.position;
		textColor = text.GetComponent<Text>().color; 
		bgPanelColor = bgPanel.GetComponent<Image>().color;
		bgPanelSelectedColor = bgPanelSelected.GetComponent<Image>().color;
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

		if (menuManager)
			menuManager.DisplayDescription(description);
	}

	

	public void OnPointerExit(PointerEventData eventData)
	{
		LeanTween.move(gameObject, startingPosition, tweenOutTime).setEase(LeanTweenType.easeInOutCubic);
		LeanTween.colorText(text, textColor, tweenOutTime).setEase(LeanTweenType.easeInOutCubic);
		LeanTween.color(bgPanelSelected, bgPanelSelectedColor, tweenOutTime).setEase(LeanTweenType.easeInOutCubic);
		LeanTween.color(bgPanel, bgPanelColor, tweenInTime).setEase(LeanTweenType.easeInOutCubic);
		
		if (menuManager)
			menuManager.HideDescription();
	}
}
