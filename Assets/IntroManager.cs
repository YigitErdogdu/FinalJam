using UnityEngine;
using UnityEngine.Video; // Video iþlemleri için gerekli kütüphane
using UnityEngine.SceneManagement; // Sahne geçiþi için gerekli

public class IntroManager : MonoBehaviour
{
    public VideoPlayer videoOynatici; // Editörden buraya atama yapacaðýz

    void Start()
    {
        // "Video bittiði an 'SiradakiSahneyeGec' fonksiyonunu çalýþtýr" emrini veriyoruz.
        videoOynatici.loopPointReached += SiradakiSahneyeGec;
    }

    void SiradakiSahneyeGec(VideoPlayer vp)
    {
        // 2 numaralý sahneyi (Level1_Base) yükle
        SceneManager.LoadScene(2);
    }
}