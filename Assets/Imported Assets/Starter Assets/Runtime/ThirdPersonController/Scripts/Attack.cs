using System.Collections;
using StarterAssets;
using UnityEngine;


public class AttackController : MonoBehaviour
{
    Animator animator;
    ThirdPersonController thirdPersonController;

    void Start()    
    {
        animator = GetComponent<Animator>();
        thirdPersonController = GetComponent<ThirdPersonController>();
    }

    public void attackProcess()
    {
        thirdPersonController.canMove = false;
        animator.SetTrigger("Attack");
    }

    public void attackEnd()
    {
        thirdPersonController.canMove = true;
    }
}
