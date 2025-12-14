using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; // TextMeshPro kullanýyorsan

public class ArenaManager : MonoBehaviour
{
    [Header("UI Elemanlarý")]
    public GameObject victoryImage; // 'yazýlar 3' görseli
    public GameObject loadingPanel; // Siyah ekran paneli
    public TMP_Text loadingText;    // 'LOADING' yazýsý

    public void BossDefeated()
    {
        StartCoroutine(WinSequence());
    }

    IEnumerator WinSequence()
    {
        // 1. Tebrikler yazýsýný göster
        victoryImage.SetActive(true);

        // Yazý ekranda 3 saniye kalsýn (okunmasý için)
        yield return new WaitForSeconds(3f);

        // 2. Loading ekranýný aç
        victoryImage.SetActive(false); // Ýstersen bunu kapatabilirsin
        loadingPanel.SetActive(true);

        // 3. Loading animasyonu (Noktalar artacak)
        string baseText = "LOADING";
        for (int i = 0; i < 3; i++) // 3 kere döngü (süre uzatýlabilir)
        {
            loadingText.text = baseText + ".";
            yield return new WaitForSeconds(0.5f);
            loadingText.text = baseText + "..";
            yield return new WaitForSeconds(0.5f);
            loadingText.text = baseText + "...";
            yield return new WaitForSeconds(0.5f);
        }

        // 4. Yeni sahneye geç
        SceneManager.LoadScene("Level3_Finale");
    }
}