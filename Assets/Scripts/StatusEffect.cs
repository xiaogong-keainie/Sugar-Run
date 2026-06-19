using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffect : MonoBehaviour
{
    [Header("Stamina Effect")]
    [Tooltip("hunger 10~20: speed 0.8, jump 0.8")]
    public float lowStaminaSpeed = 0.8f;
    public float lowStaminaJump = 0.8f;
    [Tooltip("hunger > 80: speed 0.5, jump 0.5")]
    public float highStaminaSpeed = 0.5f;
    public float highStaminaJump = 0.5f;

    [Header("Near Death (hunger < 10)")]
    [Tooltip("Adrenaline surge: speed 1.5, jump 1.5")]
    public float nearDeathSpeed = 1.5f;
    public float nearDeathJump = 1.5f;

    [Header("High Sugar Effect (> 70)")]
    public float highSugarSpeedMultiplier = 0.85f;
    public float highSugarHpDrain = 1f;
    public float highSugarJumpMultiplier = 0.75f;
    public float highSugarDrainInterval = 2f;

    [Header("Screen Flash & Shake")]
    public Color flashColor = new Color(1f, 0f, 0f, 0.3f);
    public float flashDuration = 1.2f;
    public float shakeIntensity = 0.15f;
    public float shakeFrequency = 0.05f;

    private StatusUI statusUI;
    private PlayerController playerController;
    private CameraFollow cameraFollow;
    private Image flashOverlay;
    private float highSugarTimer;
    private float originalMoveSpeed;
    private float originalJumpForce;
    private Canvas canvas;
    private Coroutine flashRoutine;
    private Coroutine shakeRoutine;

    void Start()
    {
        statusUI = FindObjectOfType<StatusUI>();
        playerController = GetComponent<PlayerController>();
        canvas = FindObjectOfType<Canvas>();
        cameraFollow = FindObjectOfType<CameraFollow>();

        if (playerController != null)
        {
            originalMoveSpeed = playerController.moveSpeed;
            originalJumpForce = playerController.jumpForce;
        }
    }

    void Update()
    {
        if (statusUI == null || playerController == null) return;

        float hunger = statusUI.hunger;
        float sugar = statusUI.sugar;

        float speedMult = 1f;
        float jumpMult = 1f;

        // --- Hunger-based speed/jump modifiers ---
        if (hunger < 10f)
        {
            speedMult *= nearDeathSpeed;
            jumpMult *= nearDeathJump;
        }
        else if (hunger < 20f)
        {
            speedMult *= lowStaminaSpeed;
            jumpMult *= lowStaminaJump;
        }
        else if (hunger > 80f)
        {
            speedMult *= highStaminaSpeed;
            jumpMult *= highStaminaJump;
        }

        bool lowHunger = hunger < 20f;
        bool lowSugar = sugar < 20f;
        bool shouldFlash = lowHunger || lowSugar;

        // --- Low sugar effect: reverse controls ---
        playerController.controlsReversed = lowSugar;

        // --- Screen flash (hunger < 20 or sugar < 20) ---
        if (shouldFlash)
        {
            if (flashOverlay == null && canvas != null)
                CreateFlashOverlay();

            if (flashRoutine == null && flashOverlay != null)
                flashRoutine = StartCoroutine(FlashLoop());
        }
        else
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }

            if (flashOverlay != null && flashOverlay.gameObject.activeSelf)
                flashOverlay.gameObject.SetActive(false);
        }

        // --- Screen shake (hunger < 20) ---
        if (lowHunger)
        {
            if (shakeRoutine == null && cameraFollow != null)
                shakeRoutine = StartCoroutine(ShakeLoop());
        }
        else
        {
            if (shakeRoutine != null)
            {
                StopCoroutine(shakeRoutine);
                shakeRoutine = null;
            }
            if (cameraFollow != null)
                cameraFollow.shakeOffset = Vector3.zero;
        }

        // --- High sugar effect: sugar > 70 ---
        if (sugar > 70f)
        {
            speedMult *= highSugarSpeedMultiplier;
            jumpMult *= highSugarJumpMultiplier;

            highSugarTimer += Time.deltaTime;
            if (highSugarTimer >= highSugarDrainInterval)
            {
                highSugarTimer = 0f;
                statusUI.AddHunger(-highSugarHpDrain);
            }
        }
        else
        {
            highSugarTimer = 0f;
        }

        playerController.moveSpeed = originalMoveSpeed * speedMult;
        playerController.jumpForce = originalJumpForce * jumpMult;
    }

    private void CreateFlashOverlay()
    {
        var go = new GameObject("LowSugarFlash", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(canvas.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        go.transform.SetAsLastSibling();

        flashOverlay = go.GetComponent<Image>();
        flashOverlay.color = flashColor;
        flashOverlay.raycastTarget = false;
        flashOverlay.gameObject.SetActive(false);
    }

    private IEnumerator FlashLoop()
    {
        while (true)
        {
            flashOverlay.gameObject.SetActive(true);
            yield return new WaitForSeconds(flashDuration);
            flashOverlay.gameObject.SetActive(false);
            yield return new WaitForSeconds(flashDuration);
        }
    }

    private IEnumerator ShakeLoop()
    {
        while (true)
        {
            cameraFollow.shakeOffset = (Vector3)Random.insideUnitCircle * shakeIntensity;
            yield return new WaitForSeconds(shakeFrequency);
        }
    }

    void OnDestroy()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }
    }
}
