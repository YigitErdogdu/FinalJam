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
        
        // Debug.Log($"💔 Oyuncu {damage} hasar aldı! Kalan can: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    // Enemy tag'ine sahip objelerle çarpışma (Collision)
    void OnCollisionEnter(Collision collision)
    {
        if (IsEnemy(collision.gameObject))
        {
            TakeDamage(collisionDamage);
            // Debug.Log($"💥 Enemy ile çarpışma! Hasar: {collisionDamage}");
        }
    }
    
    // Enemy tag'ine sahip objelerle çarpışma (Trigger)
    void OnTriggerEnter(Collider other)
    {
        // Silah, Weapon gibi objeleri ignore et
        if (other.CompareTag("Weapon") || other.name.Contains("Weapon") || other.name.Contains("Sword") || other.name.Contains("Katana"))
        {
            return; // Silahlara hasar verme
        }
        
        if (IsEnemy(other.gameObject))
        {
            TakeDamage(collisionDamage);
            // Debug.Log($"💥 Enemy trigger'a girdi! Hasar: {collisionDamage}");
        }
    }
    
    // Düşman kontrolü - sadece component kontrolü (tag kontrolü yok, çünkü "Enemy" tag'i tanımlı değil)
    private bool IsEnemy(GameObject obj)
    {
        if (obj == null) return false;
        
        // Component kontrolü yap (EnemyAI, BossController gibi düşman component'leri)
        // Bu daha güvenilir çünkü tag'e bağımlı değil
        if (obj.GetComponent<EnemyAI>() != null || 
            obj.GetComponent<BossController>() != null ||
            obj.GetComponentInParent<EnemyAI>() != null ||
            obj.GetComponentInParent<BossController>() != null)
        {
            return true;
        }
        
        return false;
    }
    
    private void Die()
    {
        
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
        // Debug.Log($"✅ Oyuncu canı resetlendi! Can: {currentHealth}/{maxHealth}");
    }
        
    
}

