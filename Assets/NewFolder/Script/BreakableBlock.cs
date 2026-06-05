using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableBlock : MonoBehaviour
{
    [Header("Break Conditions (파괴 허용 방향)")]
    public bool canBreakFromTop = true;    
    public bool canBreakFromBottom = true; 
    public bool canBreakFromLeft = false;  
    public bool canBreakFromRight = false; 

    [Header("Effects")]
    public GameObject breakEffectPrefab; 
    public AudioClip breakSound;         
    [Range(0f, 1f)] public float soundVolume = 1f;

    [Header("드랍 아이템")]
    public GameObject coinPrefab; 

    private bool isBroken = false;

    // ⭐ [핵심] Collision 대신 Trigger(센서)에 닿았을 때 발동!
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 플레이어가 블록의 어느 쪽에서 왔는지 방향을 수학적으로 계산합니다.
            Vector2 dir = (collision.transform.position - transform.position).normalized;
            bool shouldBreak = false;

            // Y축(위아래) 차이가 X축(좌우) 차이보다 크다면?
            if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
            {
                if (dir.y > 0.5f && canBreakFromTop) shouldBreak = true;    // 위에서 밟음
                if (dir.y < -0.5f && canBreakFromBottom) shouldBreak = true; // 아래서 침
            }
            else // 좌우에서 쳤다면?
            {
                if (dir.x < -0.5f && canBreakFromLeft) shouldBreak = true;  // 왼쪽에서 침
                if (dir.x > 0.5f && canBreakFromRight) shouldBreak = true;  // 오른쪽에서 침
            }

            if (shouldBreak)
            {
                BreakBlock();
            }
        }
    }

    public void BreakBlock()
    {
        if (!this || !gameObject) return;
        if (isBroken) return;
        isBroken = true;

        if (coinPrefab != null) Instantiate(coinPrefab, transform.position, Quaternion.identity);
        if (breakSound != null) AudioSource.PlayClipAtPoint(breakSound, Camera.main.transform.position, soundVolume);
        if (breakEffectPrefab != null) Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);

        Tilemap tilemap = GetComponentInParent<Tilemap>();
        if (tilemap != null)
        {
            Vector3Int cellPosition = tilemap.WorldToCell(transform.position);
            tilemap.SetTile(cellPosition, null);
        }

        Destroy(gameObject);
    }
}