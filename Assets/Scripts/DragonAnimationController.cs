using UnityEngine;

public class DragonAnimationController : MonoBehaviour
{
    private Animator animator;
    
    // 애니메이션 파라미터 이름
    private const string PARAM_IS_WALKING = "IsWalking";
    private const string PARAM_IS_RUNNING = "IsRunning";
    private const string PARAM_IS_FLYING = "IsFlying";
    private const string PARAM_ATTACK = "Attack";
    private const string PARAM_FIRE_BREATH = "FireBreath";
    private const string PARAM_START_FLYING = "StartFlying";
    private const string PARAM_LAND = "Land";
    private const string PARAM_DIE = "Die";
    private const string PARAM_SPEED = "Speed";
    
    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    public void SetWalking(bool isWalking)
    {
        animator.SetBool(PARAM_IS_WALKING, isWalking);
    }
    
    public void SetRunning(bool isRunning)
    {
        animator.SetBool(PARAM_IS_RUNNING, isRunning);
    }
    
    public void SetFlying(bool isFlying)
    {
        animator.SetBool(PARAM_IS_FLYING, isFlying);
    }
    
    public void TriggerAttack()
    {
        animator.SetTrigger(PARAM_ATTACK);
    }
    
    public void TriggerFireBreath()
    {
        animator.SetTrigger(PARAM_FIRE_BREATH);
    }
    
    public void TriggerStartFlying()
    {
        animator.SetTrigger(PARAM_START_FLYING);
    }
    
    public void TriggerLand()
    {
        animator.SetTrigger(PARAM_LAND);
    }
    
    public void TriggerDie()
    {
        animator.SetTrigger(PARAM_DIE);
    }
    
    public void SetSpeed(float speed)
    {
        animator.SetFloat(PARAM_SPEED, speed);
    }
    
    // 애니메이션 이벤트에서 호출할 메서드들
    public void OnAttackAnimationHit()
    {
        // 공격 애니메이션의 특정 프레임에서 호출됨
        DragonCombat combat = GetComponent<DragonCombat>();
        if (combat != null)
        {
            combat.ApplyClawDamage();
        }
    }
    
    public void OnAnimationEnd()
    {
        // 애니메이션 종료 시 호출됨
        DragonAI ai = GetComponent<DragonAI>();
        if (ai != null)
        {
            ai.OnAnimationComplete();
        }
    }
}
