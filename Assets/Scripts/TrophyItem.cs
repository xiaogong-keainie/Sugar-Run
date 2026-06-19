using UnityEngine;

public class TrophyItem : MonoBehaviour
{
    void Awake()
    {
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2f, 2f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        StatusUI status = FindObjectOfType<StatusUI>();
        if (status != null)
            status.ShowVictory();
    }
}
