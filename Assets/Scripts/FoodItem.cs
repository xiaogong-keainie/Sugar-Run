using UnityEngine;

public class FoodItem : MonoBehaviour
{
    [Header("Stats")]
    public float sugarBonus = 10f;
    public float hungerBonus = 10f;

    void Awake()
    {
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        StatusUI status = FindObjectOfType<StatusUI>();
        if (status != null)
        {
            status.AddSugar(sugarBonus);
            status.AddHunger(hungerBonus);
        }

        gameObject.SetActive(false);
    }
}
