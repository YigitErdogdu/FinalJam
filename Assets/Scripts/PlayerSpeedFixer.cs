using UnityEngine;

/// <summary>
/// PlayerArmature'nin hızını ve scale'ini düzeltmek için yardımcı script
/// </summary>
public class PlayerSpeedFixer : MonoBehaviour
{
    [Header("Scale Ayarları")]
    [Tooltip("Karakterin scale'ini ayarla (1 = normal boyut)")]
    public Vector3 targetScale = Vector3.one;
    
    [Header("Hız Ayarları")]
    [Tooltip("Normal yürüme hızı (m/s)")]
    public float walkSpeed = 20f;
    
    [Tooltip("Koşma hızı (m/s)")]
    public float sprintSpeed = 35f;
    
    [Header("Otomatik Düzeltme")]
    [Tooltip("Başlangıçta otomatik olarak scale ve hızı düzelt")]
    public bool autoFixOnStart = true;
    
    private StarterAssets.ThirdPersonController thirdPersonController;
    
    void Start()
    {
        thirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();
        
        if (autoFixOnStart)
        {
            FixScale();
            FixSpeed();
        }
    }
    
    /// <summary>
    /// Scale'i düzelt (karakterin boyutunu ayarla)
    /// </summary>
    [ContextMenu("Scale'i Düzelt")]
    public void FixScale()
    {
        transform.localScale = targetScale;
        Debug.Log($"✅ PlayerArmature scale düzeltildi: {targetScale}");
    }
    
    /// <summary>
    /// Hızı düzelt
    /// </summary>
    [ContextMenu("Hızı Düzelt")]
    public void FixSpeed()
    {
        if (thirdPersonController != null)
        {
            thirdPersonController.MoveSpeed = walkSpeed;
            thirdPersonController.SprintSpeed = sprintSpeed;
            Debug.Log($"✅ PlayerArmature hızı düzeltildi: Walk={walkSpeed}, Sprint={sprintSpeed}");
        }
        else
        {
            Debug.LogWarning("❌ ThirdPersonController bulunamadı!");
        }
    }
    
    /// <summary>
    /// Her ikisini de düzelt
    /// </summary>
    [ContextMenu("Hepsini Düzelt")]
    public void FixAll()
    {
        FixScale();
        FixSpeed();
    }
}

