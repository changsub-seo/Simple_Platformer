using UnityEngine;
using System.Collections;

public class DummyMonster : MonoBehaviour, IDamageable
{
    [Header("스탯")]
    public int maxHp = 50; 
    private int currentHp;
    public int defense = 5; 

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

    [Header("체력바 UI")]
    public HealthBar healthBar; 

    private Vector3 startPosition;

    void Start()
    {
        currentHp = maxHp;
        startPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;

        // ⭐ 변경점: 시작할 때는 Update가 아닌 Initialize를 불러서 조용히 숨겨둡니다.
        if (healthBar != null) healthBar.InitializeHealth(currentHp, maxHp);
    }

    public void TakeDamage(int physicalDamage, int elementalDamage)
    {
        int finalDamage = CalculateDamage(physicalDamage, elementalDamage);
        currentHp -= finalDamage;
        currentHp = Mathf.Max(0, currentHp);

        // ⭐ 맞았을 때는 짠! 하고 체력바가 나타나도록 Update를 부릅니다.
        if (healthBar != null) healthBar.UpdateHealth(currentHp, maxHp);

        if (gameObject.activeInHierarchy) StartCoroutine(HitFlashRoutine());
        ShowDamageText(finalDamage);
        ApplyKnockback();

        if (currentHp <= 0)
        {
            ResetDummy();
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

    public void ResetDummy()
    {
        Debug.Log("더미 초기화 완료!");
        currentHp = maxHp;
        transform.position = startPosition;
        if (rb != null) rb.velocity = Vector2.zero;
        if (sr != null) sr.color = originalColor;

        // ⭐ 변경점: 체력이 다 닳아서 리셋될 때도 다시 투명하게 숨어버립니다.
        if (healthBar != null) healthBar.InitializeHealth(currentHp, maxHp);
    }
}