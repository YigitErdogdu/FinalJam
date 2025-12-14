using UnityEngine;

/// <summary>
/// Mor robot için SÜPER BASİT takip sistemi
/// NavMesh gerektirmez! Direkt Transform ile çalışır
/// </summary>
public class SimpleFollower : MonoBehaviour
{
    [Header("Takip Ayarları")]
    [Tooltip("Oyuncuya ne kadar yaklaşacak (metre)")]
    [SerializeField] private float followDistance = 5f; // Daha uzakta dursun
    
    [Tooltip("Hareket hızı")]
    [SerializeField] private float moveSpeed = 2.5f; // Daha yavaş
    
    [Tooltip("Dönüş hızı")]
    [SerializeField] private float rotationSpeed = 5f;
    
    [Tooltip("Her zaman takip et (harita ne kadar büyük olursa olsun)")]
    [SerializeField] private bool alwaysFollow = true;

    [Header("Otomatik Bulunur")]
    [SerializeField] private Transform target; // Player otomatik bulunur
    
    [Header("Animasyon (Opsiyonel)")]
    [SerializeField] private Animator animator;
    [SerializeField] private string walkAnimationParameter = "Walk";

    private int animIDWalk;
    private bool hasWalkAnimation = false;
    private CharacterController characterController;
    
    // Oyuncu hareket takibi
    private Vector3 lastPlayerPosition;
    private float playerStationaryTime = 0f;
    private float playerStationaryThreshold = 0.5f; // 0.5 saniye hareketsiz kalırsa dur

