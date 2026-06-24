using UnityEngine;

public class OneTimePickup : MonoBehaviour
{
    void Awake()
    {
        if (GetComponent<Collider2D>() == null)
        {
            var col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1, 1);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        gameObject.SetActive(false);
    }
}
