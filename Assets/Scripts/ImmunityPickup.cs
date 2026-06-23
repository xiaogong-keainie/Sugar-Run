using UnityEngine;

public class ImmunityPickup : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        var immunity = other.GetComponent<PlayerImmunity>();
        if (immunity == null)
            immunity = other.gameObject.AddComponent<PlayerImmunity>();

        immunity.Activate();

        gameObject.SetActive(false);
    }
}
