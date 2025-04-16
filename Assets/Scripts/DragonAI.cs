using System.Collections;
using UnityEngine;

public class DragonAI : MonoBehaviour
{
    private DragonStats stats;
    private DragonAnimationController animController;
    private DragonMovement movement;
    private DragonCombat combat;
    
    private Transform player;
    
    [Header("AI 설정")]
    public float detectionRange = 20f;
    public float flyingTriggerDistance = 12f;
    public float flyingDuration = 15f;
    
    private enum DragonState { Idle, Walking, Running, TakeOff, Flying, Landing, ClawAttack, GroundFireBreath, AirFireBreath }
    private DragonState currentState = DragonState.Idle;
    private float lastFlyingTime = -30f;
    private bool isAnimationPlaying = false;
    
    void Start()
    {
        stats = GetComponent<DragonStats>();
        animController = GetComponent<DragonAnimationController>();
        movement = GetComponent<DragonMovement>();
        combat = GetComponent<DragonCombat>();
        
        // 플레이어 찾기
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // 죽음 이벤트 구독
        stats.OnDeath += HandleDeath;
    }
    
    void Update()
    {
        if (stats.IsDead() || player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // 플레이어가 감지 범위를 벗어나면 대기 상태로
        if (distanceToPlayer > detectionRange)
        {
            SetState(DragonState.Idle);
            return;
        }
        
        // 애니메이션 재생 중이면 상태 변경 없음
        if (isAnimationPlaying) return;
        
        // 상태 결정 로직
        DecideNextState(distanceToPlayer);
        
        // 현재 상태에 따른 행동 수행
        PerformCurrentStateAction();
    }
    
    void DecideNextState(float distanceToPlayer)
    {
        // 지상 상태일 때만 새로운 상태 결정
        if (movement.IsGrounded())
        {
            // 비행 조건 확인
            bool shouldFly = ShouldStartFlying(distanceToPlayer);
            if (shouldFly)
            {
                SetState(DragonState.TakeOff);
                return;
            }

            // 지상 공격 조건 확인
            if (distanceToPlayer <= combat.clawAttackRange && combat.CanClawAttack())
            {
                SetState(DragonState.ClawAttack);
                return;
            }

            // 지상 불뿜기 조건 확인
            if (distanceToPlayer <= combat.fireBreathRange && distanceToPlayer > combat.clawAttackRange && 
                combat.CanFireBreath())
            {
                SetState(DragonState.GroundFireBreath);
                return;
            }

            // 이동 상태 결정
            if (distanceToPlayer > combat.clawAttackRange)
            {
                if (distanceToPlayer > combat.fireBreathRange)
                {
                    SetState(DragonState.Running);
                }
                else
                {
                    SetState(DragonState.Walking);
                }
            }
            else
            {
                SetState(DragonState.Idle);
            }
        }
        else
        {
            // 공중에서는 일정 확률로 불뿜기 공격
            if (Random.value < 0.005f && combat.CanFireBreath())
            {
                SetState(DragonState.AirFireBreath);
            }
        }
    }
    
    bool ShouldStartFlying(float distanceToPlayer)
    {
        // 비행 쿨다운 확인
        if (Time.time < lastFlyingTime + stats.flyingCooldown)
            return false;
            
        // 거리 기반 비행 결정
        bool distanceBased = distanceToPlayer <= flyingTriggerDistance && distanceToPlayer > combat.clawAttackRange;
        
        // 체력 기반 비행 결정
        bool healthBased = stats.currentHealth / stats.maxHealth < stats.healthThresholdForFlying;
        
        return distanceBased || healthBased;
    }
    
    void PerformCurrentStateAction()
    {
        if (player == null) return;
        
        switch (currentState)
        {
            case DragonState.Idle:
                movement.Stop();
                movement.LookAtTarget(player.position);
                break;
                
            case DragonState.Walking:
                movement.Walk(player.position);
                break;
                
            case DragonState.Running:
                movement.Run(player.position);
                break;
                
            case DragonState.TakeOff:
                isAnimationPlaying = true;
                StartCoroutine(TakeOffSequence());
                break;
                
            case DragonState.Flying:
                movement.FlyAroundTarget(player.position, flyingTriggerDistance);
                break;
                
            case DragonState.Landing:
                isAnimationPlaying = true;
                StartCoroutine(LandingSequence());
                break;
                
            case DragonState.ClawAttack:
                isAnimationPlaying = true;
                movement.Stop();
                movement.LookAtTarget(player.position);
                combat.PerformClawAttack();
                break;
                
            case DragonState.GroundFireBreath:
                isAnimationPlaying = true;
                movement.Stop();
                movement.LookAtTarget(player.position);
                combat.PerformFireBreath(false);
                break;
                
            case DragonState.AirFireBreath:
                isAnimationPlaying = true;
                StartCoroutine(AirFireBreathSequence());
                break;
        }
    }
    
    void SetState(DragonState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
        }
    }
    
    IEnumerator TakeOffSequence()
    {
        // 이륙 시작
        yield return StartCoroutine(movement.TakeOff());
        
        // 이륙 완료 후 비행 상태로 전환
        SetState(DragonState.Flying);
        lastFlyingTime = Time.time;
        isAnimationPlaying = false;
        
        // 일정 시간 후 착륙
        StartCoroutine(FlyingDurationTimer());
    }
    
    IEnumerator FlyingDurationTimer()
    {
        yield return new WaitForSeconds(flyingDuration);
        
        // 비행 중이고 애니메이션이 재생 중이 아니면 착륙 시작
        if (currentState == DragonState.Flying && !isAnimationPlaying)
        {
            SetState(DragonState.Landing);
            PerformCurrentStateAction();
        }
    }
    
    IEnumerator LandingSequence()
    {
        // 착륙 시작
        yield return StartCoroutine(movement.Land());
        
        // 착륙 완료 후 대기 상태로 전환
        SetState(DragonState.Idle);
        isAnimationPlaying = false;
    }
    
    IEnumerator AirFireBreathSequence()
    {
        // 공중에서 플레이어를 향해 회전
        float rotationTime = 0f;
        while (rotationTime < 1f)
        {
            rotationTime += Time.deltaTime;
            movement.LookAtTarget(player.position);
            yield return null;
        }
        
        // 불뿜기 공격 수행
        combat.PerformFireBreath(true);
        
        // 공격 애니메이션은 DragonCombat에서 처리하고, 
        // OnAnimationComplete에서 isAnimationPlaying을 false로 설정
    }
    
    // 애니메이션 완료 콜백
    public void OnAnimationComplete()
    {
        isAnimationPlaying = false;
        
        // 공격 후 대기 상태로 복귀
        if (currentState == DragonState.ClawAttack || 
            currentState == DragonState.GroundFireBreath || 
            currentState == DragonState.AirFireBreath)
        {
            if (movement.IsGrounded())
            {
                SetState(DragonState.Idle);
            }
            else
            {
                SetState(DragonState.Flying);
            }
        }
    }
    
    void HandleDeath()
    {
        // 모든 행동 중지
        StopAllCoroutines();
        
        // 죽음 애니메이션 재생
        animController.TriggerDie();
        
        // 움직임 중지
        movement.Stop();
        
        // 콜라이더 비활성화
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }
    }
}
