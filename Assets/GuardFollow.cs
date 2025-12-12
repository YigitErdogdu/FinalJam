using UnityEngine;
using UnityEngine.AI;

public class GuardFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Ana karakter (mahkum) - Inspector'dan atanacak")]
    public Transform targetCharacter;
    
    [Header("Position Settings")]
    [Tooltip("Ana karakterin arkasında ne kadar mesafe olacak")]
    public float followDistance = 2f;
    
    [Tooltip("Sağ tarafta mı sol tarafta mı? (1 = sağ, -1 = sol)")]
    public float sideOffset = 1f; // 1 = sağ, -1 = sol
    
    [Tooltip("Yan tarafta ne kadar mesafe olacak")]
    public float sideDistance = 1.5f;
    
    [Header("Movement Settings")]
    [Tooltip("Hareket hızı")]
    public float moveSpeed = 3f;
    
    [Tooltip("Dönüş hızı")]
    public float rotationSpeed = 5f;
    
    [Tooltip("NavMesh Agent kullanılsın mı?")]
    public bool useNavMesh = true;
    
    private NavMeshAgent navAgent;
    private Animator animator;
    private Vector3 targetPosition;
    
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
            navAgent.speed = moveSpeed;
            navAgent.angularSpeed = rotationSpeed * 50f;
            navAgent.stoppingDistance = 0.5f;
        }
        
        // Animator kontrolü
        animator = GetComponent<Animator>();
        
        // Eğer target atanmamışsa, sahne içinde Tattooed_Character'ı bul
        if (targetCharacter == null)
        {
            GameObject targetObj = GameObject.Find("Tattooed_Character_St_1212141706_texture");
            if (targetObj != null)
            {
                targetCharacter = targetObj.transform;
            }
        }
    }
    
    void Update()
    {
        if (targetCharacter == null)
        {
            return;
        }
        
        // Ana karakterin pozisyonunu ve rotasyonunu al
        Vector3 targetForward = targetCharacter.forward;
        Vector3 targetRight = targetCharacter.right;
        Vector3 targetPosition = targetCharacter.position;
        
        // Hedef pozisyonu hesapla: Ana karakterin arkasında, sağ/sol tarafta
        Vector3 behindPosition = targetPosition - targetForward * followDistance;
        Vector3 sideOffsetVector = targetRight * (sideOffset * sideDistance);
        Vector3 desiredPosition = behindPosition + sideOffsetVector;
        
        // NavMesh kullanıyorsak
        if (useNavMesh && navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.SetDestination(desiredPosition);
            
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
            Vector3 direction = (desiredPosition - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, desiredPosition);
            
            if (distance > 0.1f)
            {
                // Hareket
                transform.position += direction * moveSpeed * Time.deltaTime;
                
                // Rotasyon - ana karaktere doğru bak
                Vector3 lookDirection = targetPosition - transform.position;
                lookDirection.y = 0; // Y eksenini sıfırla
                if (lookDirection.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
                
                // Animator kontrolü
                if (animator != null)
                {
                    animator.SetFloat("Speed", moveSpeed);
                }
            }
            else
            {
                // Animator kontrolü - durduğunda
                if (animator != null)
                {
                    animator.SetFloat("Speed", 0f);
                }
            }
        }
    }
    
    // Inspector'dan pozisyon ayarlamak için helper method
    public void SetSidePosition(bool isRightSide)
    {
        sideOffset = isRightSide ? 1f : -1f;
    }
}
