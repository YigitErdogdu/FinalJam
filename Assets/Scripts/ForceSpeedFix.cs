using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Hızı zorla ayarlar - sadece belirli sahnelerde aktif
/// </summary>
public class ForceSpeedFix : MonoBehaviour
{
    [Header("Zorla Hız Ayarları")]
    [Tooltip("Normal yürüme hızı (m/s) - Her frame güncellenir")]
    public float forceMoveSpeed = 500f;
    
    [Tooltip("Koşma hızı (m/s) - Her frame güncellenir")]
    public float forceSprintSpeed = 800f;
    
    [Header("Sahne Ayarları")]
    [Tooltip("Sadece bu sahnelerde aktif olsun (boşsa tüm sahnelerde aktif)")]
    public string[] activeScenes = { "Level1_Base 1", "Level1_Base1" };
    
    [Tooltip("Sadece bu sahnelerde aktif olsun mu?")]
    public bool onlyInSpecificScenes = true;
    
    [Header("Ayarlar")]
    [Tooltip("Her frame hızı zorla ayarla (override'ları ezer)")]
    public bool forceEveryFrame = true;
    
    private StarterAssets.ThirdPersonController thirdPersonController;
    private bool isActive = false;
    
    void Start()
    {
        // Sahne kontrolü
        CheckScene();
        
        if (isActive)
        {
            thirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();
            ApplySpeed();
        }
        else
        {
            // Bu sahne aktif değilse script'i devre dışı bırak
            this.enabled = false;
        }
    }
    
    void Update()
    {
        if (isActive && forceEveryFrame && thirdPersonController != null)
        {
            ApplySpeed();
        }
    }
    
    void CheckScene()
    {
        if (!onlyInSpecificScenes)
        {
            // Tüm sahnelerde aktif
            isActive = true;
            return;
        }
        
        // Sadece belirtilen sahnelerde aktif
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        foreach (string sceneName in activeScenes)
        {
            if (currentSceneName.Contains(sceneName) || currentSceneName == sceneName)
            {
                isActive = true;
                Debug.Log($"✅ ForceSpeedFix: {currentSceneName} sahnesinde aktif! Hız: {forceMoveSpeed}/{forceSprintSpeed}");
                return;
            }
        }
        
        // Bu sahne listede değil, devre dışı
        isActive = false;
        Debug.Log($"ℹ️ ForceSpeedFix: {currentSceneName} sahnesinde devre dışı (normal hız kullanılacak)");
    }
    
    void ApplySpeed()
    {
        if (thirdPersonController == null)
        {
            thirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();
            if (thirdPersonController == null)
            {
                Debug.LogError("❌ ForceSpeedFix: ThirdPersonController bulunamadı!");
                return;
            }
        }
        
        // Hızı zorla ayarla
        thirdPersonController.MoveSpeed = forceMoveSpeed;
        thirdPersonController.SprintSpeed = forceSprintSpeed;
    }
    
    [ContextMenu("Hızı Şimdi Ayarla")]
    void ApplySpeedNow()
    {
        ApplySpeed();
        Debug.Log($"✅ ForceSpeedFix: Hız zorla ayarlandı! Move={forceMoveSpeed}, Sprint={forceSprintSpeed}");
    }
}

