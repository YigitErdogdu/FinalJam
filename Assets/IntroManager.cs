using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class IntroManager : MonoBehaviour
{
    public VideoPlayer videoOynatici;

    [Header("Loading Ayarlarý")]
    public GameObject loadingPaneli; // Bu kutunun Unity'de çýkmasý lazým
    public TMP_Text loadingYazisi;   // Bu kutunun da çýkmasý lazým

    void Start()
    {
        videoOynatici.loopPointReached += VideoBitti;
    }

    void VideoBitti(VideoPlayer vp)
    {
        StartCoroutine(SahneYukleVeAnimasyonYap());
    }

    IEnumerator SahneYukleVeAnimasyonYap()
    {
        // Paneli aç
        if (loadingPaneli != null) loadingPaneli.SetActive(true);
        if (videoOynatici != null) videoOynatici.gameObject.SetActive(false);

        float minimumBeklemeSuresi = 3.0f;
        float toplamGecenSure = 0f;

        // 2 numaralý sahneyi (Level1_Base) yükle
        AsyncOperation operasyon = SceneManager.LoadSceneAsync(2);
        operasyon.allowSceneActivation = false;

        float yaziZamanlayici = 0;

        while (!operasyon.isDone)
        {
            float zamanFarki = Time.deltaTime;
            toplamGecenSure += zamanFarki;
            yaziZamanlayici += zamanFarki;

            // Yazý animasyonu (Eðer yazý atanmýþsa çalýþsýn)
            if (loadingYazisi != null)
            {
                if (yaziZamanlayici < 0.5f) loadingYazisi.text = "LOADING.";
                else if (yaziZamanlayici < 1.0f) loadingYazisi.text = "LOADING..";
                else if (yaziZamanlayici < 1.5f) loadingYazisi.text = "LOADING...";
                else yaziZamanlayici = 0;
            }

            if (operasyon.progress >= 0.9f && toplamGecenSure >= minimumBeklemeSuresi)
            {
                operasyon.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}