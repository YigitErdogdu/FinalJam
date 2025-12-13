using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("Raycast mesafesi")]
    [SerializeField] private float pickupRange = 5f;
    
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
    private GameObject lastCheckedWeapon; // Önceki frame'deki silahı hatırla

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
        
        // Sadece arena sahnesinde weapon pickup aktif olsun
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (!currentScene.Contains("Arena") && !currentScene.Contains("Level2") && !currentScene.Contains("Level3"))
        {
            // Arena sahnesi değilse, bu script'i deaktif et
            this.enabled = false;
            Debug.Log($"WeaponPickup deaktif edildi. Sahne: {currentScene}");
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
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("WeaponPickup: Kamera bulunamadı!");
                return;
            }
        }
        
        // Ekranın ortasından ray oluştur
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        // Raycast at (tüm collider'lara)
        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            GameObject hitObject = hit.collider.gameObject;
            
            // Tag kontrolü - "PurpleBlade" veya "Katana" tag'ine sahip mi?
            bool isWeaponTag = hitObject.CompareTag("PurpleBlade") || hitObject.CompareTag("Katana");
            
            // Veya Weapon component'i var mı?
            Weapon weapon = hitObject.GetComponent<Weapon>();
            
            // Eğer tag varsa ama Weapon component yoksa, ekle
            if (isWeaponTag && weapon == null)
            {
                weapon = hitObject.AddComponent<Weapon>();
                if (hitObject.CompareTag("PurpleBlade"))
                {
                    weapon.weaponType = Weapon.WeaponType.Purple;
                    weapon.weaponName = "Purple Blade";
                    weapon.damage = 10f;
                    Debug.Log("✅ PurpleBlade tag'i bulundu, Weapon component eklendi!");
                }
                else if (hitObject.CompareTag("Katana"))
                {
                    weapon.weaponType = Weapon.WeaponType.White;
                    weapon.weaponName = "Katana";
                    weapon.damage = 100f;
                    Debug.Log("✅ Katana tag'i bulundu, Weapon component eklendi!");
                }
            }
            
            if (isWeaponTag || weapon != null)
            {
                currentLookingAtWeapon = hitObject;
                
                // Sadece yeni bir silaha bakıldığında log yaz
                if (lastCheckedWeapon != hitObject)
                {
                    string weaponName = weapon != null ? weapon.weaponName : hitObject.tag;
                    Debug.Log($"✅ Silaha bakıyorsun: {weaponName} - {pickupMessage}");
                    lastCheckedWeapon = hitObject;
                }
            }
            else
            {
                currentLookingAtWeapon = null;
                if (lastCheckedWeapon != null)
                {
                    lastCheckedWeapon = null;
                }
            }
        }
        else
        {
            currentLookingAtWeapon = null;
        }
    }

    void PickupWeapon(GameObject weaponObject)
    {
        if (weaponObject == null)
        {
            Debug.LogError("WeaponPickup: Silah objesi null!");
            return;
        }
        
        if (rightHand == null)
        {
            Debug.LogError("WeaponPickup: Right Hand atanmamış! Player GameObject'inde WeaponPickup script'inde Right Hand transform'unu atayın!");
            return;
        }
        
        Weapon weapon = weaponObject.GetComponent<Weapon>();
        if (weapon == null)
        {
            Debug.LogError($"WeaponPickup: {weaponObject.name} objesinde Weapon component yok!");
            return;
        }
        
        // WeaponManager'a bildir
        if (weaponManager == null)
        {
            weaponManager = GetComponent<WeaponManager>();
            if (weaponManager == null)
            {
                Debug.LogError("WeaponPickup: WeaponManager bulunamadı! Player GameObject'ine WeaponManager script'i ekleyin!");
                return;
            }
        }
        
        // Aynı tipte silah var mı kontrol et
        if (!weaponManager.CanPickupWeapon(weapon.weaponType))
        {
            Debug.Log($"⚠️ {weapon.weaponType} silahını zaten aldın!");
            return;
        }
        
        // Silahı kaydet
        weaponManager.RegisterWeapon(weapon);
        
        // Rigidbody varsa kinematic yap (önce)
        Rigidbody rb = weaponObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        // Collider'ı kapat
        Collider weaponCollider = weaponObject.GetComponent<Collider>();
        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
        
        // Silahı sağ elin child'ı yap
        weaponObject.transform.SetParent(rightHand);
        
        // Pozisyonu ve rotasyonu ayarla
        weaponObject.transform.localPosition = Vector3.zero;
        weaponObject.transform.localRotation = Quaternion.identity;
        weaponObject.transform.localScale = Vector3.one;
        
        // Silahı görünür yap
        weaponObject.SetActive(true);
        
        // Renderer'ları kontrol et
        Renderer[] renderers = weaponObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;
        }
        
        Debug.Log($"✅ {weapon.weaponName} başarıyla alındı! Hasar: {weapon.damage} - Pozisyon: {weaponObject.transform.position}");
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
