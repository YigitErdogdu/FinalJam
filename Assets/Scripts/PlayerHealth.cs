using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Slider için gerekli kütüphane

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

    [Header("UI Settings")]
    public Slider healthSlider; // BURASI DEĞİŞTİ: Artık Image değil Slider alıyoruz

    void Awake()
    {
        // Gerekirse initialization
    }

    void Start()
    {
        currentHealth = maxHealth;

        // Oyun başladığında Slider ayarlarını yapalım
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth; // Slider'ın maksimum değeri canımız kadar olsun (örn: 100)
            healthSlider.value = currentHealth; // Slider'ı fulleyelim
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // Invincibility kontrolü
        if (Time.time - lastDamageTime < invincibilityDuration)
        {
            return; // Hasar alma süresi dolmadı, hasar verme
        }

        // ÖNCE hasarı düşüyoruz
        currentHealth -= damage;
        lastDamageTime = Time.time;

        // SONRA UI'ı güncelliyoruz (Burası önemli, yer değiştirdi)
        UpdateHealthUI();

        //Debug.Log($"💔 Oyuncu {damage} hasar aldı! Kalan can: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Enemy tag'ine sahip objelerle çarpışma (Trigger)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(collisionDamage);
            //Debug.Log($"💥 Enemy trigger'a girdi! Hasar: {collisionDamage}");
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        currentHealth = 0;
        UpdateHealthUI(); // Ölünce barın tamamen boşaldığından emin olalım

        Debug.Log($"💀 Oyuncu öldü!");

        // Ölüm animasyonu
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Buraya Game Over ekranı veya sahne geçiş kodu gelecek
    }

    // UI Güncelleme Fonksiyonu
    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            // Slider değerini direkt mevcut cana eşitliyoruz
            healthSlider.value = currentHealth;
        }
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
        UpdateHealthUI(); // Resetlenince barı da fulle
        Debug.Log($"✅ Oyuncu canı resetlendi! Can: {currentHealth}/{maxHealth}");
    }
}