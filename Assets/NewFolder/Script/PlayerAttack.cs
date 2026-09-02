using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator anim;
    
    [Header("공격 설정")]
    public float comboWindow = 0.5f; 
    
    [Tooltip("자식 오브젝트인 Hitbox를 여기에 끌어다 넣으세요.")]
    public GameObject hitbox; 
    
    private HitboxDamage hitboxDamageScript;
    
    private int comboStep = 0; 
    private float comboTimer = 0f; 

    void Start()
    {
        anim = GetComponent<Animator>();
        
        if (hitbox != null) 
        {
            hitbox.SetActive(false); 
            hitboxDamageScript = hitbox.GetComponent<HitboxDamage>(); 
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
        
        // ❌ 여기서 명부를 지우면 안 됩니다! 버튼 연타 시 데미지가 중복해서 들어갑니다. (삭제 완료)
    }

    private void ResetCombo()
    {
        comboStep = 0;
        comboTimer = 0f;
        
        // 혹시 공격 도중에 맞거나 끊겨서 히트박스가 켜진 채 굳어버리는 것을 방지
        DisableHitbox(); 
    }

    public void EndAttackSequence()
    {
        ResetCombo();
    }

    // ⭐ 핵심 해결 구간: "애니메이션에서 칼을 내지르는 정확한 타이밍"
    public void EnableHitbox()
    {
        if (hitbox != null) 
        {
            // 공격 애니메이션이 모션 캔슬되어 히트박스가 켜진 채로 넘어왔을 수도 있으니,
            // '실제로 칼이 나가는 이 순간'에만 타격 명부를 강제로 한 번 지워줍니다.
            if (hitboxDamageScript != null)
            {
                hitboxDamageScript.ClearHitMemory();
            }
            
            hitbox.SetActive(true);
        }
    }

    public void DisableHitbox()
    {
        if (hitbox != null) hitbox.SetActive(false);
    }
}