using System.Collections;
using UnityEngine;

public class TrophyItem : MonoBehaviour
{
    [Header("Transition Delay")]
    public float transitionDelay = 1.5f;

    private bool collected;

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

        if (collected) return;
        collected = true;

        // Deactivate trophy so it can't be collected again
        gameObject.SetActive(false);

        // Show victory feedback briefly
        StatusUI status = FindObjectOfType<StatusUI>();
        if (status != null)
            status.ShowVictory();

        StartCoroutine(TransitionAfterDelay());
    }

    IEnumerator TransitionAfterDelay()
    {
        yield return new WaitForSeconds(transitionDelay);

        if (GameManager.Instance != null)
            GameManager.Instance.OnTrophyCollected();
    }
}
