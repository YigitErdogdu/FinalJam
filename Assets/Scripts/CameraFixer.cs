using UnityEngine;
using StarterAssets;

/// <summary>
/// Kamera hareket sorunlarını otomatik düzeltir
/// </summary>
public class CameraFixer : MonoBehaviour
{
    void Start()
    {
        ThirdPersonController controller = GetComponent<ThirdPersonController>();
        if (controller == null)
        {
            Debug.LogError("❌ CameraFixer: ThirdPersonController bulunamadı!");
            return;
        }

        // LockCameraPosition'ı kontrol et ve düzelt
        if (controller.LockCameraPosition)
        {
            Debug.LogWarning("⚠️ LockCameraPosition = true! Kapatılıyor...");
            controller.LockCameraPosition = false;
        }

        // CinemachineCameraTarget'ı kontrol et
        if (controller.CinemachineCameraTarget == null)
        {
            Debug.LogWarning("⚠️ CinemachineCameraTarget NULL! Aranıyor...");
            
            // PlayerArmature altında CameraRoot'u bul
            Transform cameraRoot = transform.Find("CameraRoot");
            if (cameraRoot == null)
            {
                // PlayerArmature altında başka bir yerde olabilir
                cameraRoot = transform.Find("PlayerCameraRoot");
            }
            
            if (cameraRoot == null)
            {
                // Hiç yoksa oluştur
                GameObject newCameraRoot = new GameObject("CameraRoot");
                newCameraRoot.transform.SetParent(transform);
                newCameraRoot.transform.localPosition = new Vector3(0, 1.8f, 0);
                cameraRoot = newCameraRoot.transform;
                Debug.Log("✅ CameraRoot oluşturuldu!");
            }
            
            controller.CinemachineCameraTarget = cameraRoot.gameObject;
            Debug.Log($"✅ CinemachineCameraTarget atandı: {cameraRoot.name}");
        }

        // Cursor'ı kilitle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("✅ Cursor kilitlendi!");
    }

    void Update()
    {
        // ESC tuşuna basılırsa cursor'ı serbest bırak
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Mouse'a tıklanırsa tekrar kilitle
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

