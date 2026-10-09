using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator anim;
    private PlayerStats playerStats; // ⭐ 플레이어 스탯 참조 추가
    
    [Header("공격 설정")]
    public float comboWindow = 0.5f; 
    [Tooltip("평타 데미지 계수 (1.0 = 캐릭터 공격력 100%)")]
    public float attackDamageMultiplier = 1.0f; // ⭐ 평타 데미지 배율 추가
    
    [Tooltip("자식 오브젝트인 Hitbox를 여기에 끌어다 넣으세요. (비워두면 자동 탐색)")]
    public GameObject hitbox; 
    
    private HitboxDamage hitboxDamageScript;
    
    private int comboStep = 0; 
    private float comboTimer = 0f; 

    void Start()
    {
        anim = GetComponent<Animator>();
        playerStats = GetComponent<PlayerStats>(); // ⭐ PlayerStats 컴포넌트 자동 가져오기
        
        // hitbox가 연결되어 있지 않다면 자식 오브젝트에서 자동으로 검색
        if (hitbox == null)
        {
            HitboxDamage childHitbox = GetComponentInChildren<HitboxDamage>();
            if (childHitbox != null)
            {
                hitbox = childHitbox.gameObject;
            }
        }

        if (hitbox != null) 
        {
            hitboxDamageScript = hitbox.GetComponent<HitboxDamage>(); 
            hitbox.SetActive(false); 
        }
    }

    void Update()
    {
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
        }
        else if (comboTimer <= 0 && comboStep != 0)
        {
            ResetCombo();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            AttemptAttack();
        }
    }

    private void AttemptAttack()
    {
        if (comboStep == 0)
        {
            comboStep = 1;
            TriggerAttackAnim();
        }
        else if (comboStep == 1 && comboTimer > 0)
        {
            comboStep = 2;
            TriggerAttackAnim();
        }
        else if (comboStep == 2 && comboTimer > 0)
        {
            comboStep = 3;
            TriggerAttackAnim();
        }
    }

    private void TriggerAttackAnim()
    {
        anim.SetTrigger("Attack"); 
        comboTimer = comboWindow; 
    }

    private void ResetCombo()
    {
        comboStep = 0;
        comboTimer = 0f;
        
        DisableHitbox(); 
    }

    public void EndAttackSequence()
    {
        ResetCombo();
    }

    // ⭐ 핵심 해결 구간: 애니메이션에서 히트박스가 켜지는 타이밍
    public void EnableHitbox()
    {
        if (hitbox != null) 
        {
            if (hitboxDamageScript == null)
            {
                hitboxDamageScript = hitbox.GetComponent<HitboxDamage>();
            }

            // ⭐ 평타를 내지를 때마다 PlayerStats의 공격력을 기반으로 데미지를 새로 설정합니다!
            if (hitboxDamageScript != null)
            {
                float baseAttackDamage = (playerStats != null) ? playerStats.GetTotalAttackDamage() : 10f;
                int finalNormalDamage = Mathf.RoundToInt(baseAttackDamage * attackDamageMultiplier);
                
                hitboxDamageScript.SetDamage(finalNormalDamage); // 데미지 갱신
                hitboxDamageScript.ClearHitMemory(); // 타격 명부 초기화
            }
            
            hitbox.SetActive(true);
        }
    }

    public void DisableHitbox()
    {
        if (hitbox != null) hitbox.SetActive(false);
    }
}
