using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    // Ekrana týklayýnca çalýþacak

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }
    public void OyunuBasaSar()
    {
        // 0 numaralý sahneye (Main_Scene / Menü) döner
        SceneManager.LoadScene(0);
    }
}