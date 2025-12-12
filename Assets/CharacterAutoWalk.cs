using UnityEngine;
using UnityEngine.AI;

public class CharacterAutoWalk : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Hareket hızı")]
    public float walkSpeed = 3f;
    
    [Tooltip("Otomatik yürüme aktif mi?")]
    public bool autoWalk = true;
    
    [Header("Path Settings")]
    [Tooltip("Waypoint'ler - koridor boyunca gidilecek noktalar (boşsa forward yönünde yürür)")]
    public Transform[] waypoints;
    
    [Tooltip("Döngüsel mi? (Son waypoint'ten sonra başa dönsün mü?)")]
    public bool loopPath = false;
    
    [Header("Forward Walk Settings")]
    [Tooltip("Waypoint yoksa hangi yöne yürüsün? (forward = karakterin önü)")]
    public Vector3 forwardDirection = Vector3.forward;
    
    [Tooltip("NavMesh Agent kullanılsın mı? (False = Transform ile hareket)")]
    public bool useNavMesh = false;
    
    private NavMeshAgent navAgent;
    private Animator animator;
    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private float startYPosition;
    
    void Start()
    {
        // Başlangıç Y pozisyonunu kaydet (uçmaması için)
        startYPosition = transform.position.y;
        
        // Animator kontrolü
        animator = GetComponent<Animator>();
        
        // NavMesh Agent kontrolü (sadece kullanılacaksa)
        if (useNavMesh)
        {
            navAgent = GetComponent<NavMeshAgent>();
            if (navAgent == null)
            {
                navAgent = gameObject.AddComponent<NavMeshAgent>();
            }
            
            if (navAgent != null)
            {
                navAgent.speed = walkSpeed;
                navAgent.stoppingDistance = 0.5f;
                navAgent.updatePosition = true;
                navAgent.updateRotation = true;
            }
        }
        else
        {
            // NavMesh Agent varsa devre dışı bırak
            navAgent = GetComponent<NavMeshAgent>();
            if (navAgent != null)
            {
                navAgent.enabled = false;
            }
        }
        
        // Yönü normalize et
        if (forwardDirection != Vector3.zero)
        {
            forwardDirection.y = 0;
            forwardDirection = forwardDirection.normalized;
        }
        
        // İlk waypoint'e git (varsa)
        if (autoWalk && waypoints != null && waypoints.Length > 0)
        {
            MoveToWaypoint(0);
        }
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
        
        // NavMesh kullanıyorsak
        if (useNavMesh && navAgent != null && navAgent.isOnNavMesh)
        {
            // Waypoint'e ulaşıldı mı kontrol et
            if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                // Bir sonraki waypoint'e git
                MoveToNextWaypoint();
            }
            
            // Animator kontrolü
            if (animator != null)
            {
                float speed = navAgent.velocity.magnitude;
                animator.SetFloat("Speed", speed);
            }
        }
        else if (waypoints != null && waypoints.Length > 0)
        {
            // Transform ile hareket
            Transform targetWaypoint = waypoints[currentWaypointIndex];
            if (targetWaypoint != null)
            {
                Vector3 direction = (targetWaypoint.position - transform.position);
                float distance = direction.magnitude;
                
                if (distance > 0.3f)
                {
                    // Y eksenini sıfırla ve normalize et
                    direction.y = 0;
                    direction = direction.normalized;
                    
                    // Hareket
                    Vector3 newPosition = transform.position + direction * walkSpeed * Time.deltaTime;
                    
                    // Y pozisyonunu koru (uçmaması için)
                    newPosition.y = startYPosition;
                    transform.position = newPosition;
                    
                    // Rotasyon - waypoint'e doğru bak
                    Vector3 lookDirection = direction;
                    if (lookDirection.magnitude > 0.1f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 8f * Time.deltaTime);
                    }
                    
                    // Animator kontrolü
                    if (animator != null)
                    {
                        animator.SetFloat("Speed", walkSpeed);
                    }
                    
                    isMoving = true;
                }
                else
                {
                    // Waypoint'e ulaşıldı, bir sonrakine geç
                    isMoving = false;
                    MoveToNextWaypoint();
                }
            }
        }
        else
        {
            // Waypoint yoksa forward yönünde yürü
            if (autoWalk)
            {
                // Karakterin forward yönünü kullan
                Vector3 moveDirection = transform.forward;
                if (forwardDirection != Vector3.zero)
                {
                    // Eğer forwardDirection ayarlanmışsa onu kullan
                    moveDirection = forwardDirection;
                }
                
                // Y eksenini sıfırla
                moveDirection.y = 0;
                moveDirection = moveDirection.normalized;
                
                // Hareket
                Vector3 newPosition = transform.position + moveDirection * walkSpeed * Time.deltaTime;
                
                // Y pozisyonunu koru (uçmaması için)
                newPosition.y = startYPosition;
                transform.position = newPosition;
                
                // Rotasyon - yürüdüğü yöne doğru bak (zaten forward'a bakıyorsa değişmez)
                if (moveDirection.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 8f * Time.deltaTime);
                }
                
                // Animator kontrolü
                if (animator != null)
                {
                    animator.SetFloat("Speed", walkSpeed);
                }
                
                isMoving = true;
            }
            else
            {
                isMoving = false;
                if (animator != null)
                {
                    animator.SetFloat("Speed", 0f);
                }
            }
        }
    }
    
    void MoveToWaypoint(int index)
    {
        if (waypoints == null || waypoints.Length == 0)
            return;
        
        if (index < 0 || index >= waypoints.Length)
            return;
        
        currentWaypointIndex = index;
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        
        if (targetWaypoint == null)
            return;
        
        if (useNavMesh && navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.SetDestination(targetWaypoint.position);
        }
    }
    
    void MoveToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;
        
        currentWaypointIndex++;
        
        if (currentWaypointIndex >= waypoints.Length)
        {
            if (loopPath)
            {
                currentWaypointIndex = 0;
            }
            else
            {
                // Döngü yoksa dur
                autoWalk = false;
                if (animator != null)
                {
                    animator.SetFloat("Speed", 0f);
                }
                return;
            }
        }
        
        MoveToWaypoint(currentWaypointIndex);
    }
    
    // Dışarıdan çağrılabilir - yürümeyi başlat/durdur
    public void SetAutoWalk(bool enable)
    {
        autoWalk = enable;
        if (enable && waypoints != null && waypoints.Length > 0)
        {
            MoveToWaypoint(currentWaypointIndex);
        }
    }
}


