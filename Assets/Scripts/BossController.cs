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
    [SerializeField] private float attackCooldown = 2f;
    
    [Tooltip("Saldırı animasyon süresi")]
    [SerializeField] private float attackDuration = 1.5f;

    [Header("Movement Settings")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Health Settings")]
    [Tooltip("Boss'un maksimum canı")]
    [SerializeField] private float maxHealth = 100f;
    
    private float currentHealth;

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

        // Saldırı animasyonu süresince bekle
        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
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

        Debug.Log("Boss öldü!");

        // NavMeshAgent'ı durdur
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Ölüm animasyonu (eğer varsa)
        if (animator != null)
        {
            animator.SetTrigger("Death"); // Animator'da "Death" trigger'ı olmalı
        }

        // Collider'ı kapat (isteğe bağlı)
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // 3 saniye sonra objeyi yok et (isteğe bağlı)
        Destroy(gameObject, 3f);
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
