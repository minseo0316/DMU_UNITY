using System.Collections;
using StarterAssets;
using UnityEngine;


public class AttackController : MonoBehaviour
{
    public int attackCount = 0;
    public int maxAttackCount = 4;
    public float attackCooldown = 0.5f;
    public float exitTime = 1f;
    public bool max_combo = false;

    Animator animator;
    StarterAssetsInputs input;
    ThirdPersonController thirdPersonController;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        input = GetComponent<StarterAssetsInputs>();
        
        if (input == null)
        {
            input = GetComponentInParent<StarterAssetsInputs>();
            
            if (input == null)
            {
                input = GetComponentInChildren<StarterAssetsInputs>();
                
                if (input == null)
                {
                    Debug.LogError("StarterAssetsInputs를 찾을 수 없습니다!");
                }
            }
        }
    }

    
    void Update()
    {
        attackProcess();
    }

    public void attackProcess()
    {
        if (input != null && input.attack)
        {
            // 입력을 초기화하여 연속 입력 방지
            input.attack = false;
            
            if (attackCount < maxAttackCount && !max_combo)
            {
                attackCount++;
                
                if (attackCount == maxAttackCount)
                {
                    max_combo = true;
                }

                if (attackCount == 1)
                {
                    StartCoroutine(attackTimer());
                }
            }
        }
    }

    private IEnumerator attackTimer()
    {
        int currentAttack = 0;

        while(attackCount > 0)
        {
            if(currentAttack >= maxAttackCount)
            {
                Debug.Log("최대 콤보 도달");
                ResetAttack();
                break;
            }

            animator.SetTrigger("Attack");
            animator.SetInteger("attackCount", currentAttack);
            Debug.Log("Attack Count: " + currentAttack);
            currentAttack++;

            // 공격 애니메이션 실행 시간 대기
            yield return new WaitForSeconds(attackCooldown);
            attackCount--;
        }
        
        // 콤보 종료 후 초기화
        animator.SetTrigger("attackEnd");
        yield return new WaitForSeconds(exitTime);
        max_combo = false;
    }
    
    private void ResetAttack()
    {
        attackCount = 0;
        max_combo = false;
        animator.SetInteger("attackCount", 0);
        animator.SetTrigger("attackEnd");
    }
}
