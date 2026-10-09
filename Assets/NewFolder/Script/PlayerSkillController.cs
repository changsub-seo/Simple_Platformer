using UnityEngine;
using System.Collections;

public class PlayerSkillController : MonoBehaviour
{
    [Header("장착된 스킬")]
    public SkillData skill_V; 
    public KeyCode skillKey = KeyCode.V; 

    [Header("스킬 쿨타임 및 사운드 설정")]
    public float skillCooldown = 0f; // 0으로 설정 시 쿨타임 없이 연속 사용 가능
    public AudioClip attackSound;    
    [Range(0f, 1f)]
    public float soundVolume = 0.5f; 

    [Header("순간이동 타격 설정")]
    public float teleportRange = 8f;        
    public float teleportOffset = 1.5f;     
    [Tooltip("0보다 크면 SkillData의 기본 딜레이 대신 이 수치를 타격 간격(초)으로 사용합니다.")]
    public float comboDelayOverride = 0f; 
    public LayerMask enemyLayer;            

    [Header("데미지 및 스탯 연동")]
    [Tooltip("비워두면 자식 오브젝트(Hitbox)에서 자동으로 찾아 연결합니다.")]
    public HitboxDamage playerHitbox; 
    [Tooltip("컨트롤러에서 추가로 부여하는 스킬 데미지 계수 (1.0 = 100%, 1.2 = 120%)")]
    public float skillDamageMultiplierOverride = 1.0f; 

    [Header("애니메이션 배속 설정")]
    public float baseAnimLength = 0.3f; 

    [Header("애니메이터 파라미터 이름 (공중 씹힘 방지용)")]
    public string groundedParamName = "isGrounded"; 
    public string jumpParamName = "isJumping";     

    private Animator anim;
    private Rigidbody2D rb;
    private PlayerMovement playerMovement; 
    private PlayerStats playerStats; 
    private AudioSource audioSource; 
    
    private float currentCooldown = 0f;
    private bool isTeleportAttacking = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>(); 
        playerStats = GetComponent<PlayerStats>(); 
        audioSource = GetComponent<AudioSource>(); 

        if (playerHitbox == null)
        {
            playerHitbox = GetComponentInChildren<HitboxDamage>();
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }

        if (Input.GetKeyDown(skillKey) && currentCooldown <= 0 && skill_V != null && !isTeleportAttacking)
        {
            StartCoroutine(TeleportAttackRoutine(skill_V));
        }
    }

    private IEnumerator TeleportAttackRoutine(SkillData skill)
    {
        Transform initialTarget = FindClosestEnemy();
        if (initialTarget == null) yield break; 

        isTeleportAttacking = true;
        currentCooldown = skillCooldown; 

        if (playerMovement != null) playerMovement.enabled = false;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;

        SetAnimatorGroundedState(true);

        if (playerHitbox == null)
        {
            playerHitbox = GetComponentInChildren<HitboxDamage>();
        }

        // 스탯 비례 최종 데미지 연산 및 히트박스 전달
        float selectedAttackStat = 10f;
        if (playerStats != null)
        {
            selectedAttackStat = skill.damageType == SkillDamageType.Magic
                ? playerStats.GetTotalAbilityPower()
                : playerStats.GetTotalAttackDamage();
        }
        int finalDamage = Mathf.RoundToInt(selectedAttackStat * skill.damageMultiplier * skillDamageMultiplierOverride);

        if (playerHitbox != null)
        {
            bool isMagicDamage = skill.damageType == SkillDamageType.Magic;
            playerHitbox.SetDamage(isMagicDamage ? 0 : finalDamage, isMagicDamage ? finalDamage : 0);
        }

        // 스탯 비례 연타 딜레이 및 애니메이션 배속 연산
        float totalAttackSpeed = (playerStats != null) ? playerStats.GetTotalAttackSpeed() : 1.0f;
        float effectiveSpeed = totalAttackSpeed * skill.speedMultiplier;
        if (effectiveSpeed <= 0.05f) effectiveSpeed = 0.05f;

        // comboDelayOverride 수치가 0보다 크면 컨트롤러 수치를, 0이면 SkillData의 기본 딜레이를 사용합니다.
        float targetBaseDelay = (comboDelayOverride > 0f) ? comboDelayOverride : skill.baseComboDelay;
        float calculatedComboDelay = targetBaseDelay / effectiveSpeed;
        
        float fastAnimSpeed = baseAnimLength / calculatedComboDelay;
        if (fastAnimSpeed < 1f) fastAnimSpeed = 1f;

        int sideMultiplier = (transform.position.x < initialTarget.position.x) ? -1 : 1;

        while (Input.GetKey(skillKey))
        {
            Transform target = FindClosestEnemy();
            if (target == null) break;

            Vector3 targetPos = target.position + new Vector3(sideMultiplier * teleportOffset, 0f, 0f);
            transform.position = targetPos;
            transform.localScale = new Vector3(-sideMultiplier, 1f, 1f);

            if (anim != null && !string.IsNullOrEmpty(skill.animTriggerName))
            {
                anim.speed = fastAnimSpeed; 
                anim.Play(skill.animTriggerName, 0, 0f); 
            }

            if (audioSource != null && attackSound != null)
            {
                audioSource.PlayOneShot(attackSound, soundVolume);
            }

            yield return new WaitForSeconds(calculatedComboDelay);

            sideMultiplier *= -1;
        }

        if (anim != null) anim.speed = 1f;

        rb.gravityScale = originalGravity;
        if (playerMovement != null) playerMovement.enabled = true;
        
        isTeleportAttacking = false;
    }

    private void SetAnimatorGroundedState(bool isGrounded)
    {
        if (anim == null) return;

        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == groundedParamName && param.type == AnimatorControllerParameterType.Bool)
            {
                anim.SetBool(groundedParamName, isGrounded);
            }
            if (param.name == jumpParamName && param.type == AnimatorControllerParameterType.Bool)
            {
                anim.SetBool(jumpParamName, !isGrounded);
            }
        }
    }

    private Transform FindClosestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, teleportRange, enemyLayer);
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            if (!hit.enabled) continue;

            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = hit.transform;
            }
        }
        return closest;
    }
}
