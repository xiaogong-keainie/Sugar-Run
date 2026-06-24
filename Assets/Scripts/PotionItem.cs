using System.Collections;
using UnityEngine;

public class PotionItem : MonoBehaviour
{
    public enum PotionType { Blue, Green }
    public PotionType potionType;

    [Header("Audio")]
    public AudioClip pickupSound;
    public float volume = 1f;

    [Header("Green Potion")]
    public float freezeDuration = 5f;

    private static AudioSource audioSource;

    void Awake()
    {
        if (audioSource == null)
        {
            var go = new GameObject("PotionAudioSource");
            audioSource = go.AddComponent<AudioSource>();
            DontDestroyOnLoad(go);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        var pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        if (potionType == PotionType.Blue)
        {
            if (pc.AddPotion())
            {
                if (pickupSound != null && audioSource != null)
                    audioSource.PlayOneShot(pickupSound, volume);
                gameObject.SetActive(false);
            }
        }
        else // Green — freeze depletion for 5s
        {
            var status = FindObjectOfType<StatusUI>();
            if (status != null)
            {
                if (status.pauseDepletion)
                {
                    // Already frozen, still give a small sugar boost
                    status.AddSugar(10f);
                }
                else
                {
                    status.pauseDepletion = true;
                    status.StartCoroutine(UnfreezeAfterDelay(status, freezeDuration));
                }
            }
            gameObject.SetActive(false);
        }
    }

    private static IEnumerator UnfreezeAfterDelay(StatusUI status, float delay)
    {
        yield return new WaitForSeconds(delay);
        status.pauseDepletion = false;
    }
}
