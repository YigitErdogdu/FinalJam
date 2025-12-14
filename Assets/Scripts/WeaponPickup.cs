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
    private Vector3 weaponOriginalPosition; // Silahın ilk alındığı pozisyon
    private Quaternion weaponOriginalRotation; // Silahın ilk alındığı rotasyon
    
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
        // Elinde silah var mı kontrol et
        CheckIfHasWeapon();
        
        // Raycast ile silah ara
        CheckForWeapon();
        
        // E tuşuna basıldığında
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Elinde silah varsa bırak
            if (isHaveWeapon)
            {
                DropWeapon();
            }
            // Elinde silah yoksa ve raycast'te silah varsa al
            else if (currentWeapon != null)
            {
                PickupWeapon(currentWeapon);
            }
        }
    }
    
    void CheckIfHasWeapon()
    {
        // Right Hand'de silah var mı kontrol et
        Transform handTransform = rightHand != null ? rightHand : transform;
        
        if (handTransform.childCount > 0)
        {
            // Child'larda Weapon tag'li obje var mı?
            foreach (Transform child in handTransform)
            {
                if (child.CompareTag(weaponTag))
                {
                    isHaveWeapon = true;
                    return;
                }
            }
        }
        
        isHaveWeapon = false;
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
        
        // Weapon component'i yoksa ekle veya güncelle
        Weapon weapon = weaponObject.GetComponent<Weapon>();
        if (weapon == null)
        {
            weapon = weaponObject.AddComponent<Weapon>();
        }
        
        // Silah ismine göre hasar ve tip ayarla
        string weaponName = weaponObject.name.ToLower();
        
        // 193_Weapon → 20 hasar
        if (weaponName.Contains("193_weapon") || weaponName.Contains("193 weapon"))
        {
            weapon.weaponName = "193_Weapon";
            weapon.weaponType = Weapon.WeaponType.White;
            weapon.damage = 20f;
            Debug.Log($"✅ 193_Weapon olarak ayarlandı! Hasar: {weapon.damage}");
        }
        // Sword 13 → 10 hasar
        else if (weaponName.Contains("sword 13") || weaponName.Contains("sword13"))
        {
            weapon.weaponName = "Sword 13";
            weapon.weaponType = Weapon.WeaponType.Purple;
            weapon.damage = 10f;
            Debug.Log($"✅ Sword 13 olarak ayarlandı! Hasar: {weapon.damage}");
        }
        // Eski sistem (geriye dönük uyumluluk)
        else if (weaponName.Contains("katana") || weaponName.Contains("katana 2"))
        {
            weapon.weaponName = "Katana 2";
            weapon.weaponType = Weapon.WeaponType.White;
            weapon.damage = 100f;
            Debug.Log($"✅ Katana 2 olarak ayarlandı! Hasar: {weapon.damage}");
        }
        else if (weaponName.Contains("purple") || weaponName.Contains("purple blade"))
        {
            weapon.weaponName = "Purple Blade";
            weapon.weaponType = Weapon.WeaponType.Purple;
            weapon.damage = 10f;
            Debug.Log($"✅ Purple Blade olarak ayarlandı! Hasar: {weapon.damage}");
        }
        else
        {
            // Diğer silahlar için varsayılan değerler
            weapon.weaponName = weaponObject.name;
            weapon.weaponType = Weapon.WeaponType.Purple;
            weapon.damage = 10f;
            Debug.Log($"✅ Silah varsayılan değerlerle ayarlandı! Hasar: {weapon.damage}");
        }
        
        // Silahın orijinal pozisyonunu ve rotasyonunu kaydet (sadece ilk alışta)
        if (!isHaveWeapon)
        {
            weaponOriginalPosition = weaponObject.transform.position;
            weaponOriginalRotation = weaponObject.transform.rotation;
            Debug.Log($"📍 Orijinal pozisyon kaydedildi: {weaponOriginalPosition}");
        }
        
        // Rigidbody varsa kinematic yap (fizik devre dışı)
        Rigidbody rb = weaponObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Önce hızları sıfırla (kinematic olmadan önce)
            if (!rb.isKinematic)
            {
                rb.velocity = Vector3.zero; // Hızı sıfırla
                rb.angularVelocity = Vector3.zero; // Açısal hızı sıfırla
            }
            // Sonra kinematic yap
            rb.isKinematic = true;
        }
        
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
        
        // WeaponManager'a kaydet (hasar verme için gerekli)
        WeaponManager weaponManager = GetComponent<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.RegisterWeapon(weapon);
            Debug.Log($"✅ WeaponManager'a kaydedildi! Hasar: {weapon.damage}");
        }
        else
        {
            Debug.LogWarning("⚠️ WeaponManager bulunamadı! Hasar verme çalışmayabilir!");
        }
        
        Debug.Log($"✅✅✅ {weaponObject.name} başarıyla alındı! Sağ ele eklendi. Hasar: {weapon.damage}");
        
        currentWeapon = null;
        isHaveWeapon = true;
    }
    
    void DropWeapon()
    {
        Transform handTransform = rightHand != null ? rightHand : transform;
        
        // Hand'deki silahı bul
        Transform weaponTransform = null;
        foreach (Transform child in handTransform)
        {
            if (child.CompareTag(weaponTag))
            {
                weaponTransform = child;
                break;
            }
        }
        
        if (weaponTransform == null)
        {
            Debug.LogWarning("⚠️ Elinde silah yok!");
            isHaveWeapon = false;
            return;
        }
        
        GameObject weaponObject = weaponTransform.gameObject;
        Debug.Log($"🎯 Silah bırakılıyor: {weaponObject.name}");
        
        // Parent'ı kaldır (dünyaya bırak)
        weaponObject.transform.SetParent(null);
        
        // Pozisyonu ilk alındığı yere koy
        weaponObject.transform.position = weaponOriginalPosition;
        
        // Rotasyonu ilk alındığı rotasyona geri döndür
        weaponObject.transform.rotation = weaponOriginalRotation;
        
        // Rigidbody varsa önce kinematic yap, sonra pozisyonu ayarla, sonra kinematic'i kapat
        Rigidbody rb = weaponObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Önce kinematic yap (pozisyon ayarlanırken fizik devre dışı)
            rb.isKinematic = true;
            
            // Pozisyonu ayarla
            rb.position = weaponOriginalPosition;
            rb.rotation = weaponOriginalRotation;
            
            // Hızları sıfırla
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            // Sonra kinematic'i kapat (fizik aktif)
            rb.isKinematic = false;
        }
        
        // Collider'ı tekrar aç
        Collider weaponCollider = weaponObject.GetComponent<Collider>();
        if (weaponCollider != null)
        {
            weaponCollider.enabled = true;
        }
        
        Debug.Log($"✅✅✅ {weaponObject.name} bırakıldı!");
        isHaveWeapon = false;
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
