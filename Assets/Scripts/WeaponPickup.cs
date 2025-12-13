using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("Raycast mesafesi")]
    [SerializeField] private float pickupRange = 3f;
    
    [Tooltip("Hangi layer'lar silah olarak algılanacak")]
    [SerializeField] private LayerMask weaponLayer;
    
    [Header("Hand Settings")]
    [Tooltip("Sağ el transform (silahlar buraya eklenecek)")]
    [SerializeField] private Transform rightHand;
    
    [Header("Camera")]
    [Tooltip("Ana kamera (raycast için)")]
    [SerializeField] private Camera mainCamera;
    
    [Header("UI")]
    [Tooltip("Silah alınabilir mesajı")]
    [SerializeField] private string pickupMessage = "Silahı almak için E'ye bas";
    
    private GameObject currentLookingAtWeapon;
    private WeaponManager weaponManager;

    void Start()
    {
        // Kamera yoksa otomatik bul
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // WeaponManager'ı bul
        weaponManager = GetComponent<WeaponManager>();
        if (weaponManager == null)
        {
            Debug.LogWarning("WeaponManager bulunamadı!");
        }
    }

    void Update()
    {
        // Ekranın ortasından raycast at
        CheckForWeapon();
        
        // E tuşuna basıldığında silahı al
        if (Input.GetKeyDown(KeyCode.E) && currentLookingAtWeapon != null)
        {
            PickupWeapon(currentLookingAtWeapon);
        }
    }

    void CheckForWeapon()
    {
        // Ekranın ortasından ray oluştur
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        // Raycast at
        if (Physics.Raycast(ray, out hit, pickupRange, weaponLayer))
        {
            // Silah mı?
            Weapon weapon = hit.collider.GetComponent<Weapon>();
            if (weapon != null)
            {
                currentLookingAtWeapon = hit.collider.gameObject;
                Debug.Log($"Silaha bakıyorsun: {weapon.weaponName} - {pickupMessage}");
            }
            else
            {
                currentLookingAtWeapon = null;
            }
        }
        else
        {
            currentLookingAtWeapon = null;
        }
    }

    void PickupWeapon(GameObject weaponObject)
    {
        if (rightHand == null)
        {
            Debug.LogError("Right Hand atanmamış!");
            return;
        }
        
        Weapon weapon = weaponObject.GetComponent<Weapon>();
        if (weapon == null) return;
        
        // WeaponManager'a bildir
        if (weaponManager != null)
        {
            // Aynı tipte silah var mı kontrol et
            if (!weaponManager.CanPickupWeapon(weapon.weaponType))
            {
                Debug.Log($"{weapon.weaponType} silahını zaten aldın!");
                return;
            }
            
            weaponManager.RegisterWeapon(weapon);
        }
        
        // Silahı sağ elin child'ı yap
        weaponObject.transform.SetParent(rightHand);
        
        // Pozisyonu sıfırla
        weaponObject.transform.localPosition = Vector3.zero;
        weaponObject.transform.localRotation = Quaternion.identity;
        weaponObject.transform.localScale = Vector3.one;
        
        // Collider'ı kapat
        Collider weaponCollider = weaponObject.GetComponent<Collider>();
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
        
        // Rigidbody varsa kinematic yap
        Rigidbody rb = weaponObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        Debug.Log($"✅ {weapon.weaponName} alındı! Hasar: {weapon.damage}");
        currentLookingAtWeapon = null;
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
