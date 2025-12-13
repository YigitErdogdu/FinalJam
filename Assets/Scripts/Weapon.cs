using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Info")]
    public string weaponName = "Weapon";
    
    [Tooltip("Bu silahın verdiği hasar")]
    public float damage = 10f;
    
    [Header("Weapon Type")]
    [Tooltip("Silah tipi: Purple (Mor) veya White (Beyaz)")]
    public WeaponType weaponType = WeaponType.Purple;

    public enum WeaponType
    {
        Purple,  // Mor silah - Zayıf (10 hasar)
        White    // Beyaz silah - Güçlü (100 hasar - tek atar)
    }
}
