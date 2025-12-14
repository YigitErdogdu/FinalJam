using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelEndTrigger : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject loadingPanel; // Kopyaladýðýn Loading Panelini buraya sürükle
    public int nextSceneIndex;      // Bir sonraki sahnenin Build Index numarasý (Örn: 3)
    public float waitTime = 3f;     // Ne kadar beklesin?

    private void OnTriggerEnter(Collider other)
    {
        // Oyuncunun Tag'inin "Player" olduðundan emin ol!
        if (other.CompareTag("Player"))
        {
            StartCoroutine(LoadNextLevelRoutine());
        }
    }

    IEnumerator LoadNextLevelRoutine()
    {
        // 1. Paneli aç
        if (loadingPanel != null) loadingPanel.SetActive(true);

        // 2. Bekle
        yield return new WaitForSeconds(waitTime);

        // 3. Sahneyi Yükle
        SceneManager.LoadScene(nextSceneIndex);
    }
}