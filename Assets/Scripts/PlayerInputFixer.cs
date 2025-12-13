using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

/// <summary>
/// Player Input component'inin Actions alanını otomatik olarak düzeltir
/// </summary>
public class PlayerInputFixer : MonoBehaviour
{
    [Header("Input Action Asset")]
    [Tooltip("StarterAssets Input Action Asset (otomatik bulunacak veya manuel atayın)")]
    public InputActionAsset inputActionAsset;
    
    [Header("Otomatik Düzeltme")]
    [Tooltip("Başlangıçta otomatik olarak düzelt")]
    public bool autoFixOnStart = true;
    
    private PlayerInput playerInput;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            FixPlayerInput();
        }
    }
    
    /// <summary>
    /// Player Input Actions'ı düzelt
    /// </summary>
    [ContextMenu("Player Input'u Düzelt")]
    public void FixPlayerInput()
    {
        playerInput = GetComponent<PlayerInput>();
        
        if (playerInput == null)
        {
            Debug.LogError("❌ PlayerInputFixer: PlayerInput component bulunamadı!");
            return;
        }
        
        // Eğer inputActionAsset atanmamışsa, otomatik bul
        if (inputActionAsset == null)
        {
            // Tüm InputActionAsset'leri bul ve "StarterAssets" adında olanı seç
            InputActionAsset[] allAssets = Resources.FindObjectsOfTypeAll<InputActionAsset>();
            inputActionAsset = allAssets.FirstOrDefault(asset => asset.name == "StarterAssets");
            
            // Eğer bulamazsa, tüm asset'lerde ara
            if (inputActionAsset == null)
            {
                // Resources klasöründen yükle (eğer oraya koyulmuşsa)
                inputActionAsset = Resources.Load<InputActionAsset>("StarterAssets");
            }
        }
        
        // Eğer hala bulamazsa, mevcut actions'ı kontrol et
        if (inputActionAsset == null && playerInput.actions != null)
        {
            // Eğer mevcut actions "StarterAssets" ise, onu kullan
            if (playerInput.actions.name == "StarterAssets")
            {
                inputActionAsset = playerInput.actions;
                Debug.Log("✅ PlayerInputFixer: Mevcut Actions zaten StarterAssets!");
            }
        }
        
        // Eğer hala bulamazsa, uyarı ver
        if (inputActionAsset == null)
        {
            Debug.LogWarning("⚠️ PlayerInputFixer: StarterAssets Input Action Asset bulunamadı! " +
                          "Lütfen Inspector'dan manuel olarak atayın:\n" +
                          "Project → Assets/StarterAssets/InputSystem/StarterAssets.inputactions dosyasını\n" +
                          "Player Input → Actions alanına sürükleyin.");
            return;
        }
        
        // Player Input'un Actions alanını düzelt
        if (playerInput.actions != inputActionAsset)
        {
            // Eğer şu anki actions bir Animator Controller ise, kesinlikle değiştir
            if (playerInput.actions != null && playerInput.actions.name.Contains("ThirdPerson"))
            {
                Debug.LogWarning($"⚠️ PlayerInputFixer: Yanlış Actions atanmış ({playerInput.actions.name})! Düzeltiliyor...");
            }
            
            playerInput.actions = inputActionAsset;
            Debug.Log($"✅ PlayerInputFixer: Player Input Actions düzeltildi! Asset: {inputActionAsset.name}");
        }
        else
        {
            Debug.Log($"✅ PlayerInputFixer: Player Input Actions zaten doğru! Asset: {inputActionAsset.name}");
        }
        
        // Default Map'i kontrol et
        if (string.IsNullOrEmpty(playerInput.defaultActionMap) || playerInput.defaultActionMap != "Player")
        {
            playerInput.defaultActionMap = "Player";
            Debug.Log("✅ PlayerInputFixer: Default Action Map 'Player' olarak ayarlandı.");
        }
    }
}

