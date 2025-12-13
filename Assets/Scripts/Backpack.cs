using System.Collections.Generic;
using UnityEngine;

public class Backpack : MonoBehaviour
{
    public List<string> collectedWeapons = new List<string>();

    public void AddItem(Weapon weapon)
    {
        collectedWeapons.Add(weapon.weaponName);
        Debug.Log("Picked up: " + weapon.weaponName);
        
        // You can add logic here to instantiate the weapon in the backpack visual or UI
    }
}
