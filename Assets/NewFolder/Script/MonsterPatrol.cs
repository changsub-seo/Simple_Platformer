using UnityEngine;

public class MonsterPatrol : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 2f;
    public bool isFacingRight = true; 
    public bool canMove = true;       

    [Header("센서 설정")]
    public Transform sensorPoint;       
    public float wallCheckDistance = 0.5f;  
    public float ledgeCheckDistance = 1.0f; 
    public LayerMask groundLayer;       

    [Header("막힘(Stuck) 감지 설정")]
    public float stuckTimeLimit = 1.0f; // 지정된 시간(1초) 이상 제자리걸음 시 뒤돌기
    private float stuckTimer = 0f;
    private float lastPosX; // 이전 프레임의 위치 기록용

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lastPosX = transform.position.x;
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        // 1. 앞으로 걷기
        float direction = isFacingRight ? 1f : -1f;
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);

        // 2. 센서 작동 (절벽 및 앞벽 감지)
        CheckForWallsAndLedges(direction);

        // ⭐ 3. 장애물에 막혀서 제자리걸음 중인지 감지하는 로직
        // 현재 위치와 이전 위치의 차이가 거의 없다면(막혔다면) 타이머 증가
        if (Mathf.Abs(transform.position.x - lastPosX) < 0.01f)
        {
            stuckTimer += Time.fixedDeltaTime;
            
            // 1초(stuckTimeLimit) 이상 막혔으면 뒤돌기 실행
            if (stuckTimer >= stuckTimeLimit)
            {
                TurnAround();
            }
        }
        else
        {
            // 정상적으로 이동 중이라면 타이머 초기화
            stuckTimer = 0f;
        }

        // 다음 프레임 비교를 위해 현재 위치 저장
        lastPosX = transform.position.x;
    }

    private void CheckForWallsAndLedges(float direction)
    {
        Vector2 forwardDir = new Vector2(direction, 0f);
        RaycastHit2D wallHit = Physics2D.Raycast(sensorPoint.position, forwardDir, wallCheckDistance, groundLayer);
        RaycastHit2D ledgeHit = Physics2D.Raycast(sensorPoint.position, Vector2.down, ledgeCheckDistance, groundLayer);

        if (wallHit.collider != null || ledgeHit.collider == null)
        {
            TurnAround();
        }
    }

    private void TurnAround()
    {
        isFacingRight = !isFacingRight;
        
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;

        // 뒤돌았으니 막힘 타이머도 즉시 0으로 리셋
        stuckTimer = 0f; 
    }

    private void OnDrawGizmos()
    {
        if (sensorPoint != null)
        {
            float direction = isFacingRight ? 1f : -1f;
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(sensorPoint.position, sensorPoint.position + new Vector3(direction * wallCheckDistance, 0f, 0f));
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(sensorPoint.position, sensorPoint.position + new Vector3(0f, -ledgeCheckDistance, 0f));
        }
    }
}