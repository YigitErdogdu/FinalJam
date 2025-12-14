using UnityEngine;
using UnityEngine.AI;

public class SimpleForwardWalk : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Takip edilecek hedef (Player - otomatik bulunur)")]
    public Transform target;
    
    [Header("Movement Settings")]
    [Tooltip("Hareket hızı")]
    public float walkSpeed = 3f;
    
    [Tooltip("Otomatik takip aktif mi?")]
    public bool autoWalk = true;
    
    [Header("Follow Settings")]
    [Tooltip("Oyuncuya ne kadar yaklaşacak (metre) - Bu mesafeye gelince durur")]
    public float followDistance = 3f;
    
    [Tooltip("Oyuncudan bu mesafeden uzaklaşırsa takip etmeye başlar")]
    public float maxFollowDistance = 100f;
    
    [Tooltip("Her zaman takip et (mesafe sınırı olmasın)")]
    public bool alwaysFollow = true;
    
    [Tooltip("Durma mesafesi (followDistance'dan biraz daha büyük - daha erken durur)")]
    public float stopDistance = 3.5f;
    
    [Tooltip("NavMesh Agent kullanılsın mı?")]
    public bool useNavMesh = true;
    
    [Header("Animation Settings")]
    [Tooltip("Yürüme animasyon parametresi adı (bool)")]
    public string walkAnimationParameter = "Walk";
    
    [Tooltip("Hız animasyon parametresi adı (float)")]
    public string speedAnimationParameter = "Speed";
    
    private NavMeshAgent navAgent;
    private Animator animator;
    private Vector3 startPosition;
    private bool isFollowing = false;
    
    void Start()
    {
        // Başlangıç pozisyonunu kaydet
        startPosition = transform.position;
        
        // Player'ı otomatik bul
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
        
        // NavMesh Agent kontrolü
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null && useNavMesh)
        {
            navAgent = gameObject.AddComponent<NavMeshAgent>();
        }
        
        if (navAgent != null)
        {
            navAgent.speed = walkSpeed;
            navAgent.stoppingDistance = followDistance; // NavMesh'in durma mesafesi
            navAgent.autoBraking = true; // Otomatik yavaşlama
            navAgent.updateRotation = true; // NavMesh rotasyonu kontrol etsin
            
            // stopDistance'ı followDistance'dan biraz büyük yap (eğer ayarlanmamışsa)
            if (stopDistance <= followDistance)
            {
                stopDistance = followDistance + 0.5f;
            }
        }
        
        // Animator kontrolü
        animator = GetComponent<Animator>();
    }
    
    void Update()
    {
        if (!autoWalk || target == null)
        {
            // Animator kontrolü - durduğunda
            StopFollowing();
            return;
        }
        
        // Player'a olan mesafe
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        // Her zaman takip et veya mesafe kontrolü yap
        if (alwaysFollow || distanceToTarget <= maxFollowDistance)
        {
            // Oyuncudan uzaksa takip et (stopDistance'dan uzaksa)
            if (distanceToTarget > stopDistance)
            {
                FollowTarget();
                isFollowing = true;
            }
            else
            {
                // Yeterince yakın, dur (followDistance'a ulaştı veya geçti)
                StopFollowing();
                isFollowing = false;
                
                // Debug: Her 60 frame'de bir mesafe bilgisi
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"🛑 {gameObject.name}: Player'a yeterince yakın! Mesafe: {distanceToTarget:F2}m (Durma: {stopDistance}m, Takip: {followDistance}m)");
                }
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
        if (target == null) return;
        
        // NavMesh kullanıyorsak
        if (useNavMesh && navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = false;
            navAgent.speed = walkSpeed;
            navAgent.SetDestination(target.position);
        }
        else
        {
            // Transform ile hareket (NavMesh yoksa)
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0; // Sadece yatay düzlemde hareket
            
            if (direction.magnitude > 0.1f)
            {
                // Hareket et
                Vector3 movement = direction * walkSpeed * Time.deltaTime;
                transform.position += movement;
                
                // Hedefe doğru dön
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
            }
        }
    }
    
    private void StopFollowing()
    {
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
        }
    }
    
    private void UpdateAnimations()
    {
        if (animator == null) return;
        
        // NavMesh kullanıyorsak hızı NavMesh'ten al
        float currentSpeed = 0f;
        if (useNavMesh && navAgent != null && navAgent.isOnNavMesh)
        {
            currentSpeed = navAgent.velocity.magnitude;
        }
        else if (isFollowing)
        {
            currentSpeed = walkSpeed;
        }
        
        // Speed parametresi (float)
        if (!string.IsNullOrEmpty(speedAnimationParameter))
        {
            animator.SetFloat(speedAnimationParameter, currentSpeed);
        }
        
        // Walk parametresi (bool)
        if (!string.IsNullOrEmpty(walkAnimationParameter))
        {
            animator.SetBool(walkAnimationParameter, isFollowing && currentSpeed > 0.1f);
        }
    }
    
    // Dışarıdan çağrılabilir - takibi başlat/durdur
    public void SetAutoWalk(bool enable)
    {
        autoWalk = enable;
    }
    
    // Hedefi değiştir
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    // Takip mesafesini ayarla
    public void SetFollowDistance(float distance)
    {
        followDistance = distance;
        if (navAgent != null)
        {
            navAgent.stoppingDistance = followDistance;
        }
    }
}


