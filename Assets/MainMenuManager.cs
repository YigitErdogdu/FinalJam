using UnityEngine;
using UnityEngine.SceneManagement; // Sahne deðiþimi için bu kütüphane þart

public class MainMenuManager : MonoBehaviour
{
    // Play butonuna týklandýðýnda bu fonksiyon çalýþacak
    public void OyunuBaslat()
    {
        // Build Settings'deki 1 numaralý sahneyi (oyun sahnesini) yükle
        SceneManager.LoadScene(1);
    }
}