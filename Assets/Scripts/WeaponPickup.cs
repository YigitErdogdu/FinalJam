using UnityEngine;

/// <summary>
/// Raycast ile "Weapon" tag'li objeleri bulup sağ eline ekler
/// </summary>
public class WeaponPickup : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("Raycast mesafesi")]
    [SerializeField] private float pickupRange = 5f;
    
    [Header("Hand Settings")]
    [Tooltip("Sağ el transform (otomatik bulunur)")]
    [SerializeField] private Transform rightHand;
    
    [Header("Camera")]
    [Tooltip("Ana kamera (raycast için - otomatik bulunur)")]
    [SerializeField] private Camera mainCamera;
    
    [Header("Weapon Tag")]
    [Tooltip("Hangi tag'li objeler silah olarak algılanacak?")]
    [SerializeField] private string weaponTag = "Weapon";
    
    private GameObject currentWeapon;

    private bool isHaveWeapon;
    
    void Start()
    {
        // Kamera otomatik bul
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // Right Hand otomatik bul
        if (rightHand == null)
        {
            rightHand = FindChildRecursive(transform, "Right_Hand");
            if (rightHand == null)
            {
                rightHand = FindChildRecursive(transform, "RightHand");
            }
        }
    }
    
    void Update()
    {
        // Raycast ile silah ara
        CheckForWeapon();
        
        // E tuşuna basıldığında silahı al
        if (Input.GetKeyDown(KeyCode.E) && currentWeapon != null && !isHaveWeapon)
        {
            PickupWeapon(currentWeapon);
            isHaveWeapon = true;
        }
    }
    
    void CheckForWeapon()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }
        
        // Ekranın ortasından raycast
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            GameObject hitObject = hit.collider.gameObject;
            
            // Weapon tag'i kontrol et
            if (hitObject.CompareTag(weaponTag))
            {
                currentWeapon = hitObject;
            }
            
        }
        else
        {
            currentWeapon = null;
        }
    }
    
    void PickupWeapon(GameObject weaponObject)
    {
        if (weaponObject == null)
        {
            Debug.LogError("WeaponPickup: Silah objesi null!");
            return;
        }
        
        // Right Hand yoksa player'ın transform'unu kullan
        Transform handTransform = rightHand;
        if (handTransform == null)
        {
            handTransform = transform;
            Debug.LogWarning("⚠️ Right Hand bulunamadı! Silah player'ın transform'una eklenecek.");
        }
        
        Debug.Log($"🎯 Silah alınıyor: {weaponObject.name}");
        
        
        
        // Collider'ı kapat
        Collider weaponCollider = weaponObject.GetComponent<Collider>();
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
        
        // Silahı sağ elin child'ı yap
        weaponObject.transform.SetParent(handTransform);
        
        // Pozisyonu ve rotasyonu ayarla (el pozisyonuna ışınla)
        weaponObject.transform.localPosition = Vector3.zero;
        weaponObject.transform.localRotation = Quaternion.Euler(0, 0, -90);
        weaponObject.transform.localScale = Vector3.one;
        
        // Silahı görünür yap
        weaponObject.SetActive(true);
        
        Debug.Log($"✅✅✅ {weaponObject.name} başarıyla alındı! Sağ ele eklendi.");
        
        currentWeapon = null;
    }
    
    // Recursive olarak child'larda ara
    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name || child.name.Contains(name))
            {
                return child;
            }
            Transform found = FindChildRecursive(child, name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
    
    // Debug için raycast çizgisi
    void OnDrawGizmos()
    {
        if (mainCamera == null) return;
        
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Gizmos.color = Color.green;
        Gizmos.DrawRay(ray.origin, ray.direction * pickupRange);
    }
}
