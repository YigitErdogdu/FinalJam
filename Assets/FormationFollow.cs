using UnityEngine;

public class FormationFollow : MonoBehaviour
{
    [Header("Ayarlar")]
    public Transform target; // Takip edilecek hedef (Mahkum)
    public float smoothSpeed = 5f; // Takip yumuþaklýðý (Daha yüksek = daha sýký takip)
    public float rotationSpeed = 10f; // Dönüþ hýzý

    private Vector3 _offset; // Baþlangýçtaki mesafe farký

    void Start()
    {
        // Oyun baþladýðýnda gardiyanýn mahkuma göre nerede durduðunu hesapla ve kaydet.
        // InverseTransformPoint: Mahkumun yönüne göre göreceli konumu bulur.
        if (target != null)
        {
            _offset = target.InverseTransformPoint(transform.position);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. POZÝSYON TAKÝBÝ
        // Mahkumun þu anki pozisyonuna ve dönüþüne göre, gardiyanýn olmasý gereken yeri hesapla.
        Vector3 targetPosition = target.TransformPoint(_offset);

        // Gardiyaný o noktaya yumuþakça hareket ettir (Lerp ile).
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // 2. DÖNÜÞ (ROTASYON) TAKÝBÝ
        // Gardiyanýn yönünü mahkumun yönüne çevir.
        Quaternion targetRotation = target.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}