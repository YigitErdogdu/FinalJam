using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Loading Ayarlarý")]
    public GameObject loadingPaneli;
    public TMP_Text loadingYazisi;

    public void OyunuBaslat()
    {
        StartCoroutine(SahneYukleVeAnimasyonYap());
    }

    IEnumerator SahneYukleVeAnimasyonYap()
    {
        loadingPaneli.SetActive(true);

        // EN AZ KAÇ SANÝYE BEKLESÝN? (Buradan ayarlayabilirsin)
        float minimumBeklemeSuresi = 3.0f;
        float toplamGecenSure = 0f;

        AsyncOperation operasyon = SceneManager.LoadSceneAsync(1);
        operasyon.allowSceneActivation = false; // Otomatik geçiþi engelle

        float yaziZamanlayici = 0;

        // Hem yükleme bitene kadar HEM DE süremiz dolana kadar dön
        while (!operasyon.isDone)
        {
            float zamanFarki = Time.deltaTime;
            toplamGecenSure += zamanFarki;
            yaziZamanlayici += zamanFarki;

            // --- YAZI ANÝMASYONU ---
            if (yaziZamanlayici < 0.5f)
                loadingYazisi.text = "LOADING.";
            else if (yaziZamanlayici < 1.0f)
                loadingYazisi.text = "LOADING..";
            else if (yaziZamanlayici < 1.5f)
                loadingYazisi.text = "LOADING...";
            else
                yaziZamanlayici = 0;
            // -----------------------

            // Yükleme %90'a geldiyse (%0.9) VE beklediðimiz süre dolduysa geçiþ yap
            if (operasyon.progress >= 0.9f && toplamGecenSure >= minimumBeklemeSuresi)
            {
                operasyon.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}