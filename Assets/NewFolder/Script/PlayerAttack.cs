using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator anim;
    
    [Header("공격 설정")]
    public float comboWindow = 0.5f; 
    
    // ⭐ 히트박스를 연결할 변수 추가
    [Tooltip("자식 오브젝트인 Hitbox를 여기에 끌어다 넣으세요.")]
    public GameObject hitbox; 
    
    private int comboStep = 0; 
    private float comboTimer = 0f; 

    void Start()
    {
        anim = GetComponent<Animator>();
        
        // 시작할 때 히트박스가 확실히 꺼져있도록 보장
        if (hitbox != null) 
        {
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
            anim.SetTrigger("Attack"); 
            comboTimer = comboWindow; 
        }
        else if (comboStep == 1 && comboTimer > 0)
        {
            comboStep = 2;
            anim.SetTrigger("Attack");
            comboTimer = comboWindow; 
        }
        else if (comboStep == 2 && comboTimer > 0)
        {
            comboStep = 3;
            anim.SetTrigger("Attack");
            comboTimer = comboWindow; 
        }
    }

    private void ResetCombo()
    {
        comboStep = 0;
        comboTimer = 0f;
    }

    public void EndAttackSequence()
    {
        ResetCombo();
    }

    // ⭐ 애니메이션 이벤트에서 호출할 함수 (히트박스 켜기)
    public void EnableHitbox()
    {
        if (hitbox != null) hitbox.SetActive(true);
    }

    // ⭐ 애니메이션 이벤트에서 호출할 함수 (히트박스 끄기)
    public void DisableHitbox()
    {
        if (hitbox != null) hitbox.SetActive(false);
    }
}