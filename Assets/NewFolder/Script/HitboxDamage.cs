using UnityEngine;
using System.Collections.Generic;

public class HitboxDamage : MonoBehaviour
{
    private BoxCollider2D hitboxCollider;
    private ContactFilter2D filter;

    [Header("몬스터 타격 설정")]
    public int physicalDamage = 10; 
    public int elementalDamage = 0; 

    private HashSet<Collider2D> alreadyHitColliders = new HashSet<Collider2D>();

    void Awake()
    {
        hitboxCollider = GetComponent<BoxCollider2D>();
        
        filter = new ContactFilter2D();
        filter.NoFilter();
        filter.useTriggers = true; 
    }

    void OnEnable()
    {
        ClearHitMemory();
    }

    void Update()
    {
        Collider2D[] results = new Collider2D[50]; 
        int hitCount = hitboxCollider.OverlapCollider(filter, results);

        for (int i = 0; i < hitCount; i++)
        {
            ProcessDamage(results[i]);
        }
    }

    // ⭐ [추가] 외부(스킬 컨트롤러 등)에서 계산된 데미지를 실시간으로 갱신하는 함수
    public void SetDamage(int newPhysicalDamage, int newElementalDamage = 0)
    {
        physicalDamage = newPhysicalDamage;
        elementalDamage = newElementalDamage;
    }

    // 다른 스크립트에서 강제로 명부를 지울 수 있게 Public으로 열어둡니다.
    public void ClearHitMemory()
    {
        alreadyHitColliders.Clear();
    }

    private void ProcessDamage(Collider2D other)
    {
        if (!other || !other.gameObject) return;

        // 자식 타격 판정이 같은 캐릭터의 몸 콜라이더를 때려 자기 피해를 주지 않도록 합니다.
        if (other.transform.root == transform.root) return;
        
        if (alreadyHitColliders.Contains(other)) return;

        try
        {
            IDamageable target = other.GetComponent<IDamageable>();
            if (target != null)
            {
                alreadyHitColliders.Add(other); 
                target.TakeDamage(physicalDamage, elementalDamage);
            }

            if (other.CompareTag("Breakable"))
            {
                BreakableBlock blockScript = other.GetComponentInParent<BreakableBlock>();
                if (blockScript == null) blockScript = other.GetComponent<BreakableBlock>();

                if (blockScript != null)
                {
                    alreadyHitColliders.Add(other); 
                    blockScript.BreakBlock();
                    return; 
                }
                // Tilemap tags apply to every cell in the map. Do not destroy cells
                // based on the map tag; only blocks with their own BreakableBlock
                // component should react to attacks.
            }
        }
        catch (System.Exception)
        {
        }
    }

}
