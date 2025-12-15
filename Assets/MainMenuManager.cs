using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Loading Ayarlarý")]
    public GameObject loadingPaneli;
    public TMP_Text loadingYazisi;

    [Header("Menü Ayarlarý")]
    public GameObject howToPlayPaneli; // Yeni Panelimiz


    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible=true;
    }

    public void OyunuBaslat()
    {
        StartCoroutine(SahneYukleVeAnimasyonYap());
    }

    // --- YENÝ EKLENEN FONKSÝYONLAR ---

    // 1. How To Play butonuna basýnca çalýþacak
    public void HowToPlayAc()
    {
        howToPlayPaneli.SetActive(true); // Paneli açar
    }

    // 2. Panelin içindeki Quit/Geri butonuna basýnca çalýþacak
    public void HowToPlayKapat()
    {
        howToPlayPaneli.SetActive(false); // Paneli kapatýr, menüye döner
    }

    // 3. Ana menüdeki Quit butonuna basýnca çalýþacak
    public void OyundanCik()
    {
        Debug.Log("Oyundan çýkýldý!"); // Editörde çýkmaz, sadece konsola yazar
        Application.Quit(); // Gerçek oyunda (Build'de) oyunu kapatýr
    }

    // ---------------------------------

    IEnumerator SahneYukleVeAnimasyonYap()
    {
        // (Burasý senin eski kodun, aynen kalsýn)
        loadingPaneli.SetActive(true);
        float minimumBeklemeSuresi = 3.0f;
        float toplamGecenSure = 0f;
        AsyncOperation operasyon = SceneManager.LoadSceneAsync(1);
        operasyon.allowSceneActivation = false;
        float yaziZamanlayici = 0;

        while (!operasyon.isDone)
        {
            float zamanFarki = Time.deltaTime;
            toplamGecenSure += zamanFarki;
            yaziZamanlayici += zamanFarki;

            if (yaziZamanlayici < 0.5f) loadingYazisi.text = "LOADING.";
            else if (yaziZamanlayici < 1.0f) loadingYazisi.text = "LOADING..";
            else if (yaziZamanlayici < 1.5f) loadingYazisi.text = "LOADING...";
            else yaziZamanlayici = 0;

            if (operasyon.progress >= 0.9f && toplamGecenSure >= minimumBeklemeSuresi)
            {
                operasyon.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}