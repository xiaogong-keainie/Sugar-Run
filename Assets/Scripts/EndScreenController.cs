using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class EndScreenController : MonoBehaviour
{
    [Header("Level Buttons")]
    public Button level1Button;
    public Button level2Button;
    public Button level3Button;

    [Header("Other")]
    public Button backButton;

    void Start()
    {
        if (level1Button != null)
            level1Button.onClick.AddListener(() => GameManager.Instance.LoadLevel(0));
        if (level2Button != null)
            level2Button.onClick.AddListener(() => GameManager.Instance.LoadLevel(1));
        if (level3Button != null)
            level3Button.onClick.AddListener(() => GameManager.Instance.LoadLevel(2));
        if (backButton != null)
            backButton.onClick.AddListener(() => GameManager.Instance.LoadStartScene());
    }
}
