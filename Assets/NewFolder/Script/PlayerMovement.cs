using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float jumpCutMultiplier = 0.5f; 
    public float jumpSnappiness = 1.5f;
    
    [Header("Dash")]
    public float dashPower = 15f;
    public float dashTime = 0.2f;      
    public float dashCooldown = 1f;
    public float dashCutMultiplier = 0.3f;
    public float upDashTimeMultiplier = 0.4f; 
    public float momentumLingerTime = 0.2f; 
    private float momentumTimer; 
    
    [Header("Coyote Time")]
    public float coyoteTime = 0.1f; 
    private float coyoteCounter; 

    [Header("AfterImage")]
    public GameObject ghostPrefab;       
    public float ghostSpawnDelay = 0.05f;
    private float ghostTimer;            

    [Header("Sounds")]
    public AudioClip jumpSound;
    public AudioClip dashSound;
    [Range(0f, 1f)] 
    public float sfxVolume = 0.5f; 
    
    private AudioSource audioSource; 

    private Color[] rainbowColors = new Color[] {
        new Color(1f, 0f, 0f, 0.6f),       
        new Color(1f, 0.5f, 0f, 0.6f),     
        new Color(1f, 1f, 0f, 0.6f),       
        new Color(0f, 1f, 0f, 0.6f),       
        new Color(0f, 0f, 1f, 0.6f),       
        new Color(0.29f, 0f, 0.51f, 0.6f), 
        new Color(0.56f, 0f, 1f, 0.6f)     
    };
    private int colorIndex = 0; 

    private bool canDash = true;
    private bool isDashing;
    private bool isDashJumping; 
    private bool isAirDashFloating; 

    private int jumpCount = 0; 
    private bool canAirDash; 

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer; 
    private Collider2D bodyCollider; 
    
    private float moveInput;
    private bool isGrounded = true;
    private float defaultGravity; 

    private Coroutine dashCoroutine; 
    private Vector2 currentDashDir;
    
    private float lastGroundY;
    private float singleJumpHeight; 
    private float maxDoubleJumpHeight;
    private float activeCeilingLimit; 

    private float lastJumpInputTime = -10f;
    private float lastDashInputTime = -10f;
    private float inputBufferWindow = 0.1f; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); 
        spriteRenderer = GetComponent<SpriteRenderer>(); 
        bodyCollider = GetComponent<Collider2D>(); 
        audioSource = GetComponent<AudioSource>(); 

        defaultGravity = rb.gravityScale * jumpSnappiness;
        rb.gravityScale = defaultGravity;
        jumpForce = jumpForce * Mathf.Sqrt(jumpSnappiness);

        float g = Mathf.Abs(Physics2D.gravity.y) * defaultGravity;
        
        singleJumpHeight = (jumpForce * jumpForce) / (2f * g);
        maxDoubleJumpHeight = singleJumpHeight * 2f; 
        activeCeilingLimit = maxDoubleJumpHeight;
    }

    void Update()
    {
        // ⭐ [에러 방어막] 시간이 멈췄을 때(일시정지 중)는 아래의 모든 키보드 입력을 무시하고 넘깁니다!
        if (Time.timeScale == 0f) return;

        CheckGrounded();

        if (isGrounded)
        {
            lastGroundY = transform.position.y;
            coyoteCounter = coyoteTime; 
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        float absoluteMaxY = lastGroundY + activeCeilingLimit;
        
        if (!isGrounded && rb.velocity.y > 0 && transform.position.y >= absoluteMaxY)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f); 
            if (isDashing && currentDashDir.y > 0)
            {
                EndDashEarly(); 
            }
        }

        if (momentumTimer > 0)
        {
            momentumTimer -= Time.deltaTime;
        }

        if (isDashing || isDashJumping || isAirDashFloating || momentumTimer > 0)
        {
            ghostTimer -= Time.deltaTime; 
            
            if (ghostTimer <= 0) {
                GameObject ghost = Instantiate(ghostPrefab, transform.position, transform.rotation);
                ghost.transform.localScale = transform.localScale;

                SpriteRenderer ghostSr = ghost.GetComponent<SpriteRenderer>(); 
                ghostSr.sprite = spriteRenderer.sprite;
                ghostSr.color = rainbowColors[colorIndex];

                int mainCharacterOrder = spriteRenderer.sortingOrder;
                ghostSr.sortingOrder = mainCharacterOrder - 1;

                colorIndex++;
                if (colorIndex >= rainbowColors.Length) colorIndex = 0; 

                ghostTimer = ghostSpawnDelay;
            }
        }

        moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput != 0 && !isDashing)
        {
            if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
            else transform.localScale = new Vector3(1, 1, 1);
        }

        // ⭐ 잃어버렸던 공격 중 마스킹 로직 완벽 복구!
        bool isAttacking = anim.GetCurrentAnimatorStateInfo(0).IsName("Whiteray_Attack01") || 
                           anim.GetCurrentAnimatorStateInfo(0).IsName("Whiteray_Attack02") || 
                           anim.GetCurrentAnimatorStateInfo(0).IsName("Whiteray_Attack03");

        anim.SetBool("isWalking", moveInput != 0 && !isAttacking);
        anim.SetBool("isJumping", !isGrounded && !isAttacking);
        anim.SetFloat("yVelocity", rb.velocity.y); 
        anim.SetBool("isDashing", isDashing && !isAttacking); 

        bool isJumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.X);
        bool isDashPressed = Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.LeftShift);

        if (isJumpPressed) lastJumpInputTime = Time.time;
        if (isDashPressed) lastDashInputTime = Time.time;

        bool isSimultaneous = Mathf.Abs(lastJumpInputTime - lastDashInputTime) <= inputBufferWindow;

        if (isSimultaneous && moveInput == 0 && (isJumpPressed || isDashPressed))
        {
            if (isGrounded || coyoteCounter > 0f || canAirDash) 
            {
                lastJumpInputTime = -10f;
                lastDashInputTime = -10f;

                if (dashCoroutine != null) StopCoroutine(dashCoroutine);
                isDashing = false;
                isDashJumping = false;
                isAirDashFloating = false;

                if (isGrounded || coyoteCounter > 0f) activeCeilingLimit = singleJumpHeight;
                else
                {
                    canAirDash = false;
                    activeCeilingLimit = maxDoubleJumpHeight;
                }
                
                coyoteCounter = 0f; 
                dashCoroutine = StartCoroutine(DashRoutine(Vector2.up)); 
            }
        }
        else if (isDashPressed && canDash && !isDashing && !isDashJumping && !isAirDashFloating)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            if (jumpCount == 2 && y >= 0) { }
            else
            {
                lastDashInputTime = -10f;

                if (isGrounded || coyoteCounter > 0f || canAirDash)
                {
                    if (!isGrounded && coyoteCounter <= 0f) canAirDash = false;
                    
                    Vector2 dir = new Vector2(x, y);
                    if (dir != Vector2.zero) dir = dir.normalized;
                    
                    if ((isGrounded || coyoteCounter > 0f) && dir.y > 0) activeCeilingLimit = singleJumpHeight;
                    else activeCeilingLimit = maxDoubleJumpHeight;
                    
                    coyoteCounter = 0f; 
                    dashCoroutine = StartCoroutine(DashRoutine(dir));
                }
            }
        }
        else if (isJumpPressed)
        {
            lastJumpInputTime = -10f;

            if (isGrounded || coyoteCounter > 0f)
            {
                if (isDashing)
                {
                    StopCoroutine(dashCoroutine); 
                    isDashing = false;
                    isDashJumping = true; 
                    rb.gravityScale = defaultGravity; 
                }

                jumpCount = 1; 
                activeCeilingLimit = maxDoubleJumpHeight; 
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                PlaySound(jumpSound); 
                
                coyoteCounter = 0f; 
            }
            else
            {
                if (jumpCount < 2 && !isDashing && !isDashJumping && !isAirDashFloating)
                {
                    jumpCount = 2; 
                    activeCeilingLimit = maxDoubleJumpHeight; 
                    rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                    PlaySound(jumpSound); 
                }
            }
        }

        bool isJumpReleased = Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.X);
        if (isJumpReleased)
        {
            if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
            }
        }

        if (isDashing && (Input.GetKeyUp(KeyCode.Z) || Input.GetKeyUp(KeyCode.LeftShift)))
        {
            EndDashEarly();
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, sfxVolume); 
        }
    }

    private bool PiercingRaycast(Vector2 start, Vector2 dir, float length)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(start, dir, length);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject != gameObject && !hit.collider.isTrigger)
            {
                return true;
            }
        }
        return false; 
    }

    private void CheckGrounded()
    {
        float rayLength = 0.2f; 
        float startY = bodyCollider.bounds.min.y + 0.1f; 
        
        Vector2 leftFoot = new Vector2(bodyCollider.bounds.min.x + 0.1f, startY);
        Vector2 rightFoot = new Vector2(bodyCollider.bounds.max.x - 0.1f, startY);
        Vector2 centerFoot = new Vector2(bodyCollider.bounds.center.x, startY); 
        
        bool leftGrounded = PiercingRaycast(leftFoot, Vector2.down, rayLength);
        bool rightGrounded = PiercingRaycast(rightFoot, Vector2.down, rayLength);
        bool centerGrounded = PiercingRaycast(centerFoot, Vector2.down, rayLength);
        
        bool wasGrounded = isGrounded;
        isGrounded = leftGrounded || rightGrounded || centerGrounded;
        
        if (isGrounded && !wasGrounded)
        {
            if (rb.velocity.y > 0) rb.velocity = new Vector2(rb.velocity.x, 0f);

            if (isDashing || isDashJumping || isAirDashFloating)
            {
                if (isDashing && dashCoroutine != null)
                {
                    StopCoroutine(dashCoroutine);
                    rb.gravityScale = defaultGravity;
                    isDashing = false;
                }
                momentumTimer = momentumLingerTime; 
            }
            
            isDashJumping = false; 
            isAirDashFloating = false; 
            jumpCount = 0; 
            canAirDash = true; 
            activeCeilingLimit = maxDoubleJumpHeight; 
            colorIndex = 0;        
        }
    }

    private void EndDashEarly()
    {
        if (dashCoroutine != null) StopCoroutine(dashCoroutine); 
        
        rb.gravityScale = defaultGravity; 
        isDashing = false;               
        rb.velocity = rb.velocity * dashCutMultiplier; 
        
        CapUpwardVelocity();

        if (!isGrounded) isAirDashFloating = true; 
        else momentumTimer = momentumLingerTime; 
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            if (currentDashDir.x != 0 && IsTouchingWall(currentDashDir.x)) EndDashEarly(); 
            else return; 
        }
        
        float currentSpeed = (isDashJumping || isAirDashFloating || momentumTimer > 0) ? dashPower : moveSpeed;
        
        // ⭐ 공격 중 이동 속도 감소 복구!
        bool isAttacking = anim.GetCurrentAnimatorStateInfo(0).IsName("Whiteray_Attack01") || 
                           anim.GetCurrentAnimatorStateInfo(0).IsName("Whiteray_Attack02") || 
                           anim.GetCurrentAnimatorStateInfo(0).IsName("Whiteray_Attack03");

        if (isAttacking) currentSpeed = currentSpeed * 0.5f;

        if (IsTouchingWall(moveInput)) rb.velocity = new Vector2(0f, rb.velocity.y);
        else rb.velocity = new Vector2(moveInput * currentSpeed, rb.velocity.y);

        if (isGrounded && rb.velocity.y > 0 && jumpCount == 0 && !isDashing)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
    }

    private bool IsTouchingWall(float direction)
    {
        if (direction == 0) return false; 
        
        Vector2 center = bodyCollider.bounds.center;
        float distance = (bodyCollider.bounds.size.x / 2f) + 0.1f; 
        
        Vector2 upperSide = new Vector2(center.x, bodyCollider.bounds.max.y - 0.1f);
        Vector2 lowerSide = new Vector2(center.x, bodyCollider.bounds.min.y + 0.1f);
        
        Vector2 rayDir = direction > 0 ? Vector2.right : Vector2.left;
        
        bool hitTopWall = PiercingRaycast(upperSide, rayDir, distance);
        bool hitBottomWall = PiercingRaycast(lowerSide, rayDir, distance);
        
        return hitTopWall || hitBottomWall;
    }

    private IEnumerator DashRoutine(Vector2 dashDir)
    {
        isDashing = true;
        PlaySound(dashSound); 

        StartCoroutine(DashCooldownRoutine()); 

        rb.gravityScale = 0f; 
        currentDashDir = dashDir; 
        rb.velocity = dashDir * dashPower;

        float actualDashTime = dashTime;
        if (dashDir.y > 0) actualDashTime = dashTime * upDashTimeMultiplier;

        yield return new WaitForSeconds(actualDashTime);

        rb.gravityScale = defaultGravity; 
        isDashing = false;

        CapUpwardVelocity();

        if (!isGrounded) isAirDashFloating = true; 
        else momentumTimer = momentumLingerTime; 
    }

    private IEnumerator DashCooldownRoutine()
    {
        canDash = false;
        yield return new WaitForSeconds(canDash ? dashCooldown : dashCooldown); 
        canDash = true;
    }

    private void CapUpwardVelocity()
    {
        if (rb.velocity.y <= 0) return; 

        float absoluteMaxY = lastGroundY + activeCeilingLimit;
        float remainingH = absoluteMaxY - transform.position.y; 

        if (remainingH <= 0f) rb.velocity = new Vector2(rb.velocity.x, 0f); 
        else
        {
            float g = Mathf.Abs(Physics2D.gravity.y) * defaultGravity;
            float maxExitVy = Mathf.Sqrt(2f * g * remainingH);
            
            if (rb.velocity.y > maxExitVy) rb.velocity = new Vector2(rb.velocity.x, maxExitVy);
        }
    }
}