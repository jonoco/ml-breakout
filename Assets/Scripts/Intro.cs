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
        VideoPlayer videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.url = System.IO.Path.Combine (Application.streamingAssetsPath,"title.mp4");
        videoPlayer.Play();
        
        videoPlayer.loopPointReached += FinishedVideo;
    }

    void FinishedVideo(VideoPlayer vp)
    {
        sceneLoader.LoadScene(startScene);
    }
}
