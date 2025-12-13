using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private int currentAttackIndex = 0;
    private bool isAttacking;

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
        // Defans kontrolü (Ctrl tuşu) - Saldırı yaparken defans yapamaz
        if (Input.GetKey(KeyCode.LeftControl) && !isAttacking)
        {
            animator.SetBool("IsDefending", true);
        }
        else
        {
            animator.SetBool("IsDefending", false);
        }

        // Sol fare tuşuna basıldığında - Defans yaparken saldırı yapamaz
        if (Input.GetMouseButtonDown(0) && !isAttacking && !Input.GetKey(KeyCode.LeftControl))
        {
            StartCoroutine(Attack());
        }
    }


    IEnumerator Attack()
    {
        isAttacking = true;
        
        // AttackIndex float değerini ayarla
        animator.SetFloat("AttackIndex", currentAttackIndex);
        
        // Attack trigger'ını tetikle
        animator.SetTrigger("Attack");
        
        // Sıradaki animasyon için index'i artır (0 -> 1 -> 2 -> 0)
        currentAttackIndex = (currentAttackIndex + 1) % 3;
        
        // Root Motion'ı aktif et (eğer saldırı animasyonunda hareket varsa)
        animator.applyRootMotion = true;
        
        // Animasyon state'inin değişmesini bekle
        yield return null;
        
        // Şimdi doğru animasyon süresini al
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;
        
        // Animasyon bitene kadar bekle
        yield return new WaitForSeconds(animationLength);
        
        // Saldırı bitti
        isAttacking = false;
        animator.applyRootMotion = false;
    }
}
