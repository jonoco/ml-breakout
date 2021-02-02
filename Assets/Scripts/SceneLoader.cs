using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public float transitionTime = 1.5f; 
    public void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    public void LoadLoseScreen()
    {
        StartCoroutine(DelayedLoadScene("Lose Screen"));
    }

    public void LoadWinScreen()
    {
        StartCoroutine(DelayedLoadScene("Win Screen"));
    }

    private IEnumerator DelayedLoadScene(string sceneName)
    {
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(sceneName);
    }
}
