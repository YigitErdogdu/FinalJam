using UnityEngine;

/// <summary>
/// Silahların collision'ını kontrol eder
/// Silahın içinden geçilmemesi için collider ekler
/// </summary>
[RequireComponent(typeof(Collider))]
public class WeaponCollision : MonoBehaviour
{
    void Start()
    {
        // Collider yoksa ekle
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // BoxCollider ekle (silah tipine göre ayarlanabilir)
            col = gameObject.AddComponent<BoxCollider>();
        }
        
        // Collider'ı trigger yap (fiziksel engel olmasın ama algılanabilsin)
        col.isTrigger = false; // Fiziksel collision için false
        
        // Rigidbody yoksa ekle (fizik için)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; // Kinematic yap (yerçekimi etkilemesin)
            rb.useGravity = false;
        }
        
        Debug.Log($"WeaponCollision: {gameObject.name} için collider ayarlandı");
    }
}

