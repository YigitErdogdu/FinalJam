using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast Ayarlarý")]
    // Iþýnýn ne kadar uzaða gideceði. Gardiyana yaklaþma mesafesi.
    public float interactionDistance = 3f;
    // Gardiyaný tanýmak için kullanacaðýmýz Tag
    public string targetTag = "Guard";

    [Header("UI ve Baðlantýlar")]
    // Diyalog Paneli (Inspector'dan sürükle)
    public GameObject dialoguePanel;

    // Kameranýn bileþeni (Start'ta otomatik atanacak)
    private Camera cam;

    void Start()
    {
        // Kamerayý bul ve kaydet
        cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("RaycastInteractor kameraya baðlý deðil!");
        }

        // Diyalog paneli baþlangýçta kapalý olmalý
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    void Update()
    {
        // Iþýný fýrlatmak için bir ýþýn (Ray) oluþtur
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        // Iþýn bir þeye çarptý mý?
        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            
            // Iþýn gardiyana çarptýysa ve 'E' tuþuna basýldýysa
            if (hit.collider.CompareTag(targetTag) && Input.GetKeyDown(KeyCode.E))
            {
                print("asdsd");
                // Paneli aç/kapat
                bool newState = !dialoguePanel.activeInHierarchy;
                dialoguePanel.SetActive(newState);

                // Diyalog açýldýðýnda oyunu durdurma seçeneði:
                // Time.timeScale = newState ? 0f : 1f;
            }
        }
        else
        {
            // Iþýn hiçbir þeye çarpmýyorsa, menzilde deðilsen diyalog açýksa kapat
            if (dialoguePanel.activeInHierarchy && Input.GetKeyDown(KeyCode.E))
            {
                dialoguePanel.SetActive(false);
            }
        }
    }
}
