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
    [Tooltip("Hareket hızı (mahkumun hızıyla eşleşmeli)")]
    public float moveSpeed = 3f;
    
    [Tooltip("Dönüş hızı")]
    public float rotationSpeed = 8f;
    
    [Tooltip("NavMesh Agent kullanılsın mı? (False = Transform ile hareket)")]
    public bool useNavMesh = false;
    
    private NavMeshAgent navAgent;
    private Animator animator;
    private CharacterAutoWalk targetAutoWalk;
    private Vector3 lastTargetPosition;
    private float targetSpeed = 0f;
    private float startYPosition;
    
    void Start()
    {
        // Başlangıç Y pozisyonunu kaydet (uçmaması için)
        startYPosition = transform.position.y;
        
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
        
        // Mahkumun CharacterAutoWalk script'ini bul (hızını takip etmek için)
        if (targetCharacter != null)
        {
            targetAutoWalk = targetCharacter.GetComponent<CharacterAutoWalk>();
            lastTargetPosition = targetCharacter.position;
        }
        
        // Guard tipine göre otomatik pozisyon ayarı
        string guardName = gameObject.name;
        if (guardName.Contains("Cyborg_Sentinel") || guardName.Contains("Guard1"))
        {
            // Guard1: Sol arka çapraz
            sideOffset = -1f;
        }
        else if (guardName.Contains("Purple_Armored") || guardName.Contains("Guard2"))
        {
            // Guard2: Sağ arka çapraz
            sideOffset = 1f;
        }
        
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
                navAgent.speed = moveSpeed;
                navAgent.angularSpeed = rotationSpeed * 50f;
                navAgent.stoppingDistance = 0.1f;
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
    }
    
    void Update()
    {
        if (targetCharacter == null)
        {
            // Animator kontrolü - durduğunda
            if (animator != null)
            {
                animator.SetFloat("Speed", 0f);
            }
            return;
        }
        
        // Mahkumun hızını hesapla
        Vector3 currentTargetPosition = targetCharacter.position;
        float frameDistance = Vector3.Distance(lastTargetPosition, currentTargetPosition);
        targetSpeed = frameDistance / Time.deltaTime;
        lastTargetPosition = currentTargetPosition;
        
        // Mahkumun CharacterAutoWalk script'inden hızı al
        if (targetAutoWalk != null)
        {
            moveSpeed = targetAutoWalk.walkSpeed;
        }
        
        // Ana karakterin pozisyonunu ve rotasyonunu al
        Vector3 targetForward = targetCharacter.forward;
        Vector3 targetRight = targetCharacter.right;
        Vector3 targetPos = targetCharacter.position;
        
        // Hedef pozisyonu hesapla: Ana karakterin arka çaprazında
        Vector3 behindPosition = targetPos - targetForward * followDistance;
        Vector3 sideOffsetVector = targetRight * (sideOffset * sideDistance);
        Vector3 desiredPosition = behindPosition + sideOffsetVector;
        
        // Y eksenini koru (uçmaması için)
        desiredPosition.y = startYPosition;
        
        // NavMesh kullanıyorsak
        if (useNavMesh && navAgent != null && navAgent.isOnNavMesh && navAgent.enabled)
        {
            navAgent.speed = moveSpeed;
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
            // Transform ile hareket - mahkumla eş zamanlı
            Vector3 direction = (desiredPosition - transform.position);
            float distance = direction.magnitude;
            
            // Mahkum yürüyorsa guard da yürüsün
            bool isTargetMoving = targetSpeed > 0.1f;
            
            if (isTargetMoving || distance > 0.2f)
            {
                // Hareket - mahkumun hızıyla eş zamanlı
                direction.y = 0; // Y eksenini sıfırla
                direction = direction.normalized;
                
                // Mesafe çok fazlaysa hızlı yaklaş, yakınsa mahkumun hızıyla yürü
                float currentSpeed = distance > 1f ? moveSpeed * 1.2f : moveSpeed;
                transform.position += direction * currentSpeed * Time.deltaTime;
                
                // Y pozisyonunu koru (uçmaması için)
                Vector3 pos = transform.position;
                pos.y = startYPosition; // Başlangıç Y pozisyonunu kullan
                transform.position = pos;
                
                // Rotasyon - mahkumun yönüne doğru bak (yürüdüğü yöne)
                Vector3 lookDirection = targetForward;
                lookDirection.y = 0;
                if (lookDirection.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
                
                // Animator kontrolü - mahkum yürüyorsa guard da yürüsün
                if (animator != null)
                {
                    float animSpeed = isTargetMoving ? moveSpeed : Mathf.Min(moveSpeed, distance * 2f);
                    animator.SetFloat("Speed", animSpeed);
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
