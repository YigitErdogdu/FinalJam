using UnityEngine;
using UnityEngine.UI; // UI için gerekli

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 10;
    public int currentHealth;
     // Canvas'taki HealthBar'ý buraya sürükleyeceðiz

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        

        if (currentHealth <= 0)
        {
            // Ölüm animasyonu veya Game Over iþlemleri burada çaðrýlabilir
            Debug.Log("Oyuncu öldü!");
        }
    }

    void UpdateHealthUI()
    {
        // 10 üzerinden oranlayarak barý azaltýr (Örn: 9/10 = 0.9)
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = (float)currentHealth / maxHealth;
        }
    }
}
