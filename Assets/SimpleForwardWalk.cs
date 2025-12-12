using UnityEngine;
using UnityEngine.AI;

public class SimpleForwardWalk : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Hareket hızı")]
    public float walkSpeed = 3f;
    
    [Tooltip("Otomatik yürüme aktif mi?")]
    public bool autoWalk = true;
    
    [Header("Direction Settings")]
    [Tooltip("Hangi yöne yürüyecek? (1 = ileri, -1 = geri)")]
    public Vector3 walkDirection = Vector3.forward; // İleri doğru
    
    [Tooltip("Y eksenini sıfırla (sadece X ve Z ekseninde hareket)")]
    public bool ignoreYAxis = true;
    
    [Tooltip("NavMesh Agent kullanılsın mı?")]
    public bool useNavMesh = true;
    
    [Header("Stop Settings")]
    [Tooltip("Belirli bir mesafeden sonra dursun mu?")]
    public bool stopAfterDistance = false;
    
    [Tooltip("Ne kadar mesafe yürüsün? (0 = sınırsız)")]
    public float maxDistance = 0f;
    
    private NavMeshAgent navAgent;
    private Animator animator;
    private Vector3 startPosition;
    private float distanceTraveled = 0f;
    
    void Start()
    {
        // Başlangıç pozisyonunu kaydet
        startPosition = transform.position;
        
        // NavMesh Agent kontrolü
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null && useNavMesh)
        {
            navAgent = gameObject.AddComponent<NavMeshAgent>();
        }
        
        if (navAgent != null)
        {
            navAgent.speed = walkSpeed;
            navAgent.stoppingDistance = 0.1f;
        }
        
        // Animator kontrolü
        animator = GetComponent<Animator>();
        
        // Yönü normalize et
        if (ignoreYAxis)
        {
            walkDirection.y = 0;
        }
        walkDirection = walkDirection.normalized;
    }
    
    void Update()
    {
        if (!autoWalk)
        {
            // Animator kontrolü - durduğunda
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }
            return;
        }
        
        // Mesafe kontrolü
        if (stopAfterDistance && maxDistance > 0)
        {
            distanceTraveled = Vector3.Distance(startPosition, transform.position);
            if (distanceTraveled >= maxDistance)
            {
                autoWalk = false;
                if (animator != null)
                {
                    animator.SetFloat("Speed", 0f);
                }
                return;
            }
        }
        
        // NavMesh kullanıyorsak
        if (useNavMesh && navAgent != null && navAgent.isOnNavMesh)
        {
            // İleri doğru hareket et
            Vector3 targetPosition = transform.position + walkDirection * walkSpeed * Time.deltaTime;
            navAgent.SetDestination(targetPosition);
            
            // Animator kontrolü
            if (animator != null)
            {
                float speed = navAgent.velocity.magnitude;
                animator.SetFloat("Speed", speed);
            }
        }
        else
        {
            // Transform ile hareket
            Vector3 movement = walkDirection * walkSpeed * Time.deltaTime;
            transform.position += movement;
            
            // Rotasyon - yürüdüğü yöne doğru bak
            if (walkDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(walkDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
            }
            
            // Animator kontrolü
            if (animator != null)
            {
                animator.SetFloat("Speed", walkSpeed);
            }
        }
    }
    
    // Dışarıdan çağrılabilir - yürümeyi başlat/durdur
    public void SetAutoWalk(bool enable)
    {
        autoWalk = enable;
    }
    
    // Yönü değiştir
    public void SetDirection(Vector3 direction)
    {
        if (ignoreYAxis)
        {
            direction.y = 0;
        }
        walkDirection = direction.normalized;
    }
}

