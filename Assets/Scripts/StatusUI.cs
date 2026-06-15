using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusUI : MonoBehaviour
{
    [Header("Bars")]
    public Image hungerFill;
    public Image sugarFill;

    [Header("Value Texts")]
    public TMP_Text hungerValueText;
    public TMP_Text sugarValueText;

    [Header("Values")]
    [Range(0, 100)] public float hunger = 100f;
    [Range(0, 100)] public float sugar = 100f;

    [Header("Potion UI")]
    public TMP_Text potionText;

    [Header("Depletion Rate (per second)")]
    public float hungerRate = 2f;
    public float sugarRate = 1.5f;

    [Header("Max Values")]
    public float maxHunger = 100f;
    public float maxSugar = 100f;

    private bool isDead;

    void Start()
    {
        sugar = 70f;
        UpdateStatusUI();
    }

    void Update()
    {
        if (isDead) return;

        hunger = Mathf.Clamp(hunger - hungerRate * Time.deltaTime, 0f, maxHunger);
        sugar = Mathf.Clamp(sugar - sugarRate * Time.deltaTime, 0f, maxSugar);
        UpdateStatusUI();

        if (hunger <= 0f || sugar <= 0f)
        {
            isDead = true;
            var pc = FindObjectOfType<PlayerController>();
            if (pc != null) pc.Respawn();
        }
    }

    private void UpdateStatusUI()
    {
        if (hungerFill != null)
        {
            hungerFill.type = Image.Type.Filled;
            hungerFill.fillMethod = Image.FillMethod.Horizontal;
            hungerFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            hungerFill.fillAmount = hunger / maxHunger;
        }

        if (sugarFill != null)
        {
            sugarFill.type = Image.Type.Filled;
            sugarFill.fillMethod = Image.FillMethod.Horizontal;
            sugarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            sugarFill.fillAmount = sugar / maxSugar;
        }

        if (hungerValueText != null)
            hungerValueText.text = Mathf.RoundToInt(hunger).ToString();

        if (sugarValueText != null)
            sugarValueText.text = Mathf.RoundToInt(sugar).ToString();
    }

    public void UpdatePotionUI(int count)
    {
        if (potionText != null)
            potionText.text = $" X {count}";
    }

    public void ResetStatus()
    {
        hunger = maxHunger;
        sugar = 70f;
        isDead = false;
        UpdateStatusUI();
    }

    public void AddSugar(float amount)
    {
        sugar = Mathf.Clamp(sugar + amount, 0f, maxSugar);
    }

    public void AddHunger(float amount)
    {
        hunger = Mathf.Clamp(hunger + amount, 0f, maxHunger);
    }
}
