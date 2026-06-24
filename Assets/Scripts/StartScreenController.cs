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
    public TextMeshProUGUI promptTMP;
    public RawImage videoDisplay;

    [Header("Prompt Blink")]
    public float blinkSpeed = 2f;
    [Range(0f, 1f)] public float blinkMinAlpha = 0.5f;
    [Range(0f, 1f)] public float blinkMaxAlpha = 1f;

    public float promptFontSize = 64;
    public TMP_FontAsset promptFont;
    public float promptCharacterSpacing = 10f;

    private bool videoFinished;
    private Coroutine blinkCoroutine;

    void Start()
    {
        if (BGMManager.Instance != null)
            BGMManager.Instance.Stop();
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (promptText != null)
        {
            promptText.SetActive(false);
            if (promptTMP == null)
                promptTMP = promptText.GetComponent<TextMeshProUGUI>();
            if (promptTMP != null)
            {
                if (promptFontSize > 0)
                    promptTMP.fontSize = promptFontSize;
                if (promptFont != null)
                    promptTMP.font = promptFont;
                else
                    promptTMP.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                promptTMP.characterSpacing = promptCharacterSpacing;

                // Dark yellow outline
                var mat = Resources.Load<Material>("Fonts & Materials/LiberationSans SDF - Outline");
                if (mat != null)
                {
                    promptTMP.fontMaterial = Object.Instantiate(mat);
                    promptTMP.fontMaterial.SetColor("_OutlineColor", new Color(0.7f, 0.55f, 0f, 1f));
                    promptTMP.fontMaterial.SetFloat("_OutlineWidth", 0.2f);
                }
            }
        }

        string clipName = System.IO.Path.GetFileNameWithoutExtension(videoFileName);
        videoPlayer.clip = Resources.Load<VideoClip>("Videos/" + clipName);
        videoPlayer.source = videoPlayer.clip != null ? VideoSource.VideoClip : VideoSource.Url;
        if (videoPlayer.source == VideoSource.Url)
            videoPlayer.url = (Application.isEditor ? Application.dataPath : Application.streamingAssetsPath) + "/" + videoFileName;
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
        {
            promptText.SetActive(true);
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkPrompt());
        }
    }

    void Update()
    {
        if (videoFinished && AnyKeyPressed())
        {
            if (GameManager.Instance != null)
                GameManager.Instance.LoadEndScene();
            else
                SceneManager.LoadScene("EndScene");
        }
    }

    IEnumerator BlinkPrompt()
    {
        if (promptTMP == null) yield break;
        while (true)
        {
            float t = Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1f);
            float alpha = Mathf.Lerp(blinkMinAlpha, blinkMaxAlpha, t);
            var c = promptTMP.color;
            promptTMP.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
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
