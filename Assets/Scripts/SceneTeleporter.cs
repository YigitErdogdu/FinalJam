using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// BASİT TELEPORT - Basamak/kutu üzerine çıkınca loading screen ile sahne değiştirir
/// Loading screen'ler hazır, sadece tetikleme yapıyor
/// </summary>
public class SceneTeleporter : MonoBehaviour
{
    [Header("Teleport Ayarları")]
    [Tooltip("Hangi sahneye teleport edilecek? (Sahne adı)")]
    public string targetSceneName = "";
    [Tooltip("VEYA sahne index'i kullan (Build Settings'teki sıra numarası)")]
    public int targetSceneIndex = -1;
    
    [Header("Loading Screen (Otomatik Bulunur)")]
    [Tooltip("Loading panel GameObject - Boş bırakırsan otomatik bulur")]
    public GameObject loadingPanel;
    [Tooltip("Loading yazısı - Boş bırakırsan otomatik bulur")]
    public TMP_Text loadingText;
    
    [Header("Ayarlar")]
    [Tooltip("Minimum loading süresi (saniye)")]
    public float minimumLoadingTime = 2f;
    
    private bool isTeleporting = false;
    
    void Start()
    {
        // Loading panel'i otomatik bul
        if (loadingPanel == null)
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                Transform panel = canvas.transform.Find("LoadingPanel");
                if (panel != null)
                {
                    loadingPanel = panel.gameObject;
                    Debug.Log($"✅ LoadingPanel otomatik bulundu!");
                }
            }
        }
        
        // Loading text'i otomatik bul
        if (loadingText == null && loadingPanel != null)
        {
            loadingText = loadingPanel.GetComponentInChildren<TMP_Text>();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Sadece Player tetiklesin
        if (!other.CompareTag("Player"))
        {
            return;
        }
        
        // Zaten teleport ediliyorsa tekrar tetikleme
        if (isTeleporting)
        {
            return;
        }
        
        // Teleport başlat
        StartCoroutine(TeleportToScene());
    }
    
    IEnumerator TeleportToScene()
    {
        isTeleporting = true;
        
        Debug.Log($"🚀 Teleport başlatılıyor... Hedef: {GetTargetSceneName()}");
        
        // Loading screen'i göster
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }
        
        // Sahne yükleme başlat
        AsyncOperation operation;
        
        if (targetSceneIndex >= 0)
        {
            operation = SceneManager.LoadSceneAsync(targetSceneIndex);
        }
        else if (!string.IsNullOrEmpty(targetSceneName))
        {
            operation = SceneManager.LoadSceneAsync(targetSceneName);
        }
        else
        {
            Debug.LogError("❌ SceneTeleporter: Hedef sahne belirtilmemiş! targetSceneName veya targetSceneIndex ayarlayın!");
            isTeleporting = false;
            yield break;
        }
        
        operation.allowSceneActivation = false;
        
        float elapsedTime = 0f;
        float textTimer = 0f;
        
        // Loading animasyonu (hazır sisteminizle aynı)
        while (!operation.isDone)
        {
            elapsedTime += Time.deltaTime;
            textTimer += Time.deltaTime;
            
            // Loading yazısı animasyonu
            if (loadingText != null)
            {
                if (textTimer < 0.5f) loadingText.text = "LOADING.";
                else if (textTimer < 1.0f) loadingText.text = "LOADING..";
                else if (textTimer < 1.5f) loadingText.text = "LOADING...";
                else textTimer = 0f;
            }
            
            // Minimum süre geçtiyse ve yükleme tamamlandıysa sahneyi aktif et
            if (operation.progress >= 0.9f && elapsedTime >= minimumLoadingTime)
            {
                operation.allowSceneActivation = true;
            }
            
            yield return null;
        }
        
        Debug.Log($"✅ Teleport tamamlandı!");
    }
    
    string GetTargetSceneName()
    {
        if (targetSceneIndex >= 0)
        {
            return $"Scene Index {targetSceneIndex}";
        }
        else if (!string.IsNullOrEmpty(targetSceneName))
        {
            return targetSceneName;
        }
        return "Belirtilmemiş";
    }
    
    // Debug için - Scene view'da göster
    void OnDrawGizmosSelected()
    {
        // Trigger alanını göster
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Turuncu
            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphere = col as SphereCollider;
                Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
            }
        }
    }
}

