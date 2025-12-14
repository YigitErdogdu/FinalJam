using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Mor robot için dost takipçi sistemi - Sadece oyuncuyu takip eder, saldırmaz
/// Oyuncu nereye giderse gitsin, mor robot onu takip eder
/// </summary>
public class EnemyFollower : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Takip edilecek hedef (otomatik bulunur)")]
    [SerializeField] private Transform target;
    
    [Tooltip("NavMeshAgent component (otomatik bulunur)")]
    [SerializeField] private NavMeshAgent agent;
    
    [Tooltip("Animator component (varsa)")]
    [SerializeField] private Animator animator;

    [Header("Follow Settings")]
    [Tooltip("Oyuncuya ne kadar yaklaşacak (metre)")]
    [SerializeField] private float followDistance = 3f;
    
    [Tooltip("Oyuncudan bu mesafeden uzaklaşırsa takip etmeye başlar")]
    [SerializeField] private float maxFollowDistance = 100f;

    [Header("Movement Settings")]
    [Tooltip("Takip hızı")]
    [SerializeField] private float moveSpeed = 3.5f;
    
    [Tooltip("Dönüş hızı")]
    [SerializeField] private float rotationSpeed = 5f;
    
    [Tooltip("Her zaman takip et (mesafe sınırı olmasın)")]
    [SerializeField] private bool alwaysFollow = true;

    [Header("Animation Settings (Opsiyonel)")]
    [Tooltip("Yürüme animasyon parametresi adı")]
    [SerializeField] private string walkAnimationParameter = "Walk";
    
    [Tooltip("Hız animasyon parametresi adı (float)")]
    [SerializeField] private string speedAnimationParameter = "Speed";

    [Header("Friendly Settings")]
    [Tooltip("Dost robot - hasar almaz ve saldırmaz")]
    [SerializeField] private bool isFriendly = true;

    // State
    private bool isFollowing = false;
    private Vector3 startPosition;
    private Quaternion startRotation;

    // Animation IDs
    private int animIDWalk;
    private int animIDSpeed;
    private bool hasWalkAnimation = false;
    private bool hasSpeedAnimation = false;

    void Awake()
    {
        // Başlangıç pozisyonunu kaydet
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
                Debug.Log($"✅ {gameObject.name}: Player bulundu! Artık onu takip edeceğim!");
            }
            else
            {
                Debug.LogWarning($"⚠️ {gameObject.name}: Player bulunamadı! 'Player' tag'ine sahip bir GameObject olmalı.");
            }
        }

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                Debug.LogError($"❌ {gameObject.name}: NavMeshAgent component bulunamadı! Lütfen ekleyin.");
            }
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // NavMeshAgent ayarları
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = followDistance;
            agent.autoBraking = true;
            agent.updateRotation = false; // Manuel rotasyon kontrolü
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
                
                if (param.name == speedAnimationParameter && param.type == AnimatorControllerParameterType.Float)
                {
                    hasSpeedAnimation = true;
                    animIDSpeed = Animator.StringToHash(speedAnimationParameter);
                }
            }

            if (hasWalkAnimation)
                Debug.Log($"✅ {gameObject.name}: '{walkAnimationParameter}' animasyon parametresi bulundu!");
            
            if (hasSpeedAnimation)
                Debug.Log($"✅ {gameObject.name}: '{speedAnimationParameter}' animasyon parametresi bulundu!");
        }

        // Eğer dost robotsa, layer'ı değiştir (opsiyonel)
        if (isFriendly)
        {
            // "Friendly" layer'ı varsa kullan, yoksa varsayılan
            int friendlyLayer = LayerMask.NameToLayer("Friendly");
            if (friendlyLayer != -1)
            {
                gameObject.layer = friendlyLayer;
            }
        }
    }

    void Update()
    {
        if (target == null || agent == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Her zaman takip et veya mesafe kontrolü yap
        if (alwaysFollow || distanceToTarget <= maxFollowDistance)
        {
            // Oyuncudan uzaksa takip et
            if (distanceToTarget > followDistance)
            {
                FollowTarget();
                isFollowing = true;
            }
            else
            {
                // Yeterince yakın, dur
                StopFollowing();
                isFollowing = false;
            }
        }
        else
        {
            // Çok uzaklaştı
            StopFollowing();
            isFollowing = false;
        }

        // Animasyonları güncelle
        UpdateAnimations();
    }

    private void FollowTarget()
    {
        if (agent == null || target == null) return;

        // Hedefe doğru git
        agent.isStopped = false;
        agent.SetDestination(target.position);

        // Hedefe doğru dön (smooth)
        RotateTowardsTarget();
    }

    private void StopFollowing()
    {
        if (agent == null) return;

        agent.isStopped = true;
        
        // Durduğunda da hedefe bak
        if (target != null)
        {
            RotateTowardsTarget();
        }
    }

    private void RotateTowardsTarget()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0; // Sadece yatay düzlemde dön

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Walk animasyonu (bool)
        if (hasWalkAnimation)
        {
            bool isWalking = isFollowing && agent != null && agent.velocity.magnitude > 0.1f;
            animator.SetBool(animIDWalk, isWalking);
        }

        // Speed animasyonu (float)
        if (hasSpeedAnimation)
        {
            float speed = agent != null ? agent.velocity.magnitude : 0f;
            animator.SetFloat(animIDSpeed, speed);
        }
    }

    // Başlangıç pozisyonuna dön
    public void ReturnToStart()
    {
        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(startPosition);
        }
    }

    // Hedefi değiştir
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        Debug.Log($"{gameObject.name}: Yeni hedef ayarlandı: {newTarget.name}");
    }

    // Takip mesafesini değiştir
    public void SetFollowDistance(float distance)
    {
        followDistance = distance;
        if (agent != null)
        {
            agent.stoppingDistance = followDistance;
        }
    }

    // Hızı değiştir
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
        if (agent != null)
        {
            agent.speed = moveSpeed;
        }
    }

    // Debug için Gizmos
    private void OnDrawGizmosSelected()
    {
        // Takip mesafesi (Yeşil - dost olduğu için)
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, followDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, followDistance);

        // Maksimum takip mesafesi (eğer alwaysFollow kapalıysa)
        if (!alwaysFollow)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
            Gizmos.DrawWireSphere(transform.position, maxFollowDistance);
        }

        // Hedefe çizgi çiz (eğer varsa)
        if (target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, target.position);
            
            // Hedef pozisyonunda küçük bir küre
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(target.position, 0.5f);
        }
    }
}
