using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    [Header("Death Effects")]
    [Tooltip("Ölüm partikül efekti")]
    [SerializeField] private GameObject deathParticleEffect;
    
    [Tooltip("Ölüm ses efekti")]
    [SerializeField] private AudioClip deathSound;
    
    [Tooltip("Ölüm animasyonu süresi (saniye)")]
    [SerializeField] private float deathAnimationDuration = 2f;
    
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    public void PlayDeathEffect(Vector3 position)
    {
        // Partikül efekti oynat
        if (deathParticleEffect != null)
        {
            GameObject effect = Instantiate(deathParticleEffect, position, Quaternion.identity);
            Destroy(effect, 5f); // 5 saniye sonra partikülü yok et
        }
        
        // Ses efekti oynat
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }
    }
    
    public float GetDeathAnimationDuration()
    {
        return deathAnimationDuration;
    }
}

