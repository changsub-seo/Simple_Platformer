using UnityEngine;

public class CoinItem : MonoBehaviour
{
    [Header("코인 설정")]
    public int coinValue = 1;
    public AudioClip eatSound;
    [Range(0f, 1f)] public float volume = 1f;

    [Header("물리 튀어오르기")]
    public float jumpForceX = 3f; 
    public float jumpForceY = 6f; 

    private Rigidbody2D rb;
    private bool canCollect = false; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        float randomX = Random.Range(-jumpForceX, jumpForceX);
        rb.velocity = new Vector2(randomX, jumpForceY);
        Invoke("EnableCollection", 0.1f);
    }

    void EnableCollection()
    {
        canCollect = true;
    }

    void Update()
    {
        // 1. 빙글빙글 도는 애니메이션
        float spin = Mathf.Sin(Time.time * 6f); 
        transform.localScale = new Vector3(spin, 1, 1);

        // 2. ⭐ [핵심] 플레이어 감지 (물리 충돌 무시 상태에서도 작동함!)
        if (canCollect)
        {
            // 내 위치를 기준으로 반경 0.6 내에 있는 모든 물체를 스캔
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.6f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    CollectCoin();
                    break;
                }
            }
        }
    }

    private void CollectCoin()
    {
        if (GameManager.instance != null) GameManager.instance.AddCoin(coinValue);
        if (eatSound != null) AudioSource.PlayClipAtPoint(eatSound, Camera.main.transform.position, volume);
        Destroy(gameObject);
    }
}