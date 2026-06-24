using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Level Scenes (in order)")]
    public string[] levelScenes = { "gamesense1", "gamesense2", "gamesense3" };

    [Header("UI Scenes")]
    public string startScene = "StartScene";
    public string endScene = "EndScene";

    [Header("Volume")]
    [Range(0f, 1f)] public float volume = 1f;

    private int currentLevelIndex;
    private bool transitioning;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        volume = PlayerPrefs.GetFloat("Volume", 1f);
    }

    /// <summary>Called by TrophyItem when the player picks up a trophy.</summary>
    public void OnTrophyCollected()
    {
        if (transitioning) return;
        transitioning = true;
        SceneManager.LoadScene(endScene);
    }

    public void LoadStartScene()
    {
        transitioning = false;
        SceneManager.LoadScene(startScene);
    }

    public void LoadLevel(int index)
    {
        if (index < 0 || index >= levelScenes.Length) return;
        currentLevelIndex = index;
        transitioning = false;
        SceneManager.LoadScene(levelScenes[index]);
    }

    public void LoadEndScene()
    {
        transitioning = false;
        SceneManager.LoadScene(endScene);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat("Volume", volume);
    }

    public float GetVolume()
    {
        return volume;
    }

    public int GetCurrentLevelIndex()
    {
        return currentLevelIndex;
    }
}