    void Start()
    {
        // Player'ı otomatik bul - KENDİSİNİ BULMASIN!
        if (target == null)
        {
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
            
            // Kendisini hariç tut
            foreach (GameObject playerObj in allPlayers)
            {
                // Eğer bu obje kendisi değilse ve PlayerArmature ise
                if (playerObj != this.gameObject && 
                    (playerObj.name.Contains("PlayerArmature") || playerObj.name.Contains("Player")))
                {
                    target = playerObj.transform;
                    Debug.Log($"✅ {gameObject.name}: Player bulundu! ({playerObj.name}) Takip başlıyor!");
                    break;
                }
            }
            
            // Eğer hala bulunamadıysa, ilk Player tag'li objeyi al (kendisi hariç)
            if (target == null)
            {
                foreach (GameObject playerObj in allPlayers)
                {
                    if (playerObj != this.gameObject)
                    {
                        target = playerObj.transform;
                        Debug.Log($"✅ {gameObject.name}: Player bulundu! ({playerObj.name}) Takip başlıyor!");
                        break;
                    }
                }
            }
            
            if (target == null)
            {
                Debug.LogError($"❌ {gameObject.name}: Player bulunamadı! Player objesine 'Player' tag'i ekleyin. " +
                              $"NOT: Robot'un tag'i 'Player' OLMAMALI! Tag'i 'Untagged' veya başka bir şey yapın!");
            }
        }

        // Animator varsa kontrol et ve otomatik bul
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator != null)
            {
                Debug.Log($"✅ {gameObject.name}: Animator otomatik bulundu!");
            }
        }

        if (animator != null)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == walkAnimationParameter && param.type == AnimatorControllerParameterType.Bool)
                {
                    hasWalkAnimation = true;
                    animIDWalk = Animator.StringToHash(walkAnimationParameter);
                    Debug.Log($"✅ {gameObject.name}: Walk animasyonu bulundu!");
                    break;
                }
            }
            
            // Eğer Walk animasyonu yoksa, Speed parametresini dene
            if (!hasWalkAnimation)
            {
                foreach (AnimatorControllerParameter param in animator.parameters)
                {
                    if (param.name == "Speed" && param.type == AnimatorControllerParameterType.Float)
                    {
                        Debug.Log($"ℹ️ {gameObject.name}: Walk animasyonu yok ama Speed parametresi var. Animasyon olmadan çalışacak.");
                        break;
                    }
                }
            }
            
            // Attack parametresini devre dışı bırak (saldırı yapmasın)
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == "Attack" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    // Attack trigger'ını asla tetikleme
                    Debug.Log($"✅ {gameObject.name}: Attack parametresi bulundu ama devre dışı bırakıldı (saldırı yapmayacak).");
                    break;
                }
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ {gameObject.name}: Animator bulunamadı! Animasyon olmadan çalışacak.");
        }

        // CharacterController varsa kullan (daha iyi fizik)
        characterController = GetComponent<CharacterController>();
        if (characterController != null)
        {
            Debug.Log($"✅ {gameObject.name}: CharacterController bulundu!");
        }
        
        // Rigidbody kontrolü
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log($"✅ {gameObject.name}: Rigidbody bulundu! IsKinematic: {rb.isKinematic}");
            // Eğer kinematic değilse, freeze rotation yap (sadece Y ekseninde dön)
            if (!rb.isKinematic)
            {
                rb.freezeRotation = true;
            }
        }
        
        // Oyuncu pozisyonunu kaydet
        if (target != null)
        {
            lastPlayerPosition = target.position;
        }
        
        Debug.Log($"✅ {gameObject.name}: SimpleFollower başlatıldı! Follow Distance: {followDistance}, Move Speed: {moveSpeed}");
    }

    void Update()
    {
        if (target == null)
        {
            // Her frame uyarı verme, sadece her 2 saniyede bir
            if (Time.frameCount % 120 == 0)
            {
                Debug.LogWarning($"❌ {gameObject.name}: Target (Player) bulunamadı! Player tag'li obje var mı kontrol edin.");
            }
            return;
        }

        // Oyuncu hareket ediyor mu kontrol et (hem pozisyon hem de input)
        float playerMovement = Vector3.Distance(target.position, lastPlayerPosition);
        bool isPlayerMovingByPosition = playerMovement > 0.1f; // 0.1 metreden fazla hareket
        
        // Oyuncunun input'unu kontrol et (ThirdPersonController varsa)
        bool isPlayerMovingByInput = false;
        StarterAssets.ThirdPersonController thirdPersonController = target.GetComponent<StarterAssets.ThirdPersonController>();
        if (thirdPersonController != null)
        {
            // StarterAssetsInputs'u kontrol et
            StarterAssets.StarterAssetsInputs inputs = target.GetComponent<StarterAssets.StarterAssetsInputs>();
            if (inputs != null && inputs.move != Vector2.zero)
            {
                isPlayerMovingByInput = true;
            }
        }
        
        // Her iki yöntemden biri hareket gösteriyorsa hareket ediyor
        bool isPlayerMoving = isPlayerMovingByPosition || isPlayerMovingByInput;
        
        if (isPlayerMoving)
        {
            playerStationaryTime = 0f; // Hareket ediyor, resetle
        }
        else
        {
            playerStationaryTime += Time.deltaTime; // Duruyor, süreyi artır
        }
        lastPlayerPosition = target.position;

        // Oyuncuya olan mesafe
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);
        
        // Her 2 saniyede bir mesafe bilgisi yazdır
        if (Time.frameCount % 120 == 0)
        {
            Debug.Log($"🤖 {gameObject.name}: Player'a mesafe = {distanceToPlayer:F2} metre | " +
                     $"Follow Distance: {followDistance} | " +
                     $"Always Follow: {alwaysFollow} | " +
                     $"Target: {target.name} | " +
                     $"Pozisyon: {transform.position}");
        }

        // Oyuncu hareket etmiyorsa robot da durmalı
        if (!isPlayerMoving)
        {
            // Oyuncu duruyor, robot da dursun
            if (hasWalkAnimation && animator != null)
            {
                animator.SetBool(animIDWalk, false);
            }
            // Animator varsa ama Walk animasyonu yoksa, Speed parametresini 0 yap
            else if (animator != null)
            {
                // Speed parametresi varsa 0 yap (Idle animasyonuna geçsin)
                AnimatorControllerParameter speedParam = null;
                foreach (AnimatorControllerParameter param in animator.parameters)
                {
                    if (param.name == "Speed" && param.type == AnimatorControllerParameterType.Float)
                    {
                        animator.SetFloat("Speed", 0f);
                        break;
                    }
                }
            }
            
            // if (Time.frameCount % 120 == 0)
            // {
            //     Debug.Log($"⏸️ {gameObject.name}: Oyuncu duruyor, robot da duruyor. (Hareketsiz süre: {playerStationaryTime:F2}s)");
            // }
            
            // Sadece oyuncuya bak, hareket etme
            RotateTowardsPlayer();
            return;
        }
        
        // Her zaman takip et VEYA followDistance'dan uzaksa takip et
        bool shouldFollow = alwaysFollow || distanceToPlayer > followDistance;
        bool isFarEnough = distanceToPlayer > followDistance;
        
        // Durma mesafesi (biraz daha erken dur, çarpışmayı önle)
        float stopDistance = followDistance + 1f; // 1 metre ekstra güvenlik
        
        if (shouldFollow && isFarEnough && distanceToPlayer > stopDistance)
        {
            // Hareket et - ama yaklaştıkça yavaşla
            float speedMultiplier = 1f;
            if (distanceToPlayer < followDistance * 2f)
            {
                // Yaklaştıkça yavaşla (smooth stop)
                speedMultiplier = Mathf.Clamp01((distanceToPlayer - stopDistance) / (followDistance * 2f - stopDistance));
                speedMultiplier = Mathf.Max(0.3f, speedMultiplier); // Minimum %30 hız
            }
            
            Vector3 oldPosition = transform.position;
            MoveTowardsPlayer(distanceToPlayer, speedMultiplier);
            Vector3 newPosition = transform.position;
            float actualMovement = Vector3.Distance(oldPosition, newPosition);
            
            // Walk animasyonu (varsa)
            if (hasWalkAnimation && animator != null)
            {
                animator.SetBool(animIDWalk, true);
            }
            
            // Attack state'ine geçişi engelle (saldırı yapmasın)
            if (animator != null)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                // Eğer Attack state'indeyse hemen Idle'a döndür
                if (stateInfo.IsName("T-Pose@Dual Weapon Combo") || stateInfo.IsName("Attack"))
                {
                    animator.Play("Idle", 0, 0f);
                }
            }
            
            // Debug: Her 2 saniyede bir hareket bilgisi
            if (Time.frameCount % 120 == 0)
            {
                Debug.Log($"🏃 {gameObject.name}: HAREKET EDİYOR! " +
                         $"Mesafe: {distanceToPlayer:F2}m | " +
                         $"Hız Çarpanı: {speedMultiplier:F2} | " +
                         $"Hareket: {actualMovement:F4}m");
            }
        }
        else
        {
            // Yeterince yakın, dur
            if (hasWalkAnimation && animator != null)
            {
                animator.SetBool(animIDWalk, false);
            }
            
            if (Time.frameCount % 120 == 0 && !isFarEnough)
            {
                Debug.Log($"🛑 {gameObject.name}: Yeterince yakın ({distanceToPlayer:F2}m < {followDistance}m), duruyor.");
            }
        }

        // Her zaman oyuncuya doğru bak
        RotateTowardsPlayer();
    }

    private void MoveTowardsPlayer(float distance, float speedMultiplier = 1f)
    {
        // Oyuncuya doğru yön
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0; // Y eksenini sıfırla (sadece yatay hareket)
        direction = direction.normalized;
        
        // Scale'e göre hızı ayarla (ama çok hızlı olmasın)
        float scaleMultiplier = Mathf.Max(transform.localScale.x, transform.localScale.z);
        // Scale çok büyükse hızı sınırla (max 1.5x - daha yavaş)
        if (scaleMultiplier > 1.5f)
        {
            scaleMultiplier = 1.5f; // Scale 4x4x4 olsa bile max 1.5x hız
        }
        float adjustedSpeed = moveSpeed * scaleMultiplier * speedMultiplier;
        
        // Hareket vektörü
        Vector3 movement = direction * adjustedSpeed * Time.deltaTime;

        // Rigidbody varsa onu kullan (fizik tabanlı hareket)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            // Rigidbody ile hareket et
            Vector3 targetPosition = transform.position + movement;
            rb.MovePosition(targetPosition);
        }
        // CharacterController varsa onu kullan
        else if (characterController != null)
        {
            // Yerçekimi ekle
            movement.y = -9.81f * Time.deltaTime;
            characterController.Move(movement);
        }
        else
        {
            // Direkt transform ile hareket et
            Vector3 newPos = transform.position + movement;
            // Y pozisyonunu koru (uçmaması için)
            newPos.y = transform.position.y;
            transform.position = newPos;
        }
        
        // Debug: Her frame hareket bilgisi (sadece ilk birkaç frame)
        if (Time.frameCount < 10)
        {
            Debug.Log($"🔍 {gameObject.name}: MoveTowardsPlayer çağrıldı! " +
                     $"Direction: {direction} | " +
                     $"Scale Multiplier: {scaleMultiplier} | " +
                     $"Adjusted Speed: {adjustedSpeed} | " +
                     $"Movement: {movement} | " +
                     $"CharacterController: {(characterController != null ? "Var" : "Yok")} | " +
                     $"Rigidbody: {(GetComponent<Rigidbody>() != null ? "Var" : "Yok")}");
        }
    }

    private void RotateTowardsPlayer()
    {
        if (target == null) return;

        // Oyuncuya doğru yön (sadece yatay)
        Vector3 direction = (target.position - transform.position);
        direction.y = 0; // Y eksenini sıfırla (sadece yatay dönüş)

        if (direction != Vector3.zero)
        {
            // Hedef rotasyon
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            // Smooth dönüş
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
        }
    }

    // Debug için - Scene view'da göster
    private void OnDrawGizmosSelected()
    {
        // Takip mesafesi (Yeşil)
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, followDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, followDistance);

        // Player'a çizgi
        if (target != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}
