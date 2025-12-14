using UnityEngine;

public class ItemSelection : MonoBehaviour
{
    // Eþyalarýmýzý buraya sürükleyeceðiz (Zýrh, Kýlýç, Balta vb.)
    public GameObject[] items;

    // Þu an kaçýncý sýradaki eþyanýn seçili olduðunu tutar
    private int currentIndex = 0;

    void Start()
    {
        // Oyun baþlayýnca listeyi güncelle
        UpdateSelection();
    }

    void Update()
    {
        // D tuþuna (veya Sað Ok) basýnca ÝLERÝ git
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex++;
            // Eðer listenin sonuna geldiysek baþa dön
            if (currentIndex >= items.Length)
                currentIndex = 0;

            UpdateSelection();
        }

        // A tuþuna (veya Sol Ok) basýnca GERÝ git
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex--;
            // Eðer listenin baþýna geldiysek en sona git
            if (currentIndex < 0)
                currentIndex = items.Length - 1;

            UpdateSelection();
        }
    }

    void UpdateSelection()
    {
        // Tüm eþyalarý kapat, sadece sýrasý geleni aç
        for (int i = 0; i < items.Length; i++)
        {
            if (i == currentIndex)
                items[i].SetActive(true); // Seçili olan görünsün
            else
                items[i].SetActive(false); // Diðerleri gizlensin
        }
    }
}