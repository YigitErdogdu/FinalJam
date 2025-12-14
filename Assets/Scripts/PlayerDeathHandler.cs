using UnityEngine;
using StarterAssets;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Death Settings")]
    [Tooltip("Ölüm animasyonu süresi (saniye)")]
    [SerializeField] private float deathAnimationDuration = 2f;
    
    [Tooltip("Ölüm sonrası bekleme süresi (saniye)")]
    [SerializeField] private float deathDelay = 1f;
    
    [Header("Respawn Settings")]
    [Tooltip("Başlangıç pozisyonu (boşsa otomatik kaydedilir)")]
    [SerializeField] private Transform respawnPoint;
    
    private bool isDead = false;
    private Animator animator;
    private DeathEffect deathEffect;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private CharacterController charController;
    private ThirdPersonController controller;
    private PlayerCombat combat;
    private PlayerHealth health;
    private WeaponManager weaponManager;
    private bool positionSaved = false;
    
    void Awake()
    {
        // Awake'te pozisyonu kaydet (en erken)
        if (!positionSaved)
        {
            startPosition = transform.position;
            startRotation = transform.rotation;
            positionSaved = true;
            Debug.Log($"✅ Player başlangıç pozisyonu AWAKEDE kaydedildi: {startPosition}");
        }
    }
    
    void Start()
    {
        animator = GetComponent<Animator>();
        deathEffect = GetComponent<DeathEffect>();
        charController = GetComponent<CharacterController>();
        controller = GetComponent<ThirdPersonController>();
        combat = GetComponent<PlayerCombat>();
        health = GetComponent<PlayerHealth>();
        weaponManager = GetComponent<WeaponManager>();
        
        // Eğer respawn point atanmışsa onu kullan
        if (respawnPoint != null)
        {
            startPosition = respawnPoint.position;
            startRotation = respawnPoint.rotation;
            Debug.Log($"✅ Respawn Point kullanılıyor: {startPosition}");
        }
        else if (!positionSaved)
        {
            // Başlangıç pozisyonunu kaydet (oyun başladıktan sonra - daha güvenilir)
            startPosition = transform.position;
            startRotation = transform.rotation;
            positionSaved = true;
            Debug.Log($"✅ Player başlangıç pozisyonu START'TA kaydedildi: {startPosition}");
        }
    }
    
    public void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        Debug.Log("Oyuncu öldü!");
        
        // Ölüm animasyonu oynat
        if (animator != null)
        {
            animator.SetTrigger("Death");
            
            // Animasyon süresini al (eğer Death state'i varsa)
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Death") || stateInfo.IsName("sword and shield death"))
            {
                deathAnimationDuration = stateInfo.length;
            }
        }
        
        // Ölüm efekti oynat
        if (deathEffect != null)
        {
            deathEffect.PlayDeathEffect(transform.position);
            deathAnimationDuration = deathEffect.GetDeathAnimationDuration();
        }
        
        // Hareketi durdur (class seviyesindeki controller'ı kullan)
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        // Saldırıyı durdur (class seviyesindeki combat'ı kullan)
        if (combat != null)
        {
            combat.enabled = false;
        }
        
        // Collider'ı kapat (class seviyesindeki charController'ı kullan)
        if (charController != null)
        {
            charController.enabled = false;
        }
        
        // Ölüm animasyonu bitene kadar bekle, sonra restart
        float totalDelay = deathAnimationDuration + deathDelay;
        if (totalDelay > 0)
        {
            Invoke(nameof(OnDeathComplete), totalDelay);
        }
        else
        {
            // Eğer süre 0 ise direkt resetle
            OnDeathComplete();
        }
    }
    
    private void OnDeathComplete()
    {
        Debug.Log("Ölüm animasyonu tamamlandı! Oyuncu başlangıç noktasına dönüyor...");
        
        // Oyuncuyu resetle
        ResetPlayer();
    }
    
    private void ResetPlayer()
    {
        // Eğer GameObject destroy edilmişse çık
        if (this == null || gameObject == null) return;
        
        Debug.Log($"🔴 Oyuncu resetleniyor... Başlangıç pozisyonu: {startPosition}, Şu anki pozisyon: {transform.position}");
        
        // ÖNCE tüm component'leri kapat
        if (charController != null)
        {
            charController.enabled = false;
        }
        
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        if (combat != null)
        {
            combat.enabled = false;
        }
        
        // Canı doldur ve resetle (önce)
        if (health != null)
        {
            health.ResetHealth();
        }
        
        // Silahları resetle
        if (weaponManager != null)
        {
            weaponManager.ResetWeapons();
        }
        
        // Animator'ı resetle (önce)
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
        
        // POZİSYONU RESETLE - CharacterController kapalıyken direkt ayarla
        if (transform != null)
        {
            // Pozisyonu direkt ayarla
            transform.position = startPosition;
            transform.rotation = startRotation;
            
            // Rigidbody varsa pozisyonu zorla ayarla
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.MovePosition(startPosition);
                rb.MoveRotation(startRotation);
            }
            
            Debug.Log($"📍 Pozisyon ayarlandı: {transform.position}");
        }
        
        // Birkaç frame bekle ve component'leri tekrar aktif et
        StartCoroutine(EnableComponentsAfterFrames());
    }
    
    private System.Collections.IEnumerator EnableComponentsAfterFrames()
    {
        // İlk frame bekle
        yield return null;
        
        // GameObject hala aktif mi kontrol et
        if (this == null || gameObject == null || !gameObject.activeInHierarchy)
        {
            yield break;
        }
        
        // POZİSYONU TEKRAR AYARLA (CharacterController hala kapalı)
        if (transform != null)
        {
            transform.position = startPosition;
            transform.rotation = startRotation;
        }
        
        // Bir frame daha bekle
        yield return null;
        
        // Pozisyonu tekrar kontrol et ve gerekirse ayarla
        if (transform != null && Vector3.Distance(transform.position, startPosition) > 0.1f)
        {
            Debug.LogWarning($"⚠️ Pozisyon hala yanlış! ({transform.position} != {startPosition}) Tekrar ayarlanıyor...");
            if (charController != null)
            {
                charController.enabled = false;
            }
            transform.position = startPosition;
            transform.rotation = startRotation;
            
            // Rigidbody varsa zorla ayarla
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.MovePosition(startPosition);
                rb.MoveRotation(startRotation);
            }
            
            yield return null; // Bir frame daha bekle
        }
        
        // Şimdi component'leri tekrar aktif et
        if (charController != null)
        {
            charController.enabled = true;
        }
        
        if (controller != null)
        {
            controller.enabled = true;
        }
        
        if (combat != null)
        {
            combat.enabled = true;
        }
        
        // Son kontrol - pozisyon doğru mu? (3 frame sonra)
        yield return null;
        yield return null;
        yield return null;
        
        if (transform != null)
        {
            float distance = Vector3.Distance(transform.position, startPosition);
            if (distance > 0.5f)
            {
                Debug.LogError($"❌ POZİSYON HALA YANLIŞ! Mesafe: {distance}. SON DENEME...");
                if (charController != null)
                {
                    charController.enabled = false;
                }
                transform.position = startPosition;
                transform.rotation = startRotation;
                
                // Rigidbody varsa zorla ayarla
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.position = startPosition;
                    rb.rotation = startRotation;
                }
                
                yield return null;
                
                if (charController != null)
                {
                    charController.enabled = true;
                }
            }
            else
            {
                Debug.Log($"✅ Pozisyon doğru! Mesafe: {distance}");
            }
        }
        
        isDead = false;
        Debug.Log($"✅✅✅ Oyuncu başarıyla resetlendi! Final pozisyon: {transform.position}, Hedef: {startPosition}");
    }
    
    public bool IsDead()
    {
        return isDead;
    }
    
    void OnDestroy()
    {
        // Invoke'ları iptal et
        CancelInvoke();
        StopAllCoroutines();
    }
    
    void OnDisable()
    {
        // Invoke'ları iptal et
        CancelInvoke();
        StopAllCoroutines();
    }
}

