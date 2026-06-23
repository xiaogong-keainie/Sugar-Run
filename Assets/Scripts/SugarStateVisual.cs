using UnityEngine;

public class SugarStateVisual : MonoBehaviour
{
    [Header("Sugar Thresholds")]
    public float highSugarThreshold = 70f;
    public float lowSugarThreshold = 20f;

    [Header("High Sugar Aura")]
    public Color highSugarAuraColor = new Color(1f, 0.35f, 0f, 0.6f);
    public float highSugarAuraScale = 3.0f;
    public float highSugarPulseSpeed = 2f;

    [Header("Low Sugar Aura")]
    public Color lowSugarAuraColor = new Color(0.2f, 0.4f, 1f, 0.6f);
    public float lowSugarAuraScale = 3.2f;
    public float lowSugarPulseSpeed = 6f;

    [Header("Sprite Tint")]
    public Color highSugarTint = new Color(1f, 0.8f, 0.6f, 1f);
    public Color lowSugarTint = new Color(0.6f, 0.75f, 1f, 1f);
    public float tintLerpSpeed = 5f;

    private StatusUI statusUI;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private GameObject highSugarAura;
    private GameObject lowSugarAura;
    private Color originalColor;
    private int currentSugarState = 0;
    private Vector3 auraScaleCompensation = Vector3.one;

    static readonly int HashSugarState = Animator.StringToHash("SugarState");

    void Start()
    {
        statusUI = FindObjectOfType<StatusUI>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        // Compensate for parent scale so aura has consistent world-space size
        Vector3 parentScale = transform.lossyScale;
        auraScaleCompensation = new Vector3(
            Mathf.Abs(parentScale.x) > 0.001f ? 1f / parentScale.x : 1f,
            Mathf.Abs(parentScale.y) > 0.001f ? 1f / parentScale.y : 1f,
            Mathf.Abs(parentScale.z) > 0.001f ? 1f / parentScale.z : 1f
        );

        CreateAuraObjects();
    }

    void CreateAuraObjects()
    {
        highSugarAura = CreateAuraObject("HighSugarAura", highSugarAuraColor, highSugarAuraScale);
        lowSugarAura = CreateAuraObject("LowSugarAura", lowSugarAuraColor, lowSugarAuraScale);
    }

    GameObject CreateAuraObject(string auraName, Color color, float scale)
    {
        var go = new GameObject(auraName);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.Scale(Vector3.one * scale, auraScaleCompensation);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GenerateGlowSprite();
        sr.color = color;
        sr.sortingOrder = 10;
        go.SetActive(false);
        return go;
    }

    void Update()
    {
        if (statusUI == null)
            statusUI = FindObjectOfType<StatusUI>();
        if (statusUI == null) return;

        float sugar = statusUI.sugar;
        int newState = 0;

        if (sugar > highSugarThreshold)
            newState = 1;
        else if (sugar < lowSugarThreshold)
            newState = 2;

        if (animator != null && newState != currentSugarState)
        {
            animator.SetInteger(HashSugarState, newState);
            currentSugarState = newState;
        }

        UpdateAuraActiveState(newState);
    }

    void UpdateAuraActiveState(int sugarState)
    {
        bool highActive = sugarState == 1;
        bool lowActive = sugarState == 2;

        if (highSugarAura != null && highSugarAura.activeSelf != highActive)
            highSugarAura.SetActive(highActive);

        if (lowSugarAura != null && lowSugarAura.activeSelf != lowActive)
            lowSugarAura.SetActive(lowActive);
    }

    void LateUpdate()
    {
        // Smooth sprite color transition
        if (spriteRenderer != null)
        {
            Color targetColor = originalColor;
            if (currentSugarState == 1)
                targetColor = highSugarTint;
            else if (currentSugarState == 2)
                targetColor = lowSugarTint;

            spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.deltaTime * tintLerpSpeed);
        }

        // Aura pulsing (compensated for parent scale)
        if (highSugarAura != null && highSugarAura.activeSelf)
        {
            float pulse = Mathf.Sin(Time.time * highSugarPulseSpeed) * 0.12f + 1f;
            highSugarAura.transform.localScale = Vector3.Scale(Vector3.one * highSugarAuraScale * pulse, auraScaleCompensation);
        }

        if (lowSugarAura != null && lowSugarAura.activeSelf)
        {
            float pulse = Mathf.Sin(Time.time * lowSugarPulseSpeed) * 0.18f + 1f;
            lowSugarAura.transform.localScale = Vector3.Scale(Vector3.one * lowSugarAuraScale * pulse, auraScaleCompensation);
        }
    }

    Sprite GenerateGlowSprite()
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

                // Soft radial glow
                float glow = Mathf.Clamp01(1f - dist / 0.9f);
                glow = Mathf.Pow(glow, 1.5f);

                byte alpha = (byte)(glow * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
    }

    void OnDestroy()
    {
        if (highSugarAura != null) Destroy(highSugarAura);
        if (lowSugarAura != null) Destroy(lowSugarAura);
    }
}
