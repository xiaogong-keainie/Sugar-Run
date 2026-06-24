using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;
using TMPro;

public class EndScreenController : MonoBehaviour
{
    [Header("Level Buttons")]
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;

    [Header("Other")]
    public Button backButton;

    [Header("Introduce")]
    public Button introduceButton;
    public GameObject introducePanel;

    [Header("Style")]
    public TMP_FontAsset buttonFont;
    public float borderRadius = 8f;

    [Header("Background Video")]
    public string videoFileName = "大肠内部4.mp4";

    [Header("Audio")]
    public AudioClip hoverSound;
    public float hoverVolume = 1f;

    [Header("Settings UI (assign in Canvas to customize)")]
    public GameObject settingsButton;
    public GameObject settingsPanel;
    public Slider volumeSlider;

    private bool settingsOpen;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();

        if (level1Button != null)
            level1Button.onClick.AddListener(() => GameManager.Instance.LoadLevel(0));
        if (level2Button != null)
            level2Button.onClick.AddListener(() => GameManager.Instance.LoadLevel(1));
        if (level3Button != null)
            level3Button.onClick.AddListener(() => GameManager.Instance.LoadLevel(2));
        if (backButton != null)
            backButton.onClick.AddListener(() => GameManager.Instance.LoadStartScene());

        if (introduceButton != null)
            introduceButton.onClick.AddListener(ToggleIntroduce);

        if (introducePanel == null)
            CreateIntroducePanel();
        else
            introducePanel.SetActive(false);

