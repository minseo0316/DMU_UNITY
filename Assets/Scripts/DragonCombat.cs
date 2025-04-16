using System.Collections;
using UnityEngine;

public class DragonCombat : MonoBehaviour
{
    public Transform mouthTransform; // 드래곤 입 위치
    public GameObject fireBreathPrefab; // 불 파티클 프리팹
    
    private DragonStats stats;
    private DragonAI ai;
    
    [Header("공격 설정")]
    public float clawAttackRange = 3f;
    public float fireBreathRange = 10f;
    public float fireBreathAngle = 30f;
    
    private Transform player;
    private float lastClawAttackTime = -3f;
    private float lastFireBreathTime = -8f;
    
    void Start()
    {
        stats = GetComponent<DragonStats>();
        ai = GetComponent<DragonAI>();
        
        // 플레이어 찾기
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        // 입 위치가 설정되지 않은 경우 찾기 시도
        if (mouthTransform == null)
        {
            // 드래곤 모델의 계층 구조에서 입 또는 머리 본 찾기
            Transform headBone = transform.Find("Root/Pelvis/Spine/Chest/Neck/Head");
            if (headBone != null)
            {
                mouthTransform = headBone;
            }
            else
            {
                GameObject mouthObj = new GameObject("MouthPosition");
                mouthObj.transform.parent = transform;
                mouthObj.transform.localPosition = new Vector3(0, 2.5f, 1.5f); // 대략적인 입 위치
                mouthTransform = mouthObj.transform;
            }
        }
    }
    
    public bool CanClawAttack()
    {
        return Time.time > lastClawAttackTime + stats.clawAttackCooldown;
    }
    
    public bool CanFireBreath()
    {
        return Time.time > lastFireBreathTime + stats.fireBreathCooldown;
    }
    
    public void PerformClawAttack()
    {
        if (!CanClawAttack()) return;
        
        lastClawAttackTime = Time.time;
        GetComponent<DragonAnimationController>().TriggerAttack();
    }
    
    public void ApplyClawDamage()
    {
        // 공격 범위 내 플레이어 확인
        if (player != null && Vector3.Distance(transform.position, player.position) <= clawAttackRange)
        {
            // 플레이어에게 데미지 적용
            PlayerStatus playerStatus = player.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                playerStatus.currentHp -= stats.clawAttackDamage;
                playerStatus.playerUI.UpdateUI();
            }
        }
    }
    
    public void PerformFireBreath(bool isAirborne)
    {
        if (!CanFireBreath()) return;
        
        lastFireBreathTime = Time.time;
        GetComponent<DragonAnimationController>().TriggerFireBreath();
        StartCoroutine(FireBreathEffect(isAirborne));
    }
    
    IEnumerator FireBreathEffect(bool isAirborne)
    {
        // 불뿜기 지속 시간
        float duration = isAirborne ? 3.0f : 2.5f;
        
        // 불 파티클 생성
        GameObject fireBreath = Instantiate(fireBreathPrefab, mouthTransform.position, mouthTransform.rotation);
        fireBreath.transform.parent = mouthTransform;
        
        // 불뿜기 데미지 영역 활성화
        StartCoroutine(DealFireBreathDamage(duration));
        
        // 불뿜기 지속
        yield return new WaitForSeconds(duration);
        
        // 파티클 서서히 종료
        ParticleSystem ps = fireBreath.GetComponent<ParticleSystem>();
        var emission = ps.emission;
        emission.enabled = false;
        
        yield return new WaitForSeconds(1.5f);
        Destroy(fireBreath);
        
        // AI에 애니메이션 완료 알림
        ai.OnAnimationComplete();
    }
    
    IEnumerator DealFireBreathDamage(float duration)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            
            // 불뿜기 범위 내 플레이어 확인
            if (player != null)
            {
                // 드래곤의 전방 방향 벡터
                Vector3 dragonForward = transform.forward;
                
                // 드래곤에서 플레이어로의 방향 벡터
                Vector3 directionToPlayer = player.position - mouthTransform.position;
                directionToPlayer.Normalize();
                
                // 드래곤의 전방 방향과 플레이어 방향 사이의 각도
                float angle = Vector3.Angle(dragonForward, directionToPlayer);
                
                // 불뿜기 범위 내에 있는지 확인 (각도 30도 이내, 거리 fireBreathRange 이내)
                if (angle < fireBreathAngle && Vector3.Distance(mouthTransform.position, player.position) < fireBreathRange)
                {
                    // 플레이어에게 데미지 적용 (초당 데미지)
                    PlayerStatus playerStatus = player.GetComponent<PlayerStatus>();
                    if (playerStatus != null)
                    {
                        playerStatus.currentHp -= stats.fireBreathDamage * Time.deltaTime;
                        playerStatus.playerUI.UpdateUI();
                    }
                }
            }
            
            yield return null;
        }
    }
}
