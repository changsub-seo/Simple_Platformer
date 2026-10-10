using UnityEngine;

public enum SkillDamageType
{
    Physical,
    Magic
}

[CreateAssetMenu(fileName = "New Skill", menuName = "Skill System/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("기본 정보")]
    public string skillName = "새로운 스킬";    // 스킬 이름
    [TextArea]
    public string description = "스킬 설명입니다."; // UI에 띄울 설명
    public Sprite skillIcon;                    // UI에 띄울 아이콘 이미지

    [Header("스킬 계수 설정")]
    [Tooltip("스킬이 참조할 공격 스탯 종류입니다.")]
    public SkillDamageType damageType = SkillDamageType.Physical;

    [Tooltip("선택한 공격 스탯의 피해 계수 (예: 1.5 = 해당 스탯의 150% 데미지)")]
    public float damageMultiplier = 1.5f;

    [Tooltip("기본 연타 타격 간격 (초 단위)")]
    public float baseComboDelay = 0.2f;         // ⭐ [추가] 기본 연타 간격 (공속 1.0 기준)

    [Tooltip("스킬 고유 공격속도 영향 비율 (1.0 = 캐릭 공속 100% 반영)")]
    public float speedMultiplier = 1.0f;        // ⭐ [추가] 캐릭터 공속 반영 비율

    [Header("전투 수치 및 쿨타임")]
    [Min(0)]
    [Tooltip("스킬을 시작할 때 한 번 소모할 마나량입니다.")]
    public float manaCost = 10f;

    public float cooldown = 3f;                 // 재사용 대기시간(초)

    [Header("애니메이션 & 시각 효과")]
    public string animTriggerName;              // 실행할 애니메이션 트리거 이름 (예: "HighKick")
    public GameObject effectPrefab;             // 생성할 이펙트 프리팹 (선택)
}
