using UnityEngine;

public class DragonStats : MonoBehaviour
{
    [Header("체력 설정")]
    public float maxHealth = 1000f;
    public float currentHealth;
    
    [Header("공격력 설정")]
    public float clawAttackDamage = 20f;
    public float fireBreathDamage = 15f; // 초당 데미지
    
    [Header("쿨다운 설정")]
    public float clawAttackCooldown = 3f;
    public float fireBreathCooldown = 8f;
    public float flyingCooldown = 30f;
    
    [Header("체력 단계")]
    public float healthThresholdForFlying = 0.7f; // 70% 체력에서 비행 패턴 시작
    
    private bool isDead = false;
    
    // 이벤트 시스템
    public delegate void OnHealthChangeDelegate();
    public event OnHealthChangeDelegate OnHealthChange;
    
    public delegate void OnDeathDelegate();
    public event OnDeathDelegate OnDeath;
    
    void Start()
    {
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        // 이벤트 발생
        if (OnHealthChange != null)
            OnHealthChange();
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        isDead = true;
        
        // 이벤트 발생
        if (OnDeath != null)
            OnDeath();
    }
    
    public bool IsDead()
    {
        return isDead;
    }
}
