using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameSettings : MonoBehaviour
{
    [Header("Settings Button (always visible)")]
    public Button settingsButton;

    [Header("Settings Panel")]
    public GameObject settingsPanel;
    public Slider volumeSlider;
    public Button resumeButton;
    public Button exitButton;

    [Header("Victory Panel Reference (optional)")]
    public GameObject victoryPanel;

    private bool isPaused;
    private VideoBackground videoBackground;

    void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        // Find VideoBackground on Main Camera for direct volume control
        var mainCam = Camera.main;
        if (mainCam != null)
            videoBackground = mainCam.GetComponent<VideoBackground>();

        float currentVolume = GameManager.Instance != null ? GameManager.Instance.GetVolume() : AudioListener.volume;

        // Sync VideoPlayer volume to match current setting
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

        // Directly control the VideoPlayer's audio volume
        if (videoBackground != null)
            videoBackground.SetVolume(value);
    }

    void ExitToStart()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
            GameManager.Instance.LoadStartScene();
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}
