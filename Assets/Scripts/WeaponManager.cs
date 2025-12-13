using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Current Weapons")]
    private Weapon currentWeapon;
    private bool hasPurpleWeapon = false;
    private bool hasWhiteWeapon = false;
    private GameObject currentWeaponObject;

    // Silah alınabilir mi kontrol et
    public bool CanPickupWeapon(Weapon.WeaponType weaponType)
    {
        if (weaponType == Weapon.WeaponType.Purple)
        {
            return !hasPurpleWeapon;
        }
        else if (weaponType == Weapon.WeaponType.White)
        {
            return !hasWhiteWeapon;
        }
        return false;
    }

    // Silahı kaydet
    public void RegisterWeapon(Weapon weapon)
    {
        if (weapon.weaponType == Weapon.WeaponType.Purple)
        {
            hasPurpleWeapon = true;
            Debug.Log($"Mor silah kaydedildi! Hasar: {weapon.damage}");
        }
        else if (weapon.weaponType == Weapon.WeaponType.White)
        {
            hasWhiteWeapon = true;
            Debug.Log($"BEYAZ KATANA kaydedildi! Hasar: {weapon.damage} - TEK ATAR!");
        }
        
        // Önceki silahı gizle
        if (currentWeaponObject != null && currentWeaponObject != weapon.gameObject)
        {
            currentWeaponObject.SetActive(false);
        }
        
        currentWeapon = weapon;
        currentWeaponObject = weapon.gameObject;
    }

    // Şu anki silahın hasarını döndür
    public float GetCurrentWeaponDamage()
    {
        if (currentWeapon != null)
        {
            return currentWeapon.damage;
        }
        return 5f; // Silahsızken 5 hasar (yumruk)
    }

    // Silah var mı kontrolü
    public bool HasWeapon()
    {
        return currentWeapon != null;
    }

    // Hangi silah aktif?
    public Weapon.WeaponType? GetCurrentWeaponType()
    {
        if (currentWeapon != null)
            return currentWeapon.weaponType;
        return null;
    }
}
