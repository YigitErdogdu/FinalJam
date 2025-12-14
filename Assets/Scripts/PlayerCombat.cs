using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    [Header("Combat Settings")]
    [Tooltip("Saldırılar arası bekleme süresi (saniye)")]
    [SerializeField] private float attackCooldown = 1f;
    
    [Tooltip("Saldırı menzili")]
    [SerializeField] private float attackRange = 5f; // Artırıldı: 2m → 5m
    
    [Tooltip("Saldırı noktası (silahın ucu)")]
    [SerializeField] private Transform attackPoint;
    
    private WeaponManager weaponManager;
    private int currentAttackIndex = 0;
    public bool isAttacking;
    private float lastAttackTime = -999f;


    public static PlayerCombat instance;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Eğer Inspector'dan atanmamışsa, otomatik olarak bul
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // WeaponManager'ı bul
        weaponManager = GetComponent<WeaponManager>();
        if (weaponManager == null)
        {
            Debug.LogWarning("WeaponManager bulunamadı! Player'a WeaponManager script'i ekleyin.");
        }

        // AttackPoint yoksa, karakterin kendisini kullan
        if (attackPoint == null)
        {
            attackPoint = transform;
        }
        
        // Sadece arena sahnesinde combat aktif olsun
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (!currentScene.Contains("Arena") && !currentScene.Contains("Level2") && !currentScene.Contains("Level3"))
        {
            // Arena sahnesi değilse, bu script'i deaktif et
            this.enabled = false;
            Debug.Log($"PlayerCombat deaktif edildi. Sahne: {currentScene}");
        }
    }

    void Update()
    {
        // Ölüyse saldırma
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.IsDead())
        {
            return;
        }
        
        // Sol fare tuşuna basıldığında
        // Cooldown kontrolü eklendi
        bool canAttack = Time.time >= lastAttackTime + attackCooldown;
        if (Input.GetMouseButtonDown(0) && !isAttacking && canAttack)
        {
            StartCoroutine(Attack());
        }
    }


    IEnumerator Attack()
    {
        if (animator == null)
        {
            isAttacking = false;
            yield break;
        }
        
        isAttacking = true;
        lastAttackTime = Time.time; // Saldırı zamanını kaydet
        
        // AttackIndex float değerini ayarla
        animator.SetFloat("AttackIndex", currentAttackIndex);
        
        // Attack trigger'ını tetikle
        animator.SetTrigger("Attack");
        
        // Sıradaki animasyon için index'i artır (0 -> 1 -> 2 -> 0)
        currentAttackIndex = (currentAttackIndex + 1) % 3;
        
        // Root Motion'ı KAPAT - saldırı sırasında normal hareket devam etsin
        animator.applyRootMotion = false;
        
        // Animasyon state'inin değişmesini bekle
        yield return null;
        
        // Şimdi doğru animasyon süresini al
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;
        float elapsedTime = 0f;
        
        // Animasyonun yarısında hasar ver (silah vuruş anı)
        while (elapsedTime < animationLength * 0.5f)
        {
            if (animator == null)
            {
                isAttacking = false;
                yield break;
            }
            
            // Roll yapıldı mı kontrol et
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Player roll"))
            {
                // Roll yapıldı, attack'ı iptal et
                isAttacking = false;
                animator.applyRootMotion = false;
                yield break;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
  
        
        // Animasyonun geri kalanını bekle
        elapsedTime = animationLength * 0.5f;
        while (elapsedTime < animationLength)
        {
            if (animator == null)
            {
                isAttacking = false;
                yield break;
            }
            
            // Roll yapıldı mı kontrol et
            if (animator.GetCurrentAnimatorStateInfo(0).IsName("Player roll"))
            {
                // Roll yapıldı, attack'ı iptal et
                isAttacking = false;
                animator.applyRootMotion = false;
                yield break;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Saldırı bitti
        isAttacking = false;
        // Root motion zaten kapalı, tekrar kapatmaya gerek yok
    }

    

    // Debug için saldırı menzilini göster
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
