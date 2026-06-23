using UnityEngine;

public class PlayerImmunity : MonoBehaviour
{
    public float duration = 5f;
    public Color auraColor = new Color(0f, 0.8f, 1f, 0.6f);
    public float auraScale = 2.5f;

    float timer;
    bool isActive;
    GameObject aura;
    StatusUI statusUI;

    void Awake()
    {
        statusUI = FindObjectOfType<StatusUI>();
        CreateAura();
    }

    public void Activate()
    {
        timer = duration;
        isActive = true;

        if (statusUI != null)
            statusUI.pauseDepletion = true;

        if (aura != null)
            aura.SetActive(true);
    }

    void Update()
    {
        if (!isActive) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
            Deactivate();
    }

    void Deactivate()
    {
        isActive = false;
        if (statusUI != null)
            statusUI.pauseDepletion = false;
        if (aura != null)
            aura.SetActive(false);
    }

    float pulseTimer;

    void CreateAura()
    {
        aura = new GameObject("ImmunityAura");
        aura.transform.SetParent(transform, false);
        aura.transform.localPosition = Vector3.zero;

        var sr = aura.AddComponent<SpriteRenderer>();
        sr.sprite = GenerateAuraSprite();
        sr.color = auraColor;
        sr.sortingOrder = -1;

        aura.transform.localScale = Vector3.one * auraScale;
        aura.SetActive(false);
    }

    void LateUpdate()
    {
        if (!isActive || aura == null) return;

        pulseTimer += Time.deltaTime * 3f;
        float pulse = Mathf.Sin(pulseTimer) * 0.15f + 1f;
        aura.transform.localScale = Vector3.one * auraScale * pulse;
    }

    Sprite GenerateAuraSprite()
    {
        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float half = size * 0.5f;
        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - half) / half;
                float dy = (y - half) / half;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Outer bright ring (sharp edge)
                float outerRing = 1f - Mathf.Abs(dist - 0.92f) / 0.05f;
                outerRing = Mathf.Clamp01(outerRing);

                // Inner glow ring
                float innerRing = 1f - Mathf.Abs(dist - 0.75f) / 0.12f;
                innerRing = Mathf.Clamp01(innerRing) * 0.5f;

                // Soft fill
                float fill = Mathf.Clamp01(1f - dist / 0.65f) * 0.25f;

                byte alpha = (byte)(Mathf.Clamp01(Mathf.Max(outerRing, innerRing, fill)) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
    }

    void OnDestroy()
    {
        if (isActive && statusUI != null)
            statusUI.pauseDepletion = false;
    }
}
