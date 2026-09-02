using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class HitboxDamage : MonoBehaviour
{
    private BoxCollider2D hitboxCollider;
    private ContactFilter2D filter;

    [Header("타격 이펙트 & 사운드 (타일맵 전용)")]
    public GameObject breakEffectPrefab; 
    public AudioClip breakSound;         
    
    [Range(0f, 1f)]
    public float soundVolume = 1.0f;

    [Header("드랍 아이템")]
    public GameObject coinPrefab; 

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

    // ⭐ 다른 스크립트에서 강제로 명부를 지울 수 있게 Public으로 열어둡니다.
    public void ClearHitMemory()
    {
        alreadyHitColliders.Clear();
    }

    private void ProcessDamage(Collider2D other)
    {
        if (!other || !other.gameObject) return;
        
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

                Tilemap tilemap = other.GetComponent<Tilemap>();
                if (tilemap != null)
                {
                    alreadyHitColliders.Add(other); 
                    Bounds bounds = hitboxCollider.bounds;
                    Vector3Int minCell = tilemap.WorldToCell(bounds.min);
                    Vector3Int maxCell = tilemap.WorldToCell(bounds.max);

                    for (int x = minCell.x; x <= maxCell.x; x++)
                    {
                        for (int y = minCell.y; y <= maxCell.y; y++)
                        {
                            Vector3Int cellPos = new Vector3Int(x, y, 0);
                            
                            if (tilemap.HasTile(cellPos))
                            {
                                Vector3 effectPos = tilemap.GetCellCenterWorld(cellPos);
                                PlayEffects(effectPos);
                                
                                tilemap.SetTile(cellPos, null); 

                                if (coinPrefab != null)
                                {
                                    Instantiate(coinPrefab, effectPos, Quaternion.identity);
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception)
        {
        }
    }

    private void PlayEffects(Vector3 pos)
    {
        if (breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, pos, Quaternion.identity);
        }
        
        if (breakSound != null)
        {
            AudioSource.PlayClipAtPoint(breakSound, Camera.main.transform.position, soundVolume);
        }
    }
}