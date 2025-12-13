using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;

    [Header("Detection Settings")]
    [Tooltip("Oyuncuyu algılama mesafesi")]
    [SerializeField] private float detectionRange = 15f;
    
    [Tooltip("Saldırı mesafesi")]
    [SerializeField] private float attackRange = 3f;

    [Header("Attack Settings")]
    [Tooltip("Saldırılar arası bekleme süresi")]
    [SerializeField] private float attackCooldown = 4f;
    
    [Tooltip("Saldırı animasyon süresi")]
    [SerializeField] private float attackDuration = 1.5f;
    
    [Tooltip("Boss'un verdiği hasar")]
    [SerializeField] private float attackDamage = 20f;

    [Header("Movement Settings")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Health Settings")]
    [Tooltip("Boss'un maksimum canı")]
    [SerializeField] private float maxHealth = 100f;
    
    private float currentHealth;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool positionSaved = false;

    // State
    private enum BossState { Idle, Chase, Attack }
    private BossState currentState = BossState.Idle;
    
    // Attack timing
    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    private bool isDead = false;

    // Animation IDs
    private int animIDWalk;
    private int animIDAttack;

    void Awake()
    {
        // Awake'te pozisyonu kaydet (en erken)
        if (!positionSaved)
        {
            startPosition = transform.position;
            startRotation = transform.rotation;
            positionSaved = true;
            Debug.Log($"✅ Boss başlangıç pozisyonu AWAKEDE kaydedildi: {startPosition}");
        }
    }

    void Start()
    {
        // Otomatik referans bulma
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (animator == null)
            animator = GetComponent<Animator>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        // NavMeshAgent ayarları
        if (agent != null)
        {
            agent.speed = chaseSpeed;
            agent.stoppingDistance = attackRange - 0.5f;
        }

        // Animation ID'lerini ayarla
        animIDWalk = Animator.StringToHash("Walk");
        animIDAttack = Animator.StringToHash("Attack");

        // Can sistemini başlat
        currentHealth = maxHealth;
        
        // Başlangıç pozisyonunu tekrar kontrol et
        if (!positionSaved)
        {
            startPosition = transform.position;
            startRotation = transform.rotation;
            positionSaved = true;
            Debug.Log($"✅ Boss başlangıç pozisyonu START'TA kaydedildi: {startPosition}");
        }
    }

    void Update()
    {
        if (player == null || isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // State Machine
        switch (currentState)
        {
            case BossState.Idle:
                HandleIdleState(distanceToPlayer);
                break;

            case BossState.Chase:
                HandleChaseState(distanceToPlayer);
                break;

            case BossState.Attack:
                HandleAttackState(distanceToPlayer);
                break;
        }

        // Animator güncelleme
        UpdateAnimator();
    }

    private void HandleIdleState(float distanceToPlayer)
    {
        // Oyuncu algılama alanına girdi mi?
        if (distanceToPlayer <= detectionRange)
        {
            currentState = BossState.Chase;
        }

        // Idle animasyonu için hız 0
        if (agent != null)
            agent.isStopped = true;
    }

    private void HandleChaseState(float distanceToPlayer)
    {
        // Oyuncu çok uzaklaştı mı?
        if (distanceToPlayer > detectionRange + 5f)
        {
            currentState = BossState.Idle;
            if (agent != null)
                agent.isStopped = true;
            return;
        }

        // Saldırı mesafesine girdi mi?
        if (distanceToPlayer <= attackRange)
        {
            currentState = BossState.Attack;
            if (agent != null)
                agent.isStopped = true;
            return;
        }

        // Oyuncuyu takip et
        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        // Oyuncuya doğru dön
        RotateTowardsPlayer();
    }

    private void HandleAttackState(float distanceToPlayer)
    {
        // Oyuncu saldırı alanından çıktı mı?
        if (distanceToPlayer > attackRange + 1f)
        {
            currentState = BossState.Chase;
            return;
        }

        // Oyuncuya doğru dön
        RotateTowardsPlayer();

        // Saldırı cooldown'ı bitti mi ve şu an saldırmıyor mu?
        if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
        {
            StartCoroutine(PerformAttack());
        }

        // Saldırı sırasında hareket etme
        if (agent != null)
            agent.isStopped = true;
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Saldırı animasyonunu tetikle
        if (animator != null)
        {
            animator.SetTrigger(animIDAttack);
        }

        // Saldırı animasyonunun yarısında hasar ver (vuruş anı)
        yield return new WaitForSeconds(attackDuration * 0.5f);
        
        // Oyuncuya hasar ver
        DealDamageToPlayer();

        // Saldırı animasyonunun geri kalanını bekle
        yield return new WaitForSeconds(attackDuration * 0.5f);

        isAttacking = false;
    }
    
    private void DealDamageToPlayer()
    {
        if (player == null)
        {
            Debug.LogWarning("Boss: Player null!");
            return;
        }
        
        // Oyuncu saldırı menzilinde mi?
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange + 1f) // Daha fazla tolerans
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth == null)
            {
                // PlayerHealth yoksa otomatik ekle
                playerHealth = player.gameObject.AddComponent<PlayerHealth>();
                Debug.Log("✅ Boss: PlayerHealth bulunamadı, otomatik eklendi!");
            }
            
            if (!playerHealth.IsDead())
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"✅ Boss oyuncuya {attackDamage} hasar verdi! Kalan can: {playerHealth.GetCurrentHealth()}/{playerHealth.GetMaxHealth()}");
            }
        }
    }

    private void RotateTowardsPlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Sadece yatay düzlemde dön

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        // Walk animasyonu - Sadece Chase state'inde yürüyor
        bool isWalking = (currentState == BossState.Chase);
        animator.SetBool(animIDWalk, isWalking);
    }

    // Hasar alma fonksiyonu - Dışarıdan çağrılabilir
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Boss hasar aldı! Kalan can: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        currentHealth = 0;

        Debug.Log("Boss öldü! 3 saniye sonra resetlenecek...");

        // NavMeshAgent'ı durdur
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Ölüm animasyonu (eğer varsa)
        float deathAnimDuration = 2f;
        if (animator != null)
        {
            animator.SetTrigger("Death"); // Animator'da "Death" trigger'ı olmalı
            
            // Animasyon süresini al (eğer Death state'i varsa)
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Death"))
            {
                deathAnimDuration = stateInfo.length;
            }
        }

        // Ölüm efekti oynat
        DeathEffect deathEffect = GetComponent<DeathEffect>();
        if (deathEffect != null)
        {
            deathEffect.PlayDeathEffect(transform.position);
            deathAnimDuration = deathEffect.GetDeathAnimationDuration();
        }

        // Collider'ı kapat
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Ölüm animasyonu bitene kadar bekle, sonra resetle
        float totalDelay = deathAnimDuration + 1f;
        if (totalDelay > 0 && this != null && gameObject != null)
        {
            Invoke(nameof(ResetBoss), totalDelay);
        }
        else
        {
            // Eğer süre 0 ise veya GameObject yoksa direkt resetle
            ResetBoss();
        }
    }
    
    private void ResetBoss()
    {
        // Eğer GameObject destroy edilmişse çık
        if (this == null || gameObject == null) return;
        
        Debug.Log($"🔴 Boss resetleniyor... Başlangıç pozisyonu: {startPosition}, Şu anki pozisyon: {transform.position}");
        
        // Component'leri kapat
        if (agent != null)
        {
            agent.enabled = false;
        }
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Pozisyonu ve rotasyonu resetle
        if (transform != null)
        {
            transform.position = startPosition;
            transform.rotation = startRotation;
            Debug.Log($"📍 Boss pozisyonu ayarlandı: {transform.position}");
        }
        
        // Canı doldur ve state'i resetle
        currentHealth = maxHealth;
        isDead = false;
        isAttacking = false;
        lastAttackTime = -999f;
        currentState = BossState.Idle;
        
        // Animator'ı resetle
        if (animator != null)
        {
            animator.Rebind(); // Tüm animasyonları resetle
            animator.Update(0f); // Hemen güncelle
        }
        
        // Bir frame bekle ve NavMeshAgent'ı ayarla
        StartCoroutine(EnableBossComponentsAfterFrame(col));
    }
    
    private System.Collections.IEnumerator EnableBossComponentsAfterFrame(Collider col)
    {
        // Bir frame bekle
        yield return null;
        
        // GameObject hala aktif mi kontrol et
        if (this == null || gameObject == null || !gameObject.activeInHierarchy)
        {
            yield break;
        }
        
        // Pozisyonu tekrar kontrol et
        if (transform != null && Vector3.Distance(transform.position, startPosition) > 0.1f)
        {
            Debug.LogWarning($"⚠️ Boss pozisyonu hala yanlış! Tekrar ayarlanıyor...");
            if (agent != null)
            {
                agent.enabled = false;
            }
            transform.position = startPosition;
            transform.rotation = startRotation;
            yield return null;
        }
        
        // Component'leri tekrar aktif et
        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            
            // NavMeshAgent için Warp kullan (eğer NavMesh'teyse)
            if (agent.isOnNavMesh)
            {
                agent.Warp(startPosition);
            }
        }
        
        if (col != null)
        {
            col.enabled = true;
        }
        
        // Son kontrol
        yield return null;
        if (transform != null)
        {
            float distance = Vector3.Distance(transform.position, startPosition);
            if (distance > 0.5f)
            {
                Debug.LogError($"❌ Boss pozisyonu hala yanlış! Mesafe: {distance}. Zorla ayarlanıyor...");
                if (agent != null)
                {
                    agent.enabled = false;
                }
                transform.position = startPosition;
                transform.rotation = startRotation;
                if (agent != null)
                {
                    agent.enabled = true;
                    if (agent.isOnNavMesh)
                    {
                        agent.Warp(startPosition);
                    }
                }
            }
            else
            {
                Debug.Log($"✅ Boss pozisyonu doğru! Mesafe: {distance}");
            }
        }
        
        Debug.Log($"✅✅✅ Boss başarıyla resetlendi! Final pozisyon: {transform.position}, Hedef: {startPosition}");
    }

    void OnDestroy()
    {
        // Invoke ve Coroutine'leri iptal et
        CancelInvoke();
        StopAllCoroutines();
    }
    
    void OnDisable()
    {
        // Invoke ve Coroutine'leri iptal et
        CancelInvoke();
        StopAllCoroutines();
    }

    // Debug için Gizmos
    private void OnDrawGizmosSelected()
    {
        // Algılama alanı (Sarı)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Saldırı alanı (Kırmızı)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
