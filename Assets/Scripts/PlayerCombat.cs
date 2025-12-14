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
    private bool isAttacking;
    private float lastAttackTime = -999f;

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
        
        // Hasar ver
        DealDamage();
        
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

    void DealDamage()
    {
        // Silahın hasarını al
        float damage = weaponManager != null ? weaponManager.GetCurrentWeaponDamage() : 5f;
        
        // AttackPoint kontrolü
        Vector3 attackPosition = attackPoint != null ? attackPoint.position : transform.position;
        
        Debug.Log($"⚔️ Saldırı başlatıldı! Hasar: {damage}, Pozisyon: {attackPosition}, Menzil: {attackRange}");
        
        // Önce tüm BossController'ları bul (mesafe kontrolü yaparak)
        BossController[] allBosses = FindObjectsOfType<BossController>();
        bool bossHit = false;
        
        Debug.Log($"🔍 Sahnedeki toplam boss sayısı: {allBosses.Length}");
        
        foreach (BossController boss in allBosses)
        {
            if (boss == null) continue;
            
            // Boss'un pozisyonunu al (collider merkezi veya transform)
            Vector3 bossPosition = boss.transform.position;
            
            // Boss'un collider'ını bul (daha doğru mesafe için)
            Collider bossCollider = boss.GetComponent<Collider>();
            if (bossCollider != null)
            {
                bossPosition = bossCollider.bounds.center;
            }
            
            float distanceToBoss = Vector3.Distance(attackPosition, bossPosition);
            Debug.Log($"🔍 Boss kontrolü: {boss.name} | " +
                     $"Attack Pos: {attackPosition} | " +
                     $"Boss Pos: {bossPosition} | " +
                     $"Mesafe: {distanceToBoss:F2}m | " +
                     $"Menzil: {attackRange}m | " +
                     $"Hasar: {damage}");
            
            if (distanceToBoss <= attackRange)
            {
                boss.TakeDamage(damage);
                Debug.Log($"✅✅✅ Boss'a {damage} hasar verildi! Mesafe: {distanceToBoss:F2}m");
                bossHit = true;
                break; // Bir boss'a hasar verildi, diğerlerine geçme
            }
            else
            {
                Debug.LogWarning($"⚠️ Boss çok uzak! Mesafe: {distanceToBoss:F2}m > Menzil: {attackRange}m");
            }
        }
        
        // Saldırı menzilindeki tüm düşmanları bul (diğer düşmanlar için)
        Collider[] hitEnemies = Physics.OverlapSphere(attackPosition, attackRange);
        
        Debug.Log($"🎯 {hitEnemies.Length} obje saldırı menzilinde bulundu!");
        
        foreach (Collider enemy in hitEnemies)
        {
            Debug.Log($"🔍 Kontrol ediliyor: {enemy.name} | Tag: {enemy.tag}");
            
            // Boss kontrolü - OverlapSphere'de de kontrol et (daha güvenilir)
            BossController bossInCollider = enemy.GetComponent<BossController>();
            if (bossInCollider == null)
            {
                // Parent'larda ara
                Transform parent = enemy.transform.parent;
                int depth = 0;
                while (parent != null && depth < 5)
                {
                    bossInCollider = parent.GetComponent<BossController>();
                    if (bossInCollider != null)
                    {
                        Debug.Log($"✅ Boss OverlapSphere'de bulundu: {parent.name} (Collider: {enemy.name})");
                        break;
                    }
                    parent = parent.parent;
                    depth++;
                }
            }
            
            // Boss bulundu mu?
            if (bossInCollider != null && !bossHit)
            {
                float distanceToBoss = Vector3.Distance(attackPosition, enemy.bounds.center);
                Debug.Log($"🎯 Boss OverlapSphere'de! Mesafe: {distanceToBoss:F2}m");
                bossInCollider.TakeDamage(damage);
                Debug.Log($"✅✅✅ Boss'a OverlapSphere ile {damage} hasar verildi!");
                bossHit = true;
                continue; // Boss'a hasar verildi, diğer kontrollere geçme
            }
            
            // Boss zaten kontrol edildi, atla
            if (bossInCollider != null)
            {
                continue;
            }
            
            // Dost robot mu kontrol et (EnemyFollower component'i varsa)
            EnemyFollower friendlyRobot = enemy.GetComponent<EnemyFollower>();
            if (friendlyRobot != null)
            {
                // Dost robota saldırma!
                Debug.Log($"💜 {enemy.name} dost bir robot! Ona saldırmıyoruz.");
                continue; // Bu düşmanı atla, bir sonrakine geç
            }
            
            // SimpleFollower kontrolü (NavMesh olmayan versiyon)
            SimpleFollower simpleFollower = enemy.GetComponent<SimpleFollower>();
            if (simpleFollower != null)
            {
                // Dost robota saldırma!
                Debug.Log($"💜 {enemy.name} dost bir robot! Ona saldırmıyoruz.");
                continue;
            }
            
            // EnemyAI'a sahip düşmanları vurduk mu?
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(damage);
                Debug.Log($"{enemy.name}'e {damage} hasar verildi!");
            }
        }
        
        if (!bossHit)
        {
            Debug.LogWarning($"⚠️ Boss'a hasar verilemedi! Menzil: {attackRange}m, Bulunan obje sayısı: {hitEnemies.Length}");
        }
    }

    // Debug için saldırı menzilini göster
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
