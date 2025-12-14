using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Oyuncunun maksimum canı")]
    [SerializeField] private float maxHealth = 100f;
    
    [Tooltip("Hasar alma sonrası geçici hasar almama süresi (saniye)")]
    [SerializeField] private float invincibilityDuration = 1f;
    
    [Tooltip("Enemy tag'ine sahip objelerle çarpışmada alınacak hasar")]
    [SerializeField] private float collisionDamage = 10f;
  
    
    private float currentHealth;
    private bool isDead = false;
    private float lastDamageTime = -999f; // Invincibility için
    

    
    void Awake()
    {
      
    }
    
    void Start()
    {
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        // Invincibility kontrolü
        if (Time.time - lastDamageTime < invincibilityDuration)
        {
            return; // Hasar alma süresi dolmadı, hasar verme
        }
        
        currentHealth -= damage;
        lastDamageTime = Time.time;
        
        Debug.Log($"💔 Oyuncu {damage} hasar aldı! Kalan can: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // Enemy tag'ine sahip objelerle çarpışma (Collision)
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(collisionDamage);
            Debug.Log($"💥 Enemy ile çarpışma! Hasar: {collisionDamage}");
        }
    }
    
    // Enemy tag'ine sahip objelerle çarpışma (Trigger)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(collisionDamage);
            Debug.Log($"💥 Enemy trigger'a girdi! Hasar: {collisionDamage}");
        }
    }
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        currentHealth = 0;
        
        Debug.Log($"💀 Oyuncu öldü!");
        
        // Ölüm animasyonu
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        
        //hedef sahneyi yükle
    }
    
    // Public getter metodları
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
    
    // Canı resetle (respawn için)
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        lastDamageTime = -999f;
        Debug.Log($"✅ Oyuncu canı resetlendi! Can: {currentHealth}/{maxHealth}");
    }
        
    
}

