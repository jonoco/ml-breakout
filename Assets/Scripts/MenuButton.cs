using UnityEngine;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField] AudioClip hoverSound;
    [SerializeField] AudioClip selectSound;

    public void OnPointerEnter(PointerEventData eventData) 
    {
        AudioManager.Instance.PlaySound(hoverSound);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySoundBetweenScenes(selectSound);
    }

}
