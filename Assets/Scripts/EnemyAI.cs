using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Mor robot için gelişmiş AI controller - Takip ve saldırı özellikleri
/// </summary>
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Takip edilecek hedef (otomatik bulunur)")]
    [SerializeField] private Transform target;
    
    [Tooltip("NavMeshAgent component (otomatik bulunur)")]
    [SerializeField] private NavMeshAgent agent;
    
    [Tooltip("Animator component (varsa)")]
    [SerializeField] private Animator animator;

    [Header("Detection Settings")]
    [Tooltip("Oyuncuyu algılama mesafesi")]
    [SerializeField] private float detectionRange = 20f;
    
    [Tooltip("Saldırı mesafesi")]
    [SerializeField] private float attackRange = 2.5f;

    [Header("Movement Settings")]
    [Tooltip("Takip hızı")]
    [SerializeField] private float moveSpeed = 3.5f;
    
    [Tooltip("Dönüş hızı")]
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Attack Settings")]
    [Tooltip("Saldırı hasarı")]
    [SerializeField] private float attackDamage = 10f;
    
    [Tooltip("Saldırılar arası bekleme süresi")]
    [SerializeField] private float attackCooldown = 2f;
    
    [Tooltip("Saldırı animasyon süresi")]
    [SerializeField] private float attackDuration = 1f;

    [Header("Health Settings")]
    [Tooltip("Maksimum can")]
    [SerializeField] private float maxHealth = 50f;

    [Header("Animation Settings (Opsiyonel)")]
    [Tooltip("Yürüme animasyon parametresi")]
    [SerializeField] private string walkAnimationParameter = "Walk";
    
    [Tooltip("Saldırı animasyon trigger'ı")]
    [SerializeField] private string attackAnimationTrigger = "Attack";

    // State
    private enum EnemyState { Idle, Chase, Attack }
    private EnemyState currentState = EnemyState.Idle;
    
    private float currentHealth;
    private bool isDead = false;
    private bool isAttacking = false;
    private float lastAttackTime = -999f;

    // Animation IDs
    private int animIDWalk;
    private int animIDAttack;
    private bool hasWalkAnimation = false;
    private bool hasAttackAnimation = false;

    // Start position
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void Start()
    {
        // Otomatik referans bulma
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
                Debug.Log($"✅ {gameObject.name}: Player bulundu!");
            }
            else
            {
                Debug.LogWarning($"⚠️ {gameObject.name}: Player bulunamadı!");
            }
        }

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // NavMeshAgent ayarları
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange - 0.5f;
            agent.autoBraking = true;
        }

        // Animation parametrelerini kontrol et
        if (animator != null)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == walkAnimationParameter && param.type == AnimatorControllerParameterType.Bool)
                {
                    hasWalkAnimation = true;
                    animIDWalk = Animator.StringToHash(walkAnimationParameter);
                }
                
                if (param.name == attackAnimationTrigger && param.type == AnimatorControllerParameterType.Trigger)
                {
                    hasAttackAnimation = true;
                    animIDAttack = Animator.StringToHash(attackAnimationTrigger);
                }
            }
        }

        // Can sistemini başlat
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (target == null || agent == null || isDead) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // State Machine
        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState(distanceToTarget);
                break;

            case EnemyState.Chase:
                HandleChaseState(distanceToTarget);
                break;

            case EnemyState.Attack:
                HandleAttackState(distanceToTarget);
                break;
        }

        // Animasyonları güncelle
        UpdateAnimations();
    }

    private void HandleIdleState(float distanceToTarget)
    {
        // Hedef algılama alanına girdi mi?
        if (distanceToTarget <= detectionRange)
        {
            currentState = EnemyState.Chase;
        }

        if (agent != null)
            agent.isStopped = true;
    }

    private void HandleChaseState(float distanceToTarget)
    {
        // Hedef çok uzaklaştı mı?
        if (distanceToTarget > detectionRange + 5f)
        {
            currentState = EnemyState.Idle;
            if (agent != null)
                agent.isStopped = true;
            return;
        }

        // Saldırı mesafesine girdi mi?
        if (distanceToTarget <= attackRange)
        {
            currentState = EnemyState.Attack;
            if (agent != null)
                agent.isStopped = true;
            return;
        }

        // Hedefi takip et
        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(target.position);
        }

        // Hedefe doğru dön
        RotateTowardsTarget();
    }

    private void HandleAttackState(float distanceToTarget)
    {
        // Hedef saldırı alanından çıktı mı?
        if (distanceToTarget > attackRange + 1f)
        {
            currentState = EnemyState.Chase;
            return;
        }

        // Hedefe doğru dön
        RotateTowardsTarget();

        // Saldırı cooldown'ı bitti mi?
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
        if (animator != null && hasAttackAnimation)
        {
            animator.SetTrigger(animIDAttack);
        }

        // Saldırı animasyonunun yarısında hasar ver
        yield return new WaitForSeconds(attackDuration * 0.5f);
        
        DealDamageToTarget();

        // Saldırı animasyonunun geri kalanını bekle
        yield return new WaitForSeconds(attackDuration * 0.5f);

        isAttacking = false;
    }

    private void DealDamageToTarget()
    {
        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        if (distanceToTarget <= attackRange + 1f)
        {
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null && !playerHealth.IsDead())
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"✅ {gameObject.name} oyuncuya {attackDamage} hasar verdi!");
            }
        }
    }

    private void RotateTowardsTarget()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Walk animasyonu
        if (hasWalkAnimation)
        {
            bool isWalking = (currentState == EnemyState.Chase);
            animator.SetBool(animIDWalk, isWalking);
        }
    }

    // Hasar alma
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} hasar aldı! Kalan can: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        currentHealth = 0;

        Debug.Log($"{gameObject.name} öldü!");

        // NavMeshAgent'ı durdur
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Collider'ı kapat
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Ölüm efekti
        DeathEffect deathEffect = GetComponent<DeathEffect>();
        if (deathEffect != null)
        {
            deathEffect.PlayDeathEffect(transform.position);
        }

        // Objeyi yok et veya deaktif et
        Destroy(gameObject, 2f);
    }

    // Debug için Gizmos
    private void OnDrawGizmosSelected()
    {
        // Algılama alanı (Mor)
        Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.3f);
        Gizmos.DrawSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Saldırı alanı (Kırmızı)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
