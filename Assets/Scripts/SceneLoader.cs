using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public float transitionTime = 1.5f; 
    public void LoadFirstLevel()
    {
        SceneManager.LoadScene("Level 1");
    }

    public void LoadLoseScreen()
    {
        StartCoroutine(DelayedLoadScene("Lose Screen"));
    }

    public void LoadWinScreen()
    {
        StartCoroutine(DelayedLoadScene("Win Screen"));
    }

    private IEnumerator DelayedLoadScene(string scene_name)
    {
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(scene_name);
    }
}
