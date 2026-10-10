using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("기초 스탯 (Primary Stats)")]
    public int strength = 10;     
    public int dexterity = 10;    
    public int intelligence = 10; 

    [Header("파생 스탯 (자동 계산)")]
    public int maxHealth;         
    public int maxMana;           
    public float currentTotalHitRate;

    [Header("현재 상태 (Resources)")]
    public int currentHealth;     
    public float currentMana;     
    public float baseSpirit = 100f;       // ⭐ 기본 정신력
    public float currentTotalSpirit;      // ⭐ 장비 보너스가 합산된 최종 정신력

    [Header("전투 스탯 (Combat Stats)")]
    public float baseAttackDamage = 10f;  
    public float baseAbilityPower = 0f;
    public float baseAttackSpeed = 1.0f; 
    public float baseHitRate = 100f;
    public int baseDefense = 5;
    public int baseResistance = 5;
    public string elementType = "None"; 

    [Header("이동 스탯 (Movement Stats)")]
    public float baseMoveSpeed = 5.0f;  
    public float baseJumpForce = 10.0f;  

    [Header("🔍 실시간 스탯 확인용 (인스펙터 모니터링)")]
    public int currentTotalStrength;
    public int currentTotalDexterity;
    public int currentTotalIntelligence;
    public float currentTotalAttackDamage; 
    public float currentTotalAbilityPower;
    public float currentTotalAttackSpeed; 
    public int currentTotalDefense;
    public int currentTotalResistance;
    public float currentTotalMoveSpeed;
    public float currentTotalJumpForce;

    [Header("🖥️ UI 연동")]
    public TextMeshProUGUI statTextUI; 

    [Header("스탯 변환 비율 설정")]
    public int hpPerStrength = 2; 
    public float hitRatePerDex = 1.5f;
    public int manaPerInt = 5; 
    public float manaRegenRate = 2f; 

    private EquipmentManager equipmentManager;
    private bool isDead;

    void Start()
    {
        equipmentManager = GetComponent<EquipmentManager>();
        if (equipmentManager == null) equipmentManager = EquipmentManager.instance;

        UpdateDerivedStats();
        currentHealth = maxHealth;
        currentMana = maxMana;
    }

    void Update()
    {
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
            if (currentMana > maxMana) currentMana = maxMana;
        }

        UpdateDerivedStats();
        UpdateStatUI(); 
    }

    public void UpdateDerivedStats()
    {
        int equipStr = (equipmentManager != null) ? equipmentManager.GetTotalBonusStrength() : 0;
        int equipDex = (equipmentManager != null) ? equipmentManager.GetTotalBonusDexterity() : 0;
        int equipInt = (equipmentManager != null) ? equipmentManager.GetTotalBonusIntelligence() : 0;
        int equipHP = (equipmentManager != null) ? equipmentManager.GetTotalBonusHP() : 0;
        int equipMP = (equipmentManager != null) ? equipmentManager.GetTotalBonusMP() : 0;
        int equipSpirit = (equipmentManager != null) ? equipmentManager.GetTotalBonusSpirit() : 0; // ⭐ 정신력 장비 보너스 연동

        currentTotalStrength = strength + equipStr;
        currentTotalDexterity = dexterity + equipDex;
        currentTotalIntelligence = intelligence + equipInt;

        maxHealth = 100 + (currentTotalStrength * hpPerStrength) + equipHP;
        maxMana = 50 + (currentTotalIntelligence * manaPerInt) + equipMP;
        currentTotalHitRate = GetTotalHitRate();
        currentTotalSpirit = baseSpirit + equipSpirit; // ⭐ 최종 정신력 계산

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        currentTotalAttackDamage = GetTotalAttackDamage();
        currentTotalAbilityPower = GetTotalAbilityPower();
        currentTotalAttackSpeed = GetTotalAttackSpeed();
        currentTotalDefense = GetTotalDefense();
        currentTotalResistance = GetTotalResistance();
        currentTotalMoveSpeed = GetTotalMoveSpeed();
        currentTotalJumpForce = GetTotalJumpForce();
    }

    public float GetTotalAttackDamage()
    {
        float equipAttackDamage = (equipmentManager != null) ? equipmentManager.GetTotalBonusAttackDamage() : 0f;
        return baseAttackDamage + equipAttackDamage;
    }

    public float GetTotalAbilityPower()
    {
        float equipmentAbilityPower = (equipmentManager != null) ? equipmentManager.GetTotalBonusAbilityPower() : 0f;
        return baseAbilityPower + equipmentAbilityPower;
    }

    public float GetTotalAttackSpeed()
    {
        float equipmentAttackSpeed = (equipmentManager != null) ? equipmentManager.GetTotalBonusAttackSpeed() : 0f;
        return baseAttackSpeed + equipmentAttackSpeed;
    }

    public int GetTotalDefense()
    {
        int equipDef = (equipmentManager != null) ? equipmentManager.GetTotalBonusDefense() : 0;
        return baseDefense + equipDef;
    }

    public int GetTotalResistance()
    {
        int equipmentResistance = (equipmentManager != null) ? equipmentManager.GetTotalBonusResistance() : 0;
        return baseResistance + equipmentResistance;
    }

    public float GetTotalHitRate()
    {
        float equipmentHitRate = (equipmentManager != null) ? equipmentManager.GetTotalBonusHitRate() : 0f;
        return baseHitRate + (currentTotalDexterity * hitRatePerDex) + equipmentHitRate;
    }

    public float GetTotalMoveSpeed()
    {
        float equipSpeed = (equipmentManager != null) ? equipmentManager.GetTotalBonusMoveSpeed() : 0f;
        return baseMoveSpeed + equipSpeed;
    }

    public float GetTotalJumpForce()
    {
        float equipJump = (equipmentManager != null) ? equipmentManager.GetTotalBonusJumpForce() : 0f;
        return baseJumpForce + equipJump;
    }

    public void UpdateStatUI()
    {
        if (statTextUI == null) return;

        string newText = $"[ 캐릭터 스탯 ]\n" +
                          $"힘 (STR) : {currentTotalStrength}\n" +
                          $"민첩 (DEX) : {currentTotalDexterity}\n" +
                          $"지능 (INT) : {currentTotalIntelligence}\n" +
                          $"-------------------------\n" +
                          $"물리 공격력 : {currentTotalAttackDamage}\n" +
                          $"마법 공격력 : {currentTotalAbilityPower}\n" +
                          $"공격속도 : {currentTotalAttackSpeed * 100f:F0}%\n" +
                          $"방어력 : {currentTotalDefense}\n" +
                          $"저항력 : {currentTotalResistance}\n" +
                          $"명중률 : {currentTotalHitRate:F1}%\n" +
                          $"-------------------------\n" +
                          $"이동속도 : {currentTotalMoveSpeed:F1}\n" +
                          $"점프력 : {currentTotalJumpForce:F1}\n" +
                          $"-------------------------\n" +
                          $"체 력 : {currentHealth} / {maxHealth}\n" +
                          $"마 나 : {(int)currentMana} / {maxMana}\n" +
                          $"정신력 : {(int)currentTotalSpirit}"; // ⭐ 최종 정신력 출력

        if (statTextUI.text != newText)
        {
            statTextUI.text = newText;
        }
    }

    public void TakeDamage(int physicalDamage, int elementalDamage)
    {
        if (currentHealth <= 0) return;

        int finalPhysical = Mathf.Max(0, physicalDamage - GetTotalDefense());
        int finalElemental = Mathf.Max(0, elementalDamage - currentTotalResistance);
        
        int totalDamage = finalPhysical + finalElemental;
        currentHealth = Mathf.Max(0, currentHealth - totalDamage);

        if (currentHealth <= 0) Die();
    }

    public bool TrySpendMana(float amount)
    {
        if (amount <= 0f) return true;
        if (currentMana < amount) return false;

        currentMana -= amount;
        return true;
    }

    public void DieFromFall()
    {
        if (isDead) return;

        currentHealth = 0;
        Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.DisableHitbox();
            attack.enabled = false;
        }

        PlayerSkillController skillController = GetComponent<PlayerSkillController>();
        if (skillController != null) skillController.enabled = false;

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.velocity = Vector2.zero;
            body.simulated = false;
        }

        Debug.Log("플레이어 사망!");
        int remainingLives = PlayerLifeSystem.LoseLife();
        GameOverUI.Show(remainingLives);
    }
}
