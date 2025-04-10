using System.Collections;
using StarterAssets;
using UnityEngine;


public class AttackController : MonoBehaviour
{
    Animator animator;
    ThirdPersonController thirdPersonController;
    PlayerStatus playerStatus;

    void Start()    
    {
        animator = GetComponent<Animator>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        playerStatus = GetComponent<PlayerStatus>();
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
