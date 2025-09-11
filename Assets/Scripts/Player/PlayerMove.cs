using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("이동 및 점프 설정")]
    public float moveSpeed = 5f;              // 이동속도
    public float jumpForce = 7f;              // 점프 초기 속도
    public float wallJumpForce = 8f;          // 벽점프 (값을 올리면 벽점프시 위쪽으로 올라가는 값 증가)
    public float wallJumpVertical = 7f;       // 벽점프 (값을 올리면 반대편으로 나가는 값 증가)
    public float jumpTimeMax = 0.3f;          // 키를 누를 수 있는 최대 점프 시간
    private Rigidbody2D rb;
    private Collider2D playerCollider;

    [Header("점프 설정")]
    private int jumpCount = 0;
    private int maxJumps = 2;                 // 최대점프 횟수 정하기
    private bool isDroppingDown = false;      // 아래점프 중인지 체크
    private float jumpTimeCounter;            // 현재 점프 지속 시간 카운트

    [Header("벽 점프 / 슬라이드")]
    public float wallSlideSpeed = 0.5f;       // 벽 슬라이드 속도
    private bool isTouchingWall = false;
    private int wallDir = 0;                  // 1 = 왼쪽벽 기준 오른쪽으로, -1 = 오른쪽벽 기준 왼쪽으로
    private bool isWallSliding = false;
    private bool isWallJumping = false;

    [Header("코요테타임 & 버퍼")]
    public float coyoteTime = 0.1f;           // 땅에서 떨어진 후 점프 가능한 시간
    private float coyoteTimeCounter;
    public float jumpBufferTime = 0.1f;       // 점프키 입력을 미리 받아두는 시간
    private float jumpBufferCounter;

    [Header("대쉬 설정")]
    public float dashSpeed = 15f;         // 대쉬 속도
    public float dashTime = 0.2f;         // 대쉬 지속 시간    대쉬거리는 속도*시간
    public float dashCooldown = 0.8f;       // 지상에서 대쉬 쿨타임
    private bool isDashing = false;       
    private float dashTimeCounter;        
    private float dashCooldownCounter;    
    private bool canAirDash = true;

    [Header("대쉬 트레일")]
    public TrailRenderer dashTrail;       // 트레일 렌더러 참조

    [Header("참조")]
    SpriteRenderer sprite;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        Move();
        HandleJumpInput(); 
        Jump();            
        dash();
    }

    void Move()
    {
        Vector2 move = Vector2.zero;
        if (isWallJumping) return;

        if (Input.GetKey(KeyCode.LeftArrow))               // 좌우 방향설정
        { move = Vector2.left; sprite.flipX = true; }
        else if (Input.GetKey(KeyCode.RightArrow))
        { move = Vector2.right; sprite.flipX = false; }

        float targetY = isWallSliding ? Mathf.Lerp(rb.linearVelocity.y, -wallSlideSpeed, 0.5f) : rb.linearVelocity.y;
        rb.linearVelocity = new Vector2(move.x * moveSpeed, targetY);

        isWallSliding = isTouchingWall && rb.linearVelocity.y < 0;
    }

   //점프버퍼
    void HandleJumpInput()
    {
        // 점프버튼 누르면 버퍼 카운트 시작
        if (Input.GetKeyDown(KeyCode.Z))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // 코요테타임 카운트 (지상일 때 리셋, 공중일 때 감소)
        if (IsGrounded())
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;
    }

    void Jump()
    {
        // 점프버퍼 체크
        if (jumpBufferCounter > 0f)
        {
            // 아래점프 처리
            if (IsOnPlatform() && Input.GetKey(KeyCode.DownArrow) && !isDroppingDown)
            {
                Collider2D platform = GetPlatformBelow();
                if (platform != null) StartCoroutine(DisableSinglePlatform(platform)); // 플랫폼 충돌 무시
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                isDroppingDown = true;
                jumpBufferCounter = 0f; 
                return;
            }

            if (isDroppingDown) return;

            // 벽점프 처리 
            if (isTouchingWall)
            {
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(new Vector2(wallDir * wallJumpForce, wallJumpVertical), ForceMode2D.Impulse);
                StartCoroutine(WallJumpLock(0.3f)); // 점프 후 이동 잠금
                jumpCount = 1; // 벽점프 후 더블점프 가능
                jumpBufferCounter = 0f; 
                jumpTimeCounter = 0f;   // 벽점프는 키 누른 시간 계산 안함
                return; 
            }

            // 일반 점프 
            if (coyoteTimeCounter > 0f || jumpCount < maxJumps)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); // 점프 초기 속도 설정
                jumpTimeCounter = jumpTimeMax; // 최대 0.3초 동안 상승 유지
                jumpCount++;
                jumpBufferCounter = 0f;
            }
        }

        
        // 점프키를 누른 시간 동안 상승력 유지 (0.3초 동안)
        if (Input.GetKey(KeyCode.Z) && jumpTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpTimeCounter -= Time.deltaTime;
        }

        // 키를 떼면 남은 시간 무시하고 낮은 점프 적용
        if (Input.GetKeyUp(KeyCode.Z))
        {
            if (jumpTimeCounter > 0f) // 0.3초보다 빨리 떼면
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.5f); // 낮은 점프
            jumpTimeCounter = 0f;
        }
    }

    void dash()
    {
        bool canDash = false;

        if (IsGrounded()) // 지상에서는 쿨다운 체크
        {
            canDash = !isDashing && dashCooldownCounter <= 0f;
            canAirDash = true; // 땅에 닿으면 공중 대쉬 다시 사용 가능
        }
        else if (isTouchingWall) // 벽에 닿으면 공중 대쉬 재사용 가능
        {
            canDash = !isDashing && canAirDash;
            canAirDash = true; // 벽에 닿으면 재사용 가능
        }
        else // 공중
        {
            canDash = !isDashing && canAirDash; // 최초 1회 가능
        }

        if (Input.GetKeyDown(KeyCode.X) && canDash)
        {
            isDashing = true;
            dashTimeCounter = dashTime;

            if (IsGrounded()) // 땅에서 대쉬면 쿨다운 적용
                dashCooldownCounter = dashCooldown;

            if (!IsGrounded() && !isTouchingWall) // 공중대쉬 사용 후
                canAirDash = false; // 땅/벽 닿기 전까지 재사용 불가

            dashTrail.emitting = true; // 대쉬 트레일 켜기
        }

        // 대쉬 중 처리
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(sprite.flipX ? -dashSpeed : dashSpeed, 0f);
            dashTimeCounter -= Time.deltaTime;

            if (dashTimeCounter <= 0f)
            {
                isDashing = false;
                dashTrail.emitting = false; // 트레일 끄기
            }
        }

        // 땅에서 대쉬 쿨타임 감소
        if (dashCooldownCounter > 0f)
            dashCooldownCounter -= Time.deltaTime;
    }

    // 지상 체크 (코요테타임용)
    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f);
        return hit.collider != null && (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("OneWayPlatform"));
    }

    private bool IsOnPlatform()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f);
        return hit.collider != null && hit.collider.CompareTag("OneWayPlatform");
    }

    private Collider2D GetPlatformBelow()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f);
        return (hit.collider != null && hit.collider.CompareTag("OneWayPlatform")) ? hit.collider : null;
    }

    private IEnumerator DisableSinglePlatform(Collider2D platform)
    {
        Physics2D.IgnoreCollision(playerCollider, platform, true);
        yield return new WaitForSeconds(0.3f); // 플랫폼 통과 시간
        Physics2D.IgnoreCollision(playerCollider, platform, false);
    }

    private IEnumerator WallJumpLock(float duration)
    {
        isWallJumping = true;
        yield return new WaitForSeconds(duration);
        isWallJumping = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("OneWayPlatform"))
        {
            jumpCount = 0;
            isDroppingDown = false; // 아래점프 상태 해제
            coyoteTimeCounter = coyoteTime; // 착지시 코요테타임 리셋
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            isTouchingWall = true;
            ContactPoint2D contact = collision.contacts[0];
            wallDir = (contact.point.x < transform.position.x) ? 1 : -1; // 왼쪽벽이면 1, 오른쪽벽이면 -1
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isTouchingWall = false;
            wallDir = 0;
        }
    }
}
