using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Oyuncunun maksimum canı")]
    [SerializeField] private float maxHealth = 100f;
    
    [Tooltip("Hasar alma sonrası geçici hasar almama süresi (saniye)")]
    [SerializeField] private float invincibilityDuration = 1f;
    
    [Header("Respawn Settings")]
    [Tooltip("Ölümden sonra respawn süresi (saniye)")]
    [SerializeField] private float respawnDelay = 3f;
    
    private float currentHealth;
    private float lastDamageTime = -999f;
    private bool isDead = false;
    
    // Spawn pozisyonu
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private bool spawnPositionSaved = false;
    
    // Events
    public System.Action<float, float> OnHealthChanged; // (currentHealth, maxHealth)
    public System.Action OnDeath;
    
    void Awake()
    {
        // Başlangıç pozisyonunu kaydet (en erken)
        if (!spawnPositionSaved)
        {
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
            spawnPositionSaved = true;
            Debug.Log($"✅ Player spawn pozisyonu kaydedildi: {spawnPosition}");
        }
    }
    
    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Spawn pozisyonunu tekrar kontrol et
        if (!spawnPositionSaved)
        {
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
            spawnPositionSaved = true;
            Debug.Log($"✅ Player spawn pozisyonu START'ta kaydedildi: {spawnPosition}");
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        // Invincibility kontrolü
        if (Time.time < lastDamageTime + invincibilityDuration)
        {
            return; // Hasar alma
        }
        
        lastDamageTime = Time.time;
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"Oyuncu hasar aldı! Kalan can: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        currentHealth = 0;
        
        Debug.Log($"💀 Oyuncu öldü! {respawnDelay} saniye sonra respawn olacak...");
        
        OnDeath?.Invoke();
        
        // Ölüm animasyonu (eğer Death trigger'ı varsa)
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            // Death trigger'ı var mı kontrol et
            bool hasDeathTrigger = false;
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == "Death" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasDeathTrigger = true;
                    break;
                }
            }
            
            if (hasDeathTrigger)
            {
                animator.SetTrigger("Death");
            }
            else
            {
                Debug.LogWarning("⚠️ Animator'da 'Death' trigger'ı yok! Ölüm animasyonu oynatılamadı.");
            }
        }
        
        // Respawn'ı başlat
        Invoke(nameof(Respawn), respawnDelay);
    }
    
    public bool IsDead()
    {
        return isDead;
    }
    
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    public float GetMaxHealth()
    {
        return maxHealth;
    }
    
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        lastDamageTime = -999f;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log("✅ PlayerHealth resetlendi!");
    }
    
    private void Respawn()
    {
        if (this == null || gameObject == null) return;
        
        Debug.Log($"🔄 Player respawn başlıyor... Spawn pozisyonu: {spawnPosition}");
        
        // CharacterController'ı geçici olarak kapat
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        // Pozisyonu ve rotasyonu resetle
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
        
        // Canı doldur
        ResetHealth();
        
        // Silahları resetle
        WeaponManager weaponManager = GetComponent<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.ResetWeapons();
        }
        
        // Animator'ı resetle
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
        
        // CharacterController'ı tekrar aç
        if (controller != null)
        {
            controller.enabled = true;
        }
        
        Debug.Log($"✅ Player respawn tamamlandı! Pozisyon: {transform.position}");
    }
    
    void OnDestroy()
    {
        CancelInvoke();
    }
    
    void OnDisable()
    {
        CancelInvoke();
    }
}

