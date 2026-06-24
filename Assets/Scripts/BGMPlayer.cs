using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    [Header("Drag your BGM audio clip here")]
    public AudioClip bgmClip;

    void Start()
    {
        if (BGMManager.Instance == null)
        {
            var go = new GameObject("BGMManager");
            go.AddComponent<BGMManager>();
        }

        if (BGMManager.Instance != null)
            BGMManager.Instance.Play(bgmClip);
    }
}
