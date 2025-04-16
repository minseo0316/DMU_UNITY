using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DragonMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private DragonAnimationController animController;
    
    [Header("이동 설정")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float flySpeed = 8f;
    public float rotationSpeed = 3f;
    public float flyingHeight = 10f;
    
    private Transform player;
    private bool isGrounded = true;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animController = GetComponent<DragonAnimationController>();
        
        // 기본 설정
        agent.speed = walkSpeed;
        
        // 플레이어 찾기
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    public void Walk(Vector3 destination)
    {
        if (!isGrounded || agent == null) return;
        
        agent.isStopped = false;
        agent.speed = walkSpeed;
        agent.SetDestination(destination);
        animController.SetWalking(true);
        animController.SetRunning(false);
        animController.SetSpeed(agent.velocity.magnitude);
    }
    
    public void Run(Vector3 destination)
    {
        if (!isGrounded || agent == null) return;
        
        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.SetDestination(destination);
        animController.SetWalking(false);
        animController.SetRunning(true);
        animController.SetSpeed(agent.velocity.magnitude);
    }
    
    public void Stop()
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
        }
        
        animController.SetWalking(false);
        animController.SetRunning(false);
        animController.SetSpeed(0);
    }
    
    public void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0; // y축 회전만 고려
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    public IEnumerator TakeOff()
    {
        // 이륙 애니메이션 시작
        animController.TriggerStartFlying();
        
        // NavMeshAgent 비활성화
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        
        // 애니메이션 시작 대기
        yield return new WaitForSeconds(1.5f);
        
        // 공중으로 올라가는 로직
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(
            transform.position.x, 
            transform.position.y + flyingHeight, 
            transform.position.z
        );
        
        float flyUpTime = 0f;
        while (flyUpTime < 2f)
        {
            flyUpTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, targetPosition, flyUpTime / 2f);
            yield return null;
        }
        
        // 비행 상태로 설정
        isGrounded = false;
        animController.SetFlying(true);
    }
    
    public IEnumerator Land()
    {
        // 착륙 애니메이션 시작
        animController.TriggerLand();
        
        // 착륙 지점 찾기
        NavMeshHit hit;
        Vector3 landingPosition;
        if (NavMesh.SamplePosition(transform.position, out hit, 20f, NavMesh.AllAreas))
        {
            landingPosition = hit.position;
        }
        else
        {
            landingPosition = transform.position;
            landingPosition.y = 0; // 기본 지면 높이
        }
        
        // 착륙 동작
        float landingTime = 0f;
        Vector3 landingStart = transform.position;
        while (landingTime < 2f)
        {
            landingTime += Time.deltaTime;
            transform.position = Vector3.Lerp(landingStart, landingPosition, landingTime / 2f);
            yield return null;
        }
        
        // 착륙 완료
        isGrounded = true;
        animController.SetFlying(false);
        
        // NavMeshAgent 다시 활성화
        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }
    }
    
    public void FlyAroundTarget(Vector3 targetPosition, float circleDistance)
    {
        if (isGrounded) return;
        
        // 타겟 주변을 선회하는 움직임
        Vector3 directionToTarget = transform.position - targetPosition;
        directionToTarget.y = 0; // 고도 유지
        
        // 원형 경로를 따라 이동
        Vector3 perpendicular = new Vector3(-directionToTarget.z, 0, directionToTarget.x).normalized;
        Vector3 newPosition = targetPosition + directionToTarget.normalized * circleDistance + perpendicular * 2f;
        newPosition.y = transform.position.y; // 현재 고도 유지
        
        // 목표 지점으로 부드럽게 이동
        transform.position = Vector3.MoveTowards(transform.position, newPosition, flySpeed * Time.deltaTime);
        
        // 타겟을 향해 회전
        LookAtTarget(targetPosition);
    }
    
    public bool IsGrounded()
    {
        return isGrounded;
    }
}
