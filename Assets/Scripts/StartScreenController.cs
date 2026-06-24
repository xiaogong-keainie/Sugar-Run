using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

public class StartScreenController : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoPlayer;
    public string videoFileName = "开场动画.mp4";

    [Header("UI")]
    public GameObject promptText;
    public RawImage videoDisplay;

    private bool videoFinished;

    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (promptText != null)
            promptText.SetActive(false);

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = Application.dataPath + "/" + videoFileName;
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Prepare();
        StartCoroutine(PlayWhenReady());
    }

    IEnumerator PlayWhenReady()
    {
        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        videoFinished = true;
        if (promptText != null)
            promptText.SetActive(true);
    }

    void Update()
    {
        if (videoFinished && AnyKeyPressed())
        {
            if (GameManager.Instance != null)
                GameManager.Instance.LoadLevel(0);
            else
                SceneManager.LoadScene("gamesense1");
        }
    }

    bool AnyKeyPressed()
    {
        if (Input.anyKey) return true;
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) return true;
        return false;
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }
}
