using UnityEngine;

// MonoBehaviour를 상속받지 않고 interface로 선언합니다.
public interface IDamageable
{
    // 데미지를 받을 수 있는 녀석(몬스터, 플레이어, 부서지는 상자 등)은 
    // 반드시 이 함수를 가지고 있어야 한다고 강제하는 약속입니다.
    void TakeDamage(int physicalDamage, int elementalDamage);
}