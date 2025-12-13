using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private int currentAttackIndex = 0;

    void Start()
    {
        // Eğer Inspector'dan atanmamışsa, otomatik olarak bul
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        // Sol fare tuşuna basıldığında
        if (Input.GetMouseButtonDown(0))
        {
            // Attack trigger'ını tetikle
            animator.SetTrigger("Attack");
            
            // AttackIndex float değerini ayarla
            animator.SetFloat("AttackIndex", currentAttackIndex);
            
            // Sıradaki animasyon için index'i artır (0 -> 1 -> 2 -> 0)
            currentAttackIndex = (currentAttackIndex + 1) % 3;
        }
    }
}
