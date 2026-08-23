using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("기초 스탯 (Primary Stats)")]
    public int strength = 10;     // 힘: 1당 최대 체력 증가
    public int dexterity = 10;    // 민첩: 1당 명중률 증가
    public int intelligence = 10; // 지능: 1당 최대 마나 증가

    [Header("파생 스탯 (자동 계산)")]
    public int maxHealth;         // 최대 생명력
    public int maxMana;           // 최대 마나
    public float accuracy;        // 명중률

    [Header("현재 상태 (Resources)")]
    public int currentHealth;     // 현재 생명력 (0이 되면 사망)
    public float currentMana;     // 현재 마나 (시간에 따라 자동 회복)
    public float currentSpirit = 100f; // 정신력 (지속형 버프 유지 자원)

    [Header("전투 스탯 (Combat Stats)")]
    public int defense = 5;       // 방어: 물리 피해 감소
    public int resistance = 5;    // 저항: 속성 피해 감소
    public string elementType = "None"; // 원소: 피해 유형 (추후 enum으로 변경 추천)

    [Header("스탯 변환 비율 설정")]
    public int hpPerStrength = 2; // 힘 1당 오르는 체력
    public float accuracyPerDex = 1.5f; // 민첩 1당 오르는 명중률
    public int manaPerInt = 5; // 지능 1당 오르는 마나

    public float manaRegenRate = 2f; // 초당 마나 재생량

    void Start()
    {
        // 게임 시작 시 스탯을 계산하고 체력/마나를 꽉 채워줍니다.
        UpdateDerivedStats();
        currentHealth = maxHealth;
        currentMana = maxMana;
    }

    void Update()
    {
        // 마나 자동 재생 로직
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
            if (currentMana > maxMana) currentMana = maxMana;
        }
    }

    // ⭐ 장비를 끼거나 벗을 때마다 이 함수를 불러주면 스탯이 재계산됩니다!
    public void UpdateDerivedStats()
    {
        // 기본치(Base)에 스탯 비례 보너스를 더해줍니다.
        maxHealth = 100 + (strength * hpPerStrength);
        maxMana = 50 + (intelligence * manaPerInt);
        accuracy = 80f + (dexterity * accuracyPerDex);

        // 최대 체력이 깎였을 경우 현재 체력도 맞춰서 조절
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    // 데미지를 받을 때 호출하는 함수 예시
    public void TakeDamage(int physicalDamage, int elementalDamage)
    {
        // 물리 피해는 방어력으로, 속성 피해는 저항력으로 감소시킵니다.
        int finalPhysical = Mathf.Max(0, physicalDamage - defense);
        int finalElemental = Mathf.Max(0, elementalDamage - resistance);
        
        int totalDamage = finalPhysical + finalElemental;
        currentHealth -= totalDamage;

        Debug.Log($"피해를 입었습니다! 받은 데미지: {totalDamage}, 남은 체력: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("플레이어 사망!");
        // 추후 여기에 사망 애니메이션 트리거 등 추가
    }
}