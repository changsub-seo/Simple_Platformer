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
    public GameObject coinPrefab; // ⭐ 뱉어낼 코인 프리팹

    private HashSet<BreakableBlock> hitBlocksThisFrame = new HashSet<BreakableBlock>();

    void Start()
    {
        hitboxCollider = GetComponent<BoxCollider2D>();
        
        filter = new ContactFilter2D();
        filter.NoFilter();
        filter.useTriggers = true; 
    }

    void Update()
    {
        Collider2D[] results = new Collider2D[50]; 
        int hitCount = hitboxCollider.OverlapCollider(filter, results);

        hitBlocksThisFrame.Clear(); 

        for (int i = 0; i < hitCount; i++)
        {
            ProcessDamage(results[i]);
        }
    }

    private void ProcessDamage(Collider2D other)
    {
        if (!other || !other.gameObject) return;

        try
        {
            if (other.CompareTag("Breakable"))
            {
                // 1. 검은색 블록 (BreakableBlock 스크립트가 있는 객체)
                BreakableBlock blockScript = other.GetComponentInParent<BreakableBlock>();
                if (blockScript == null) blockScript = other.GetComponent<BreakableBlock>();

                if (blockScript != null)
                {
                    if (!blockScript || !blockScript.gameObject) return;
                    if (hitBlocksThisFrame.Contains(blockScript)) return;
                    hitBlocksThisFrame.Add(blockScript); 

                    blockScript.BreakBlock(); // 블록 파괴 (이 안에서 코인 소환)
                    return; 
                }

                // 2. 노란색 블록 (순수 타일맵)
                Tilemap tilemap = other.GetComponent<Tilemap>();
                if (tilemap != null)
                {
                    Bounds bounds = hitboxCollider.bounds;
                    Vector3Int minCell = tilemap.WorldToCell(bounds.min);
                    Vector3Int maxCell = tilemap.WorldToCell(bounds.max);

                    for (int x = minCell.x; x <= maxCell.x; x++)
                    {
                        for (int y = minCell.y; y <= maxCell.y; y++)
                        {
                            Vector3Int cellPos = new Vector3Int(x, y, 0);
                            
                            // 타일이 존재하는 위치라면?
                            if (tilemap.HasTile(cellPos))
                            {
                                Vector3 effectPos = tilemap.GetCellCenterWorld(cellPos);
                                PlayEffects(effectPos);
                                
                                tilemap.SetTile(cellPos, null); // 타일 지우기

                                // ⭐ [핵심 추가] 타일이 부서진 위치에 코인 프리팹 소환!
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
            // 에러 무시
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