        CreateBackgroundVideo();
        MakeButtonsRounded();
        AddHoverSounds();
        ApplyButtonFont();
        CreateSettingsUI();
    }

    // ── Background Video ──

    void CreateBackgroundVideo()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        var bgGo = new GameObject("VideoBackground", typeof(RectTransform), typeof(RawImage));
        bgGo.transform.SetParent(canvas.transform, false);
        var rt = bgGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        var rawImage = bgGo.GetComponent<RawImage>();
        rawImage.color = Color.white;

        var vp = bgGo.AddComponent<VideoPlayer>();
        vp.source = VideoSource.Url;
        vp.url = (Application.isEditor ? Application.dataPath : Application.streamingAssetsPath) + "/" + videoFileName;
        vp.renderMode = VideoRenderMode.APIOnly;
        vp.isLooping = true;
        vp.playOnAwake = true;
        vp.Prepare();
        vp.prepareCompleted += (source) =>
        {
            rawImage.texture = source.texture;
            source.Play();
        };

        // Move to first child so it's behind everything
        bgGo.transform.SetAsFirstSibling();
    }

    // ── Rounded Buttons ──

    void MakeButtonsRounded()
    {
        foreach (var btn in new[] { level1Button, level2Button, level3Button, backButton, introduceButton })
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
                    // Corner quadrants
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
                        // Find corner center
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

    // ── Button Font ──

    void ApplyButtonFont()
    {
        if (buttonFont == null) return;

        foreach (var btn in new[] { level1Button, level2Button, level3Button, backButton, introduceButton })
        {
            if (btn == null) continue;
            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.font = buttonFont;
        }
    }

    // ── Hover Sound ──

    void AddHoverSounds()
    {
        if (hoverSound == null) return;

        foreach (var btn in new[] { level1Button, level2Button, level3Button, backButton, introduceButton })
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

    // ── Introduce Panel ──

    void CreateIntroducePanel()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        introducePanel = new GameObject("IntroducePanel", typeof(RectTransform), typeof(Image));
        introducePanel.transform.SetParent(canvas.transform, false);
        var panelRt = introducePanel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(600, 400);
        panelRt.anchoredPosition = Vector2.zero;

        var panelImg = introducePanel.GetComponent<Image>();
        panelImg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        // Title
        var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(introducePanel.transform, false);
        var titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1);
        titleRt.sizeDelta = new Vector2(0, 60);
        titleRt.anchoredPosition = new Vector2(0, -10);
        var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
        titleTmp.text = "游戏介绍";
        titleTmp.fontSize = 36;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = new Color(1, 0.8f, 0, 1);
        if (buttonFont != null) titleTmp.font = buttonFont;

        // Body text
        var bodyGo = new GameObject("Body", typeof(RectTransform), typeof(TextMeshProUGUI));
        bodyGo.transform.SetParent(introducePanel.transform, false);
        var bodyRt = bodyGo.GetComponent<RectTransform>();
        bodyRt.anchorMin = Vector2.zero;
        bodyRt.anchorMax = Vector2.one;
        bodyRt.pivot = new Vector2(0.5f, 0.5f);
        bodyRt.sizeDelta = new Vector2(-40, -80);
        bodyRt.anchoredPosition = new Vector2(0, -10);
        var bodyTmp = bodyGo.GetComponent<TextMeshProUGUI>();
        bodyTmp.text = "•  食物：恢复生命值\n\n•  蓝色药水：拾取后获得免疫效果\n   按 Q 键使用\n\n•  绿色药水：自动生效，5秒内\n   生命和体力不会减少\n\n•  奖杯：收集后通关";
        bodyTmp.fontSize = 24;
        bodyTmp.alignment = TextAlignmentOptions.Left;
        bodyTmp.color = Color.white;
        bodyTmp.lineSpacing = 30;

        introducePanel.SetActive(false);
    }

    void ToggleIntroduce()
    {
        if (introducePanel == null) return;
        bool show = !introducePanel.activeSelf;
        introducePanel.SetActive(show);
        if (show)
            introducePanel.transform.SetAsLastSibling();
    }

    // ── Settings UI (top-left gear) ──

    void CreateSettingsUI()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // If already assigned in Inspector, just wire up listeners
        if (settingsButton != null && settingsPanel != null && volumeSlider != null)
        {
            var btn = settingsButton.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(ToggleSettings);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            settingsPanel.SetActive(false);
            return;
        }

        // ── Dynamic creation (fallback) ──

        // Settings gear button (top-left)
        settingsButton = new GameObject("SettingsBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        settingsButton.transform.SetParent(canvas.transform, false);
        var btnRt = settingsButton.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0, 1);
        btnRt.anchorMax = new Vector2(0, 1);
        btnRt.pivot = new Vector2(0, 1);
        btnRt.sizeDelta = new Vector2(50, 50);
        btnRt.anchoredPosition = new Vector2(10, -10);

        var btnImg = settingsButton.GetComponent<Image>();
        btnImg.color = new Color(1, 1, 1, 0.7f);

        var btnComp = settingsButton.GetComponent<Button>();
        btnComp.transition = Selectable.Transition.ColorTint;
        btnComp.onClick.AddListener(ToggleSettings);

        // Gear text
        var gearGo = new GameObject("GearText", typeof(RectTransform), typeof(TextMeshProUGUI));
        gearGo.transform.SetParent(settingsButton.transform, false);
        var gearRt = gearGo.GetComponent<RectTransform>();
        gearRt.anchorMin = Vector2.zero;
        gearRt.anchorMax = Vector2.one;
        gearRt.sizeDelta = Vector2.zero;
        var gearTmp = gearGo.GetComponent<TextMeshProUGUI>();
        gearTmp.text = "⚙";
        gearTmp.fontSize = 28;
        gearTmp.alignment = TextAlignmentOptions.Center;
        gearTmp.color = Color.white;

        // Settings panel (hidden by default)
        settingsPanel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(Image));
        settingsPanel.transform.SetParent(canvas.transform, false);
        var panelRt = settingsPanel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0, 1);
        panelRt.anchorMax = new Vector2(0, 1);
        panelRt.pivot = new Vector2(0, 1);
        panelRt.sizeDelta = new Vector2(220, 80);
        panelRt.anchoredPosition = new Vector2(10, -70);

        var panelImg = settingsPanel.GetComponent<Image>();
        panelImg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        // Volume label
        var volLabel = new GameObject("VolLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        volLabel.transform.SetParent(settingsPanel.transform, false);
        var labelRt = volLabel.GetComponent<RectTransform>();
        labelRt.anchorMin = new Vector2(0, 0.5f);
        labelRt.anchorMax = new Vector2(0, 0.5f);
        labelRt.pivot = new Vector2(0, 0.5f);
        labelRt.sizeDelta = new Vector2(50, 30);
        labelRt.anchoredPosition = new Vector2(10, 0);
        var labelTmp = volLabel.GetComponent<TextMeshProUGUI>();
        labelTmp.text = "Volume";
        labelTmp.fontSize = 18;
        labelTmp.alignment = TextAlignmentOptions.Left;
        labelTmp.color = Color.white;

        // Volume slider
        var sliderGo = new GameObject("VolSlider", typeof(RectTransform), typeof(Slider));
        sliderGo.transform.SetParent(settingsPanel.transform, false);
        var sliderRt = sliderGo.GetComponent<RectTransform>();
        sliderRt.anchorMin = new Vector2(0, 0);
        sliderRt.anchorMax = new Vector2(1, 0.5f);
        sliderRt.pivot = new Vector2(0.5f, 0.5f);
        sliderRt.sizeDelta = new Vector2(-70, -10);
        sliderRt.anchoredPosition = new Vector2(-5, 0);

        volumeSlider = sliderGo.GetComponent<Slider>();
        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        volumeSlider.value = GameManager.Instance != null ? GameManager.Instance.GetVolume() : AudioListener.volume;

        // Slider bg
        var bgGo = new GameObject("Background", typeof(RectTransform));
        bgGo.transform.SetParent(sliderGo.transform, false);
        var bgRt = bgGo.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        var bgImg = bgGo.AddComponent<Image>();
        bgImg.color = new Color(0.5f, 0.5f, 0.5f, 1);

        // Slider fill
        var fillGo = new GameObject("Fill", typeof(RectTransform));
        fillGo.transform.SetParent(sliderGo.transform, false);
        var fillRt = fillGo.GetComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0, 0);
        fillRt.anchorMax = new Vector2(0, 1);
        fillRt.sizeDelta = new Vector2(0, 0);
        var fillImg = fillGo.AddComponent<Image>();
        fillImg.color = new Color(1, 0.5f, 0, 1);

        // Slider handle
        var handleGo = new GameObject("Handle", typeof(RectTransform));
        handleGo.transform.SetParent(sliderGo.transform, false);
        var handleRt = handleGo.GetComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(20, 20);
        handleRt.anchorMin = new Vector2(0, 0);
        handleRt.anchorMax = new Vector2(0, 1);
        var handleImg = handleGo.AddComponent<Image>();
        handleImg.color = Color.white;

        volumeSlider.fillRect = fillRt;
        volumeSlider.handleRect = handleRt;
        volumeSlider.targetGraphic = handleImg;
        volumeSlider.direction = Slider.Direction.LeftToRight;
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        settingsPanel.SetActive(false);
    }

    void ToggleSettings()
    {
        settingsOpen = !settingsOpen;
        if (settingsPanel != null)
            settingsPanel.SetActive(settingsOpen);
        if (settingsButton != null)
            settingsButton.SetActive(!settingsOpen);
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (settingsOpen)
        {
            var rect = settingsPanel.GetComponent<RectTransform>();
            if (rect != null && !RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, null))
            {
                ToggleSettings();
                return;
            }
        }

        if (introducePanel != null && introducePanel.activeSelf)
        {
            var rect = introducePanel.GetComponent<RectTransform>();
            if (rect != null && !RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, null))
            {
                introducePanel.SetActive(false);
            }
        }
    }

    void OnVolumeChanged(float value)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetVolume(value);
    }
}
