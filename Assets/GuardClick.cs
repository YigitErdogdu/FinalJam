using UnityEngine;
using UnityEngine.UI; // UI iþlemleri için

public class GuardClick : MonoBehaviour
{
    [Header("Açýlacak Diyalog Resmi")]
    public GameObject dialogueImage; // Canvas'taki DialoguePanel'i buraya sürükleyeceðiz

    // Nesnenin üzerine týklandýðýnda çalýþýr (Nesnede Collider olmak zorunda)
    void OnMouseDown()
    {
        if (dialogueImage != null)
        {
            dialogueImage.SetActive(true); // Resmi görünür yap
            Debug.Log("Gardiyana týklandý, resim açýldý.");
        }
    }
}