using UnityEngine;

/// <summary>
/// PlayerArmature karakterinin hareket sorunlarını debug etmek için yardımcı script
/// </summary>
public class PlayerMovementDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showDebugInfo = false; // Varsayılan olarak kapalı (sol alttaki yazıyı gizler)
    public float debugUpdateInterval = 1f;
    
    private StarterAssets.ThirdPersonController thirdPersonController;
    private StarterAssets.StarterAssetsInputs input;
    private CharacterController characterController;
    private float lastDebugTime = 0f;
    
    void Start()
    {
        // Component'leri bul
        thirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();
        input = GetComponent<StarterAssets.StarterAssetsInputs>();
        characterController = GetComponent<CharacterController>();
        
        // Debug bilgisi yazdır
        Debug.Log("=== PlayerMovementDebugger Başlatıldı ===");
        CheckComponents();
        CheckCamera();
    }
    
    void Update()
    {
        if (!showDebugInfo) return;
        
        if (Time.time - lastDebugTime >= debugUpdateInterval)
        {
            lastDebugTime = Time.time;
            PrintDebugInfo();
        }
    }
    
    void CheckComponents()
    {
        Debug.Log("=== Component Kontrolü ===");
        
        if (thirdPersonController == null)
        {
            Debug.LogError("❌ ThirdPersonController bulunamadı!");
        }
        else
        {
            Debug.Log($"✅ ThirdPersonController: Aktif={thirdPersonController.enabled}");
        }
        
        if (input == null)
        {
            Debug.LogError("❌ StarterAssetsInputs bulunamadı!");
        }
        else
        {
            Debug.Log($"✅ StarterAssetsInputs: Aktif={input.enabled}, Move={input.move}, Sprint={input.sprint}");
        }
        
        if (characterController == null)
        {
            Debug.LogError("❌ CharacterController bulunamadı!");
        }
        else
        {
            Debug.Log($"✅ CharacterController: Aktif={characterController.enabled}, Enabled={characterController.enabled}");
        }
        
        // PlayerInput kontrolü
        var playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("❌ PlayerInput bulunamadı!");
        }
        else
        {
            Debug.Log($"✅ PlayerInput: Aktif={playerInput.enabled}, Actions={playerInput.actions != null}");
            if (playerInput.actions == null)
            {
                Debug.LogError("❌ PlayerInput.actions NULL! Input Action Asset atanmamış!");
            }
        }
    }
    
    void CheckCamera()
    {
        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCamera == null)
        {
            Debug.LogError("❌ MainCamera tag'li kamera bulunamadı! ThirdPersonController MainCamera'ya ihtiyaç duyuyor.");
        }
        else
        {
            Debug.Log($"✅ MainCamera bulundu: {mainCamera.name}");
        }
    }
    
    void PrintDebugInfo()
    {
        if (input != null)
        {
            Debug.Log($"Input - Move: {input.move}, Look: {input.look}, Jump: {input.jump}, Sprint: {input.sprint}");
        }
        
        if (characterController != null)
        {
            Debug.Log($"CharacterController - Velocity: {characterController.velocity}, IsGrounded: {characterController.isGrounded}");
        }
        
        if (thirdPersonController != null)
        {
            // Reflection kullanarak private field'lara erişim (sadece debug için)
            var moveSpeedField = typeof(StarterAssets.ThirdPersonController).GetField("_speed", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (moveSpeedField != null)
            {
                float speed = (float)moveSpeedField.GetValue(thirdPersonController);
                Debug.Log($"ThirdPersonController - Internal Speed: {speed}");
            }
        }
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.normal.textColor = Color.white;
        
        float yPos = 10f;
        
        GUI.Label(new Rect(10, yPos, 500, 30), $"ThirdPersonController: {(thirdPersonController != null && thirdPersonController.enabled ? "✅" : "❌")}", style);
        yPos += 25;
        
        GUI.Label(new Rect(10, yPos, 500, 30), $"StarterAssetsInputs: {(input != null && input.enabled ? "✅" : "❌")}", style);
        yPos += 25;
        
        if (input != null)
        {
            GUI.Label(new Rect(10, yPos, 500, 30), $"Input Move: {input.move}", style);
            yPos += 25;
        }
        
        GUI.Label(new Rect(10, yPos, 500, 30), $"CharacterController: {(characterController != null && characterController.enabled ? "✅" : "❌")}", style);
        yPos += 25;
        
        var playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        GUI.Label(new Rect(10, yPos, 500, 30), $"PlayerInput: {(playerInput != null && playerInput.enabled ? "✅" : "❌")}", style);
        yPos += 25;
        
        if (playerInput != null)
        {
            GUI.Label(new Rect(10, yPos, 500, 30), $"Actions Asset: {(playerInput.actions != null ? "✅" : "❌ NULL!")}", style);
            yPos += 25;
        }
        
        GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        GUI.Label(new Rect(10, yPos, 500, 30), $"MainCamera: {(mainCamera != null ? "✅" : "❌ BULUNAMADI!")}", style);
    }
}

