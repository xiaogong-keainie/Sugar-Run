using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeathScreenController : MonoBehaviour
{
    public float displayDuration = 2f;
    public float fadeInTime = 0.4f;
    public TMP_FontAsset deathFont;

    private GameObject deathOverlay;
    private Image overlayImage;
    private TextMeshProUGUI deathText;
    private bool showing;

    void Awake()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        deathOverlay = new GameObject("DeathOverlay");
        deathOverlay.transform.SetParent(canvas.transform, false);
        var rt = deathOverlay.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        overlayImage = deathOverlay.AddComponent<Image>();
        overlayImage.color = new Color(0, 0, 0, 0);

        var textGo = new GameObject("DeathText");
        textGo.transform.SetParent(deathOverlay.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0.5f, 0.5f);
        textRt.anchorMax = new Vector2(0.5f, 0.5f);
        textRt.sizeDelta = new Vector2(900, 150);
        textRt.anchoredPosition = Vector2.zero;
        deathText = textGo.AddComponent<TextMeshProUGUI>();
        deathText.text = "YOU DIED! !";
        deathText.fontSize = 196;
        deathText.alignment = TextAlignmentOptions.Center;
        deathText.color = new Color(0.7f, 0, 0, 0);
        deathText.fontStyle = FontStyles.Bold;
        if (deathFont != null)
            deathText.font = deathFont;
        else
            deathText.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Bangers SDF");
        deathText.fontMaterial = Resources.Load<Material>("Fonts & Materials/Bangers SDF - Outline");

        deathOverlay.SetActive(false);
    }

    public void ShowDeathScreen(System.Action onComplete)
    {
        if (showing) return;
        showing = true;
        StartCoroutine(DeathSequence(onComplete));
    }

    IEnumerator DeathSequence(System.Action onComplete)
    {
        deathOverlay.SetActive(true);

        // Freeze player movement
        Time.timeScale = 0f;

        float elapsed = 0f;
        while (elapsed < fadeInTime)
        {
            float t = elapsed / fadeInTime;
            overlayImage.color = new Color(0, 0, 0, t * 0.9f);
            deathText.color = new Color(0.7f, 0, 0, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        overlayImage.color = new Color(0, 0, 0, 0.8f);
        deathText.color = new Color(0.7f, 0, 0, 1);

        // Hold
        yield return new WaitForSecondsRealtime(displayDuration);

        // Fade out
        elapsed = 0f;
        float fadeOut = fadeInTime;
        while (elapsed < fadeOut)
        {
            float t = 1f - (elapsed / fadeOut);
            overlayImage.color = new Color(0, 0, 0, t * 0.8f);
            deathText.color = new Color(0.7f, 0, 0, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        deathOverlay.SetActive(false);
        Time.timeScale = 1f;
        showing = false;

        onComplete?.Invoke();
    }
}
