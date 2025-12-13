using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    // Ekrana týklayýnca çalýþacak
    public void OyunuBasaSar()
    {
        // 0 numaralý sahneye (Main_Scene / Menü) döner
        SceneManager.LoadScene(0);
    }
}