using UnityEngine;

public class DeathZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc != null) pc.Respawn();
        }
    }
}
