using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InGameSettings : MonoBehaviour
{
    [Header("Settings Button (always visible)")]
    public Button settingsButton;
    public Sprite settingsIcon;

    [Header("Settings Panel")]
    public GameObject settingsPanel;
    public Slider volumeSlider;
    public Button resumeButton;
    public Button exitButton;

    [Header("Victory Panel Reference (optional)")]
    public GameObject victoryPanel;

    [Header("Style")]
    public float borderRadius = 8f;

    [Header("Audio")]
    public AudioClip hoverSound;
    public float hoverVolume = 1f;

    private bool isPaused;
    private VideoBackground videoBackground;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OpenSettings);
            if (settingsIcon != null)
            {
                var img = settingsButton.GetComponent<Image>();
                if (img != null)
                {
                    img.sprite = settingsIcon;
                    img.type = Image.Type.Simple;
                    img.color = Color.white;
                }
                // Hide the text child
                var textChild = settingsButton.GetComponentInChildren<TextMeshProUGUI>(true);
                if (textChild != null)
                    textChild.gameObject.SetActive(false);
            }
        }

        var mainCam = Camera.main;
        if (mainCam != null)
            videoBackground = mainCam.GetComponent<VideoBackground>();

        float currentVolume = GameManager.Instance != null ? GameManager.Instance.GetVolume() : AudioListener.volume;

        if (videoBackground != null)
            videoBackground.SetVolume(currentVolume);

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = currentVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        if (resumeButton != null)
            resumeButton.onClick.AddListener(CloseSettings);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitToStart);

        MakePanelRounded();
        MakeButtonsRounded();
        AddHoverSounds();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (victoryPanel != null && victoryPanel.activeSelf)
                return;

            if (isPaused)
                CloseSettings();
            else
                OpenSettings();
        }
    }

    void OpenSettings()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    void CloseSettings()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void OnVolumeChanged(float value)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetVolume(value);

        if (videoBackground != null)
            videoBackground.SetVolume(value);
    }

    void ExitToStart()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.LoadEndScene();
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    // ── Rounded Corners ──

    void MakePanelRounded()
    {
        if (settingsPanel == null) return;
        var img = settingsPanel.GetComponent<Image>();
        if (img == null) return;

        int texSize = 64;
        int radius = Mathf.RoundToInt(borderRadius);
        var tex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
        Color clear = Color.clear;
        Color white = Color.white;

        for (int y = 0; y < texSize; y++)
        {
            for (int x = 0; x < texSize; x++)
            {
                bool topLeft = x < radius && y < radius;
                bool topRight = x >= texSize - radius && y < radius;
                bool botLeft = x < radius && y >= texSize - radius;
                bool botRight = x >= texSize - radius && y >= texSize - radius;

                if (!topLeft && !topRight && !botLeft && !botRight)
                {
                    tex.SetPixel(x, y, white);
                }
                else
                {
                    int cx = x < radius ? radius : texSize - 1 - radius;
                    int cy = y < radius ? radius : texSize - 1 - radius;
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    tex.SetPixel(x, y, dist <= radius ? white : clear);
                }
            }
        }
        tex.Apply();

        var sprite = Sprite.Create(tex, new Rect(0, 0, texSize, texSize),
            new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.Tight,
            new Vector4(radius, radius, radius, radius));

        img.sprite = sprite;
        img.type = Image.Type.Sliced;
    }

    void MakeButtonsRounded()
    {
        foreach (var btn in new[] { settingsButton, resumeButton, exitButton })
        {
            if (btn == null) continue;
            var img = btn.GetComponent<Image>();
            if (img == null) continue;

            int texSize = 64;
            int radius = Mathf.RoundToInt(borderRadius);
            var tex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
            Color clear = Color.clear;
            Color white = Color.white;

            for (int y = 0; y < texSize; y++)
            {
                for (int x = 0; x < texSize; x++)
                {
                    bool topLeft = x < radius && y < radius;
                    bool topRight = x >= texSize - radius && y < radius;
                    bool botLeft = x < radius && y >= texSize - radius;
                    bool botRight = x >= texSize - radius && y >= texSize - radius;

                    if (!topLeft && !topRight && !botLeft && !botRight)
                    {
                        tex.SetPixel(x, y, white);
                    }
                    else
                    {
                        int cx = x < radius ? radius : texSize - 1 - radius;
                        int cy = y < radius ? radius : texSize - 1 - radius;
                        float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                        tex.SetPixel(x, y, dist <= radius ? white : clear);
                    }
                }
            }
            tex.Apply();

            var sprite = Sprite.Create(tex, new Rect(0, 0, texSize, texSize),
                new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.Tight,
                new Vector4(radius, radius, radius, radius));

            img.sprite = sprite;
            img.type = Image.Type.Sliced;
        }
    }

    // ── Hover Sound ──

    void AddHoverSounds()
    {
        if (hoverSound == null) return;

        foreach (var btn in new[] { settingsButton, resumeButton, exitButton })
        {
            if (btn == null) continue;
            var trigger = btn.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = btn.gameObject.AddComponent<EventTrigger>();

            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((_) => audioSource.PlayOneShot(hoverSound, hoverVolume));
            trigger.triggers.Add(entry);
        }
    }
}
