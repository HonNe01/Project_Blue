using System.Collections;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Move Setting")]
    public float moveSpeed = 5f;        // 이동속도
    private float inputValueX;

    [Header("Jump Setting")]
    public float jumpForce = 12f;       // 점프 파워
    public float jumpTimeMax = 0.3f;    // 점프 키 입력 유지 최대 시간
    [SerializeField] private int jumpCount = 0;          // 현재 점프 횟수
    private int maxJumps = 2;           // 최대 점프 횟수
    //private bool isDroppingDown = false;// 아래 점프 중인지 체크
    private float jumpTimeCounter;      // 점프 키 유지 시간 카운트

    [Header("Wall Jump/Sliding Setting")]
    private bool isTouchingWall = false;
    private bool isWallSliding = false;
    private bool isWallJumping = false;
    public float wallJumpYForce = 8f;   // 벽점프 파워 (수직)
    public float wallJumpXForce = 7f;   // 벽점프 파워 (수평)
    public float wallSlideSpeed = 0.5f; // 벽 슬라이드 속도
    public float wallJumpDuration = 0.3f; // 벽점프 길이
    [SerializeField] private int wallDir = 0; // 벽 위치(왼쪽 -1, 오른쪽 1)


    [Header("Coyote / Buffer")]
    public float coyoteTime = 0.1f; // 땅에서 떨어진 후 점프 가능한 시간
    private float coyoteTimeCounter;
    public float jumpBufferTime = 0.1f; // 점프키 입력을 미리 받아두는 시간
    private float jumpBufferCounter;

    [Header("Dash Setting")]
    public int dashDir = 0;
    [SerializeField] private bool isDashing = false;
    [SerializeField] private bool canAirDash = true;
    public float dashSpeed = 15f; // 대쉬 속도
    public float dashTime = 0.2f; // 대쉬 지속 시간     대쉬 거리 계산 = dashSpeed * dashTime
    public float dashCooldown = 0.8f; // 지상 대쉬 쿨타임
    private float dashTimeCounter;
    private float dashCooldownCounter;
    private float defaultGravity; // 현재 중력값 저장

    [Header("Effect")]


    //[Header("가드")]
    //Player_Guard _playerGuard;

    // PlayerState 참조
    Animator anim;
    Rigidbody2D rb;
    Collider2D coll;
    SpriteRenderer sprite;

    void Start()
    {
        defaultGravity = PlayerState.instance.rb.gravityScale;

        // 컴포넌트 참조
        anim = PlayerState.instance.anim;
        rb = PlayerState.instance.rb;
        coll = PlayerState.instance.coll;
        sprite = PlayerState.instance.sprite;
    }

    private void Update()   // 논리 로직
    {
        MoveInput();
        JumpInput();
        DashInput();

        // === Wall Check ===
        isWallSliding = isTouchingWall && 
                        rb.linearVelocity.y < 0 && 
                        inputValueX == wallDir;

        if (PlayerState.instance.isGround)
        {
            jumpCount = 0;
            coyoteTimeCounter = coyoteTime;
            //isDroppingDown = false; // 아래점프 상태 해제

            canAirDash = true;
            isWallSliding = false;
        }
        else if (isTouchingWall)
        {
            jumpCount = 0;
            coyoteTimeCounter = coyoteTime;
            canAirDash = true;
        }
    }

    private void FixedUpdate()  // 물리 로직
    {
        MovePhysics();
        JumpPhysics();
        DashPhysics();
    }

    private void LateUpdate()   // 그래픽 로직
    {
        anim.SetFloat("SpeedX", Mathf.Abs(inputValueX));
        anim.SetFloat("SpeedY", rb.linearVelocityY);

        anim.SetBool("IsGround", PlayerState.instance.isGround);
        anim.SetBool("IsDash", isDashing);
        anim.SetBool("IsWallSliding", isWallSliding);

        if (isDashing)
        {
            sprite.flipX = dashDir < 0;
        }
        else if (isWallJumping)
        {
            // 벽 점프시 좌우 반전
            sprite.flipX = wallDir > 0;
        }
        else if (isWallSliding)
        {
            // 벽 슬라이딩시 좌우 반전
            sprite.flipX = wallDir < 0;
        }
        else if (inputValueX != 0)
        {
            // 이동 시 좌우 반전
            sprite.flipX = PlayerState.instance.isRight < 0;
        }
    }

    private void MoveInput()
    {
        if (!PlayerState.instance.canMove)
        {
            anim.SetFloat("SpeedX", 0);

            return;
        }

        // 입력값 받기
        inputValueX = Input.GetAxisRaw("Horizontal");

        // 좌우 체크
        if (inputValueX != 0)
        {
            PlayerState.instance.isRight = inputValueX > 0 ? 1 : -1;

        }
    }

    private void MovePhysics()
    {
        if (!PlayerState.instance.canMove)
        {
            return;
        }

        // 이동 실행
        float targetY = isWallSliding ? Mathf.Lerp(rb.linearVelocity.y, -wallSlideSpeed, 0.5f) : rb.linearVelocity.y;
        rb.linearVelocity = new Vector2(inputValueX * moveSpeed, targetY);
    }

    void JumpInput()
    {
        // 코요테타임 카운트 (지상일 때 리셋, 공중일 때 감소)
        if (!PlayerState.instance.isGround)
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // 점프버튼 누르면 버퍼 카운트 시작
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (PlayerState.instance.canJump)
                jumpBufferCounter = jumpBufferTime;
        }
        // 카운터 갱신
        else
            jumpBufferCounter -= Time.deltaTime;

        // 짧은 점프 (점프 중 키를 떼면 즉시 떨어지도록 처리)
        if (Input.GetKeyUp(KeyCode.Z))
        {
            if (jumpTimeCounter > 0f) // 지정시간보다 빨리 떼면
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // 상승력 초기화 → 바로 떨어짐
                jumpTimeCounter = 0f;
            }
        }
    }

    void JumpPhysics()
    {
        if (!PlayerState.instance.canJump) return;

        if (jumpBufferCounter > 0f)
        {
            PlayerState.instance.isGround = false;
            anim.SetTrigger("IsJump");

            // 아래점프
            if (IsOnPlatform() && Input.GetKey(KeyCode.DownArrow))
            {
                Collider2D platform = GetPlatformBelow();
                if (platform != null)
                    StartCoroutine(DisableSinglePlatform(platform)); // 플랫폼 충돌 무시

                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                jumpCount = 1;
                jumpBufferCounter = 0f;
                return;
            }

            // 벽점프 처리
            if (isWallSliding)
            {
                StartCoroutine(WallJumpLock(wallJumpDuration)); // 점프 후 이동 잠금
                jumpCount++; // 벽점프 후 더블점프 가능
                jumpBufferCounter = 0f;
                jumpTimeCounter = 0f; // 벽점프는 키 누른 시간 계산 안함

                rb.linearVelocity = Vector2.zero;
                rb.AddForce(new Vector2(-wallDir * wallJumpXForce, wallJumpYForce), ForceMode2D.Impulse);
                return;
            }

            // 일반 점프 (지상/더블점프)
            if (coyoteTimeCounter > 0f || jumpCount < maxJumps)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 1.5f); // 점프 초기 속도만 적용
                jumpCount++;
                jumpBufferCounter = 0f;
                jumpTimeCounter = jumpTimeMax; // 점프 키 유지 최대 시간
            }
        }
        
        // 일반 점프 카운터
        if (jumpTimeCounter > 0f)
        {
            jumpTimeCounter -= Time.deltaTime;
        }
    }

    public IEnumerator WallJumpLock(float duration)
    {
        // 벽 점프 중 이동 불가
        isTouchingWall = false;
        isWallJumping = true;

        float timer = 0;
        while (timer < duration)
        {
            PlayerState.instance.canMove = false;
            PlayerState.instance.canJump = false;

            timer += Time.deltaTime;
            yield return null;
        }
        
        // 이동 불가 해제
        isWallJumping = false;
        PlayerState.instance.canMove = true;
        PlayerState.instance.canJump = true;
    }

    void DashInput()
    {
        // 대쉬 쿨타임 감소
        if (dashCooldownCounter > 0f)
            dashCooldownCounter -= Time.deltaTime;

        // 대쉬 쿨타임 초기화
        if (PlayerState.instance.isGround) // 지상에서는 쿨다운 체크
        {
            PlayerState.instance.canDash = !isDashing && dashCooldownCounter <= 0f;
            canAirDash = true; // 땅에 닿으면 공중 대쉬 다시 사용 가능
        }
        else if (isTouchingWall) // 벽에 닿으면 공중 대쉬 재사용 가능
        {
            PlayerState.instance.canDash = !isDashing && canAirDash;
            canAirDash = true; // 벽에 닿으면 재사용 가능
        }
        else // 공중
        {
            PlayerState.instance.canDash = !isDashing && canAirDash; // 최초 1회 가능
        }

        // 대쉬 실행
        if (Input.GetKeyDown(KeyCode.X) && PlayerState.instance.canDash)
        {
            isDashing = true;
            dashTimeCounter = dashTime;

            // 벽 대쉬
            if (isTouchingWall)
            {
                dashDir = -wallDir;
                PlayerState.instance.isRight = dashDir;

                isTouchingWall = false;
            }
            // 일반 대쉬
            else
            {
                dashDir = PlayerState.instance.isRight;
            }
                
            // 대쉬 쿨
            if (PlayerState.instance.isGround)  // 지상 대쉬 -> 쿨다운 적용
                dashCooldownCounter = dashCooldown;
            else                                // 그 외 대쉬 -> 
            {
                canAirDash = false; // 땅/벽 닿기 전까지 재사용 불가
            }   
        }
    }

    void DashPhysics()
    {
        // 대쉬 중
        if (isDashing)
        {
            DashLock(true);
            rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);
            
            // 대쉬 시간 카운트
            dashTimeCounter -= Time.deltaTime;

            if (dashTimeCounter <= 0f)
            {
                isDashing = false;
                DashLock(false);
            }
        }
    }

    private void DashLock(bool isStart = false)
    {
        if (isStart)
        {
            PlayerState.instance.canMove = false;
            PlayerState.instance.canJump = false;
            PlayerState.instance.canGuard = false;
            PlayerState.instance.canAttack = false;
            PlayerState.instance.canSkill = false;
            rb.gravityScale = 0f;
        }
        else
        {
            PlayerState.instance.canMove = true;
            PlayerState.instance.canJump = true;
            PlayerState.instance.canGuard = true;
            PlayerState.instance.canAttack = true;
            PlayerState.instance.canSkill = true;
            rb.gravityScale = defaultGravity;
        }
    }

    private bool IsOnPlatform()
    {
        Collider2D hit = Physics2D.OverlapBox(transform.position, PlayerState.instance.groundCheck, 0f, LayerMask.GetMask("Ground"));

        return hit != null && hit.CompareTag("OneWayPlatform");
    }

    private Collider2D GetPlatformBelow()
    {
        Collider2D hit = Physics2D.OverlapBox(transform.position, PlayerState.instance.groundCheck, 0f, LayerMask.GetMask("Ground"));

        return (hit != null && hit.CompareTag("OneWayPlatform")) ? hit : null;
    }

    private IEnumerator DisableSinglePlatform(Collider2D platform)
    {
        Physics2D.IgnoreCollision(coll, platform, true);
        yield return new WaitForSeconds(0.3f); // 플랫폼 통과 시간
        Physics2D.IgnoreCollision(coll, platform, false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            if (isDashing)
            {
                isDashing = false;
                DashLock(false);
            }
            if (!isTouchingWall)
            {
                rb.linearVelocity = Vector2.zero; // 벽에 닿으면 속도 초기화
                isTouchingWall = true;
                isWallJumping = false;
            }

            // 벽의 방향(왼쪽 -1, 오른쪽 1)
            ContactPoint2D contact = collision.contacts[0];
            wallDir = (contact.point.x < transform.position.x) ? -1 : 1;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isTouchingWall = false;
        }
    }
}
