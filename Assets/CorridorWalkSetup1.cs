using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Koridor boyu yürüme setup scripti
/// Level1_Base_devamsahne2 sahnesinde karakterleri ayarlar
/// Mahkum koridor boyu yürür, Guard'lar arka çaprazda takip eder
/// </summary>
public class CorridorWalkSetup : MonoBehaviour
{
    [Header("Character References")]
    [Tooltip("Mahkum karakteri (Tattooed_Character)")]
    public GameObject prisonerCharacter;
    
    [Tooltip("Guard1 (Cyborg_Sentinel) - Sol arka çapraz")]
    public GameObject guard1;
    
    [Tooltip("Guard2 (Purple_Armored) - Sağ arka çapraz")]
    public GameObject guard2;
    
    [Header("Auto Find Characters")]
    [Tooltip("Sahne içinde otomatik olarak karakterleri bul")]
    public bool autoFindCharacters = true;
    
    [Header("Waypoint Settings")]
    [Tooltip("Koridor boyu waypoint'ler - mahkum bu noktalardan geçecek")]
    public Transform[] corridorWaypoints;
    
    [Header("Movement Settings")]
    [Tooltip("Yürüme hızı")]
    public float walkSpeed = 3f;
    
    [Tooltip("Guard'ların mahkumdan uzaklığı")]
    public float guardFollowDistance = 2f;
    
    [Tooltip("Guard'ların yan mesafesi")]
    public float guardSideDistance = 1.5f;
    
    void Start()
    {
        if (autoFindCharacters)
        {
            FindAndSetupCharacters();
        }
        else
        {
            SetupCharacters();
        }
    }
    
    /// <summary>
    /// Sahne içinde karakterleri otomatik bul ve ayarla
    /// </summary>
    void FindAndSetupCharacters()
    {
        // Mahkum karakterini bul
        if (prisonerCharacter == null)
        {
            GameObject found = GameObject.Find("Tattooed_Character_St_1212141706_texture");
            if (found != null)
            {
                prisonerCharacter = found;
            }
        }
        
        // Guard1'i bul
        if (guard1 == null)
        {
            GameObject found = GameObject.Find("Cyborg_Sentinel_1212013450_texture");
            if (found != null)
            {
                guard1 = found;
            }
        }
        
        // Guard2'yi bul
        if (guard2 == null)
        {
            GameObject found = GameObject.Find("Purple_Armored_Sentin_1212013403_texture");
            if (found != null)
            {
                guard2 = found;
            }
        }
        
        SetupCharacters();
    }
    
    /// <summary>
    /// Karakterleri ayarla ve script'leri ekle
    /// </summary>
    void SetupCharacters()
    {
        // Mahkum karakterini ayarla
        if (prisonerCharacter != null)
        {
            SetupPrisoner(prisonerCharacter);
        }
        else
        {
            Debug.LogWarning("CorridorWalkSetup: Mahkum karakteri bulunamadı!");
        }
        
        // Guard1'i ayarla (Sol arka çapraz)
        if (guard1 != null)
        {
            SetupGuard(guard1, prisonerCharacter, -1f); // -1 = sol
        }
        else
        {
            Debug.LogWarning("CorridorWalkSetup: Guard1 bulunamadı!");
        }
        
        // Guard2'yi ayarla (Sağ arka çapraz)
        if (guard2 != null)
        {
            SetupGuard(guard2, prisonerCharacter, 1f); // 1 = sağ
        }
        else
        {
            Debug.LogWarning("CorridorWalkSetup: Guard2 bulunamadı!");
        }
    }
    
    /// <summary>
    /// Mahkum karakterini ayarla - koridor boyu yürüme
    /// </summary>
    void SetupPrisoner(GameObject prisoner)
    {
        // CharacterAutoWalk script'ini ekle veya güncelle
        CharacterAutoWalk autoWalk = prisoner.GetComponent<CharacterAutoWalk>();
        if (autoWalk == null)
        {
            autoWalk = prisoner.AddComponent<CharacterAutoWalk>();
        }
        
        autoWalk.walkSpeed = walkSpeed;
        autoWalk.autoWalk = true;
        autoWalk.waypoints = corridorWaypoints;
        autoWalk.loopPath = false;
        autoWalk.useNavMesh = false; // Transform ile hareket (uçmaması için)
        
        Debug.Log($"CorridorWalkSetup: Mahkum karakteri ayarlandı - {prisoner.name}");
    }
    
    /// <summary>
    /// Guard karakterini ayarla - mahkumu takip et
    /// </summary>
    void SetupGuard(GameObject guard, GameObject target, float sideOffset)
    {
        // GuardFollow script'ini ekle veya güncelle
        GuardFollow guardFollow = guard.GetComponent<GuardFollow>();
        if (guardFollow == null)
        {
            guardFollow = guard.AddComponent<GuardFollow>();
        }
        
        guardFollow.targetCharacter = target != null ? target.transform : null;
        guardFollow.followDistance = guardFollowDistance;
        guardFollow.sideDistance = guardSideDistance;
        guardFollow.sideOffset = sideOffset;
        guardFollow.moveSpeed = walkSpeed;
        guardFollow.useNavMesh = false; // Transform ile hareket (uçmaması için)
        
        string side = sideOffset > 0 ? "sağ" : "sol";
        Debug.Log($"CorridorWalkSetup: Guard ayarlandı - {guard.name} ({side} arka çapraz)");
    }
    
    /// <summary>
    /// Inspector'dan manuel olarak çağrılabilir
    /// </summary>
    [ContextMenu("Setup Characters")]
    public void ManualSetup()
    {
        FindAndSetupCharacters();
    }
    
    /// <summary>
    /// Yürümeyi başlat
    /// </summary>
    public void StartWalking()
    {
        if (prisonerCharacter != null)
        {
            CharacterAutoWalk autoWalk = prisonerCharacter.GetComponent<CharacterAutoWalk>();
            if (autoWalk != null)
            {
                autoWalk.SetAutoWalk(true);
            }
        }
    }
    
    /// <summary>
    /// Yürümeyi durdur
    /// </summary>
    public void StopWalking()
    {
        if (prisonerCharacter != null)
        {
            CharacterAutoWalk autoWalk = prisonerCharacter.GetComponent<CharacterAutoWalk>();
            if (autoWalk != null)
            {
                autoWalk.SetAutoWalk(false);
            }
        }
    }
}

