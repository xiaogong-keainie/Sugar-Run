using UnityEngine;

public class PotionItem : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        var pc = other.GetComponent<PlayerController>();
        if (pc != null && pc.AddPotion())
            Destroy(gameObject);
    }
}
