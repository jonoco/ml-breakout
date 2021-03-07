using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Intro : MonoBehaviour
{
    public SceneLoader sceneLoader;
    public string startScene;

    // Start is called before the first frame update
    void Start()
    {
        sceneLoader = FindObjectOfType<SceneLoader>();

        GetComponent<VideoPlayer>().loopPointReached += FinishedVideo;
    }

    void FinishedVideo(VideoPlayer vp)
    {
        sceneLoader.LoadScene(startScene);
    }
}
