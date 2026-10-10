using UnityEngine;

public class EnemyAttackController : MonoBehaviour
{
    [Header("공격 설정")]
    [Min(0)] public int physicalDamage = 10;
    [Min(0)] public int elementalDamage = 0;
    [Min(0.05f)] public float attackInterval = 1f;

    [Header("플레이어 추적 설정")]
    [Tooltip("이 거리 안에 플레이어가 들어오면 추적을 시작합니다.")]
    [Min(0.1f)] public float detectionRange = 4.5f;
    [Tooltip("플레이어를 향해 이동하는 속도입니다.")]
    [Min(0f)] public float moveSpeed = 2f;
    [Tooltip("플레이어와 이 거리만큼 가까워지면 이동을 멈춥니다.")]
    [Min(0.1f)] public float stoppingDistance = 1.25f;
    [Tooltip("수직으로 이 거리보다 멀면 추적하지 않습니다.")]
    [Min(0.1f)] public float chaseVerticalRange = 2.5f;

    [Header("공격 범위 및 방향 전환")]
    [Tooltip("공격을 시작하고 플레이어를 바라볼 수 있는 수평 거리입니다.")]
    [Min(0.1f)] public float attackRange = 1.8f;
    [Tooltip("공격 가능 높이 차이입니다.")]
    [Min(0.1f)] public float maxVerticalDifference = 1f;
    [Tooltip("공격 범위 안에서 플레이어 방향을 다시 확인하는 간격(초)입니다.")]
    [Min(0.01f)] public float facePlayerInterval = 0.25f;

    [Header("실제 타격 판정")]
    [Tooltip("애니메이션 타격 프레임에 검사하는 공격 영역입니다.")]
    public Vector2 attackHitboxSize = new Vector2(1.2f, 1.5f);
    [Tooltip("더미 중심에서 공격 영역까지의 오프셋입니다.")]
    public Vector2 attackHitboxOffset = new Vector2(0.8f, 0f);

    [Header("플레이어 공격 애니메이션 재사용")]
    public string attackAnimationName = "Whiteray_Attack01";

    private Animator animator;
    private Rigidbody2D body;
    private DummyMonster dummyMonster;
    private PlayerStats playerStats;
    private float nextAttackTime;
    private float nextFaceTime;
    private bool isAttacking;
    private bool hitApplied;
    private bool hasWalkingParameter;
    private float facingDirection = 1f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        dummyMonster = GetComponent<DummyMonster>();

        if (animator != null)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == "isWalking" && parameter.type == AnimatorControllerParameterType.Bool)
                {
                    hasWalkingParameter = true;
                    break;
                }
            }
        }
    }

    private void Update()
    {
        if (dummyMonster != null && dummyMonster.IsDead)
        {
            isAttacking = false;
            return;
        }

        if (playerStats == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerStats = player.GetComponent<PlayerStats>();
        }

        if (playerStats == null || playerStats.currentHealth <= 0 || isAttacking)
            return;

        float horizontalDistance = Mathf.Abs(playerStats.transform.position.x - transform.position.x);
        float verticalDistance = Mathf.Abs(playerStats.transform.position.y - transform.position.y);

        if (horizontalDistance <= attackRange && verticalDistance <= maxVerticalDifference && Time.time >= nextFaceTime)
        {
            FacePlayer();
            nextFaceTime = Time.time + Mathf.Max(0.01f, facePlayerInterval);
        }

        if (horizontalDistance > attackRange || verticalDistance > maxVerticalDifference || Time.time < nextAttackTime)
            return;

        FacePlayer();
        isAttacking = true;
        hitApplied = false;
        SetWalkingAnimation(false);
        nextAttackTime = Time.time + Mathf.Max(0.05f, attackInterval);

        if (animator != null && !string.IsNullOrEmpty(attackAnimationName))
            animator.Play(attackAnimationName, 0, 0f);
        else
            isAttacking = false;
    }

    private void FixedUpdate()
    {
        if (body == null || !body.simulated) return;

        if ((dummyMonster != null && dummyMonster.IsDead) || playerStats == null ||
            playerStats.currentHealth <= 0 || isAttacking)
        {
            StopHorizontalMovement();
            return;
        }

        float horizontalDelta = playerStats.transform.position.x - transform.position.x;
        float verticalDistance = Mathf.Abs(playerStats.transform.position.y - transform.position.y);
        float horizontalDistance = Mathf.Abs(horizontalDelta);

        if (horizontalDistance > detectionRange || verticalDistance > chaseVerticalRange ||
            horizontalDistance <= stoppingDistance || moveSpeed <= 0f)
        {
            StopHorizontalMovement();
            return;
        }

        facingDirection = Mathf.Sign(horizontalDelta);
        SetFacingDirection(facingDirection);
        body.velocity = new Vector2(facingDirection * moveSpeed, body.velocity.y);
        SetWalkingAnimation(true);
    }

    // Whiteray_Attack01.anim의 EnableHitbox 이벤트에서 호출됩니다.
    public void EnableHitbox()
    {
        if (!isAttacking || hitApplied) return;
        hitApplied = true;

        if (playerStats == null || playerStats.currentHealth <= 0) return;

        Vector2 center = (Vector2)transform.position + new Vector2(attackHitboxOffset.x * facingDirection, attackHitboxOffset.y);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, attackHitboxSize, 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit.isTrigger) continue;

            PlayerStats hitPlayer = hit.GetComponentInParent<PlayerStats>();
            if (hitPlayer != playerStats) continue;

            hitPlayer.TakeDamage(physicalDamage, elementalDamage);
            return;
        }
    }

    // Whiteray_Attack01.anim의 DisableHitbox 이벤트에서 호출됩니다.
    public void DisableHitbox()
    {
        isAttacking = false;
    }

    private void FacePlayer()
    {
        facingDirection = playerStats.transform.position.x < transform.position.x ? -1f : 1f;
        SetFacingDirection(facingDirection);
    }

    private void SetFacingDirection(float direction)
    {
        Vector3 scale = transform.localScale;
        // Dummy_attack의 기본 SpriteRenderer는 FlipX가 켜져 있으므로
        // 루트 스케일 방향은 플레이어 방향과 반대로 적용해야 화면에서 맞게 보입니다.
        scale.x = Mathf.Abs(scale.x) * -direction;
        transform.localScale = scale;
    }

    private void StopHorizontalMovement()
    {
        if (body != null) body.velocity = new Vector2(0f, body.velocity.y);
        SetWalkingAnimation(false);
    }

    private void SetWalkingAnimation(bool isWalking)
    {
        if (hasWalkingParameter && animator != null)
            animator.SetBool("isWalking", isWalking);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position + new Vector3(attackHitboxOffset.x * facingDirection, attackHitboxOffset.y, 0f);
        Gizmos.DrawWireCube(center, attackHitboxSize);
    }
}
