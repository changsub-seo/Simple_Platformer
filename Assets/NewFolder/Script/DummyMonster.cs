using UnityEngine;
using System.Collections;

public class DummyMonster : MonoBehaviour, IDamageable
{
    [Header("스탯")]
    public int maxHp = 50; 
    private int currentHp;
    public int defense = 5; 

    [Header("부활 설정")]
    [Tooltip("체력이 0이 된 적을 자동으로 다시 생성합니다.")]
    public bool autoRespawn = true;
    [Min(0f)]
    [Tooltip("자동 재생성까지 기다리는 시간(초)입니다.")]
    public float respawnTime = 3f;
    private bool isDead = false;   // 죽어있는 동안 중복 타격을 막기 위한 변수
    public bool IsDead => isDead;

    [Header("시각 효과 (1번 기능)")]
    public Color hitColor = Color.red;
    public float flashDuration = 0.1f;
    private SpriteRenderer sr;
    private Color originalColor;

    [Header("데미지 텍스트 (2번 기능)")]
    public GameObject damageTextPrefab; 
    public Transform textSpawnPoint;    

    [Header("넉백 설정 (3번 기능)")]
    public float knockbackForceX = 5f;
    public float knockbackForceY = 3f;
    private Rigidbody2D rb;
    private Collider2D col; // 충돌체(히트박스)를 끄기 위해 추가

    [Header("체력바 UI")]
    public HealthBar healthBar; 

    private Vector3 startPosition;
    private MonsterPatrol patrolScript; // 순찰 스크립트를 잠시 끄기 위해 추가

    void Start()
    {
        currentHp = maxHp;
        startPosition = transform.position;
        
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        patrolScript = GetComponent<MonsterPatrol>(); 

        if (sr != null) originalColor = sr.color;

        if (healthBar != null) healthBar.InitializeHealth(currentHp, maxHp);
    }

    public void TakeDamage(int physicalDamage, int elementalDamage)
    {
        // ⭐ 죽어있는 상태라면 데미지를 무시합니다.
        if (isDead) return;

        int finalDamage = CalculateDamage(physicalDamage, elementalDamage);
        currentHp -= finalDamage;
        currentHp = Mathf.Max(0, currentHp);

        if (healthBar != null) healthBar.UpdateHealth(currentHp, maxHp);

        if (gameObject.activeInHierarchy) StartCoroutine(HitFlashRoutine());
        ShowDamageText(finalDamage);
        ApplyKnockback();

        // 체력이 0이 되면 사망 처리 후 설정에 따라 재생성합니다.
        if (currentHp <= 0)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    private int CalculateDamage(int phys, int elem)
    {
        int finalPhys = Mathf.Max(1, phys - defense); 
        int totalDamage = finalPhys + elem;
        return totalDamage;
    }

    private IEnumerator HitFlashRoutine()
    {
        if (sr != null)
        {
            sr.color = hitColor;
            yield return new WaitForSeconds(flashDuration);
            sr.color = originalColor;
        }
    }

    private void ShowDamageText(int damage)
    {
        if (damageTextPrefab != null && textSpawnPoint != null)
        {
            GameObject textObj = Instantiate(damageTextPrefab, textSpawnPoint.position, Quaternion.identity);
            DamagePopup popup = textObj.GetComponent<DamagePopup>();
            if (popup != null) popup.Setup(damage);
        }
    }

    private void ApplyKnockback()
    {
        if (rb != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float direction = transform.position.x > player.transform.position.x ? 1f : -1f;
                rb.velocity = Vector2.zero; 
                rb.AddForce(new Vector2(direction * knockbackForceX, knockbackForceY), ForceMode2D.Impulse);
            }
        }
    }

    // 자동 재생성이 꺼져 있으면 사망 상태와 비활성 상태를 유지합니다.
    private IEnumerator RespawnRoutine()
    {
        isDead = true; // 사망 상태 진입

        // 1. 더미를 화면에서 지우고 충돌과 물리를 끕니다.
        if (sr != null) sr.enabled = false;           // 투명하게 만들기
        if (col != null) col.enabled = false;         // 타격 판정 없애기 (V키 스킬이 더 이상 얘를 찾지 않음)
        if (rb != null) rb.simulated = false;         // 바닥으로 떨어지지 않게 물리 정지
        if (patrolScript != null) patrolScript.enabled = false; // 순찰 정지
        
        if (healthBar != null) healthBar.InitializeHealth(0, maxHp); // 체력바 숨기기

        if (!autoRespawn) yield break;

        // 인스펙터에 설정된 시간만큼 기다립니다.
        yield return new WaitForSeconds(respawnTime);

        // 3. 체력과 위치를 초기화합니다.
        ResetDummy();

        // 4. 다시 화면에 보이고 만질 수 있게 복구합니다.
        if (sr != null) sr.enabled = true;
        if (col != null) col.enabled = true;
        if (rb != null) rb.simulated = true;
        if (patrolScript != null) patrolScript.enabled = true;

        isDead = false; // 부활 완료!
    }

    public void ResetDummy()
    {
        Debug.Log("더미 초기화 완료!");
        currentHp = maxHp;
        transform.position = startPosition;
        if (rb != null) rb.velocity = Vector2.zero;
        if (sr != null) sr.color = originalColor;

        if (healthBar != null) healthBar.InitializeHealth(currentHp, maxHp);
    }
}
