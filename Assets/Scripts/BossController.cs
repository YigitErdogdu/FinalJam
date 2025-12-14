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

    void LateUpdate()
    {
        // Boss'u ZORLA zeminde tut - HER FRAME
        if (!isDead && transform != null)
        {
            Vector3 currentPos = transform.position;
            
            // Raycast ile zemini bul
            RaycastHit hit;
            if (Physics.Raycast(currentPos + Vector3.up * 3f, Vector3.down, out hit, 10f, ~0, QueryTriggerInteraction.Ignore))
            {
                float groundY = hit.point.y;
                
                // HER ZAMAN zemin seviyesinde tut (küçük tolerans: 0.05)
                if (Mathf.Abs(currentPos.y - groundY) > 0.05f)
                {
                    Vector3 correctedPos = new Vector3(currentPos.x, groundY, currentPos.z);
                    transform.position = correctedPos;
                    
                    // NavMeshAgent'ı da senkronize et
                    if (agent != null && agent.enabled && agent.isOnNavMesh)
                    {
                        agent.nextPosition = correctedPos;
                    }
                }
            }
            
            // NavMeshAgent ile transform arasındaki Y farkını kontrol et
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                Vector3 agentPos = agent.nextPosition;
                
                // Y pozisyonları farklıysa düzelt
                if (Mathf.Abs(transform.position.y - agentPos.y) > 0.05f)
                {
                    // Transform'un Y'sini kullan
                    agent.nextPosition = new Vector3(agentPos.x, transform.position.y, agentPos.z);
                }
            }
        }
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
        //DealDamageToPlayer();

        // Saldırı animasyonunun geri kalanını bekle
        yield return new WaitForSeconds(attackDuration * 0.5f);

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
            Destroy(gameObject);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weapon") && PlayerCombat.instance.isAttacking)
        {
            TakeDamage(10);
        }
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
