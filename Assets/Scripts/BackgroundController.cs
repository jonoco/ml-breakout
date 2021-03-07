using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundController : MonoBehaviour
{
    public RectTransform scrim; 

     [Range(.1f, 3f)] public float tweenTime = 1f;

     [Range(0f, 1f)] public float scrimMinAlpha = .5f;

    // Start is called before the first frame update
    void Start()
    {
        if (!scrim)
        {
            Debug.LogError("No scrim found");
            return;
        }

        Color scrimColor = scrim.GetComponent<Image>().color;
        Color scrimColorMin = scrimColor;
        scrimColorMin.a = scrimMinAlpha;

		LeanTween.color(scrim, scrimColorMin, tweenTime).setLoopPingPong().setEase(LeanTweenType.easeInOutSine);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
