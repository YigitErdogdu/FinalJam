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
    [Tooltip("Waypoint'ler - koridor boyunca gidilecek noktalar")]
    public Transform[] waypoints;
    
    [Tooltip("Döngüsel mi? (Son waypoint'ten sonra başa dönsün mü?)")]
    public bool loopPath = false;
    
    [Tooltip("NavMesh Agent kullanılsın mı?")]
    public bool useNavMesh = true;
    
    private NavMeshAgent navAgent;
    private Animator animator;
    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    
    void Start()
    {
        // NavMesh Agent kontrolü
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null && useNavMesh)
        {
            navAgent = gameObject.AddComponent<NavMeshAgent>();
        }
        
        if (navAgent != null)
        {
            navAgent.speed = walkSpeed;
            navAgent.stoppingDistance = 0.5f;
        }
        
        // Animator kontrolü
        animator = GetComponent<Animator>();
        
        // İlk waypoint'e git
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
                Vector3 direction = (targetWaypoint.position - transform.position).normalized;
                float distance = Vector3.Distance(transform.position, targetWaypoint.position);
                
                if (distance > 0.5f)
                {
                    // Hareket
                    transform.position += direction * walkSpeed * Time.deltaTime;
                    
                    // Rotasyon - waypoint'e doğru bak
                    Vector3 lookDirection = targetWaypoint.position - transform.position;
                    lookDirection.y = 0; // Y eksenini sıfırla
                    if (lookDirection.magnitude > 0.1f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
                    }
                    
                    // Animator kontrolü
                    if (animator != null)
                    {
                        animator.SetFloat("Speed", walkSpeed);
                    }
                }
                else
                {
                    // Waypoint'e ulaşıldı, bir sonrakine geç
                    MoveToNextWaypoint();
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

