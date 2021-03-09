using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] GameData gameData;
    public const float transitionTime = 1.5f;

    public struct SceneNames
    {
        public static readonly string Level_1 = "Level 1";
        public static readonly string Level_FR = "Level_FR";
        public static readonly string EndScreen = "End_Screen";
        public static readonly string Training_0 = "Training_0";
        public static readonly string Level_Two_Player = "Level_Two_Player";
        public static readonly string End_Screen_Two_Player = "End_Screen_Two_Player";

    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }


    public void LoadSceneDelayed(string sceneName, float delay = transitionTime)
    {
        StartCoroutine(DelayedLoadScene(sceneName, delay));
    }

    private IEnumerator DelayedLoadScene(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    public void SetGameEndCondition(int condition)
    {
        gameData.gameEndCondition = (GameEndCondition)condition;
    }
}
