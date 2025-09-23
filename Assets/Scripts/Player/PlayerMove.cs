using UnityEngine;
using System.Collections;

public class PlayerMove : MonoBehaviour
{
    [Header("Move Setting")]
    public float moveSpeed = 5f;        // 이동속도
    private float inputValueX;
    
    [Header("Jump Setting")]
    public float jumpForce = 12f;       // 점프 파워
    public float jumpTimeMax = 0.3f;    // 점프 키 입력 유지 최대 시간
    private int jumpCount = 0;
    private int maxJumps = 2;           // 최대점프 횟수
    private bool isDroppingDown = false;// 아래점프 중인지 체크
    private float jumpTimeCounter;      // 점프 키 유지 시간 카운트

    [Header("Wall Jump/Sliding Setting")]
    public float wallJumpYForce = 8f;   // 벽점프 파워 (수직)
    public float wallJumpXForce = 7f;   // 벽점프 파워 (수평)
    public float wallSlideSpeed = 0.5f; // 벽 슬라이드 속도
    private bool isTouchingWall = false;
    private int wallDir = 0; // 1 = 왼쪽벽 기준 오른쪽, -1 = 오른쪽벽 기준 왼쪽
    private bool isWallSliding = false;
    private bool isWallJumping = false;

    [Header("코요테타임 & 버퍼")]
    public float coyoteTime = 0.1f; // 땅에서 떨어진 후 점프 가능한 시간
    private float coyoteTimeCounter;
    public float jumpBufferTime = 0.1f; // 점프키 입력을 미리 받아두는 시간
    private float jumpBufferCounter;

    [Header("대쉬 설정")]
    public float dashSpeed = 15f; // 대쉬 속도
    public float dashTime = 0.2f; // 대쉬 지속 시간     대쉬 거리 계산 = dashSpeed * dashTime
    public float dashCooldown = 0.8f; // 지상 대쉬 쿨타임
    private bool isDashing = false;
    private float dashTimeCounter;
    private float dashCooldownCounter;
    private bool canAirDash = true;
    private float defaultGravity; // 현재 중력값 저장

    //[Header("가드")]
    //Player_Guard _playerGuard;

    // PlayerState 참조
    Animator anim;
    Rigidbody2D rb;
    Collider2D coll;
    SpriteRenderer sprite;

    PlayerMove playerMove;
    PlayerAttack playerAttack;
    PlayerGuard playerGuard;

    
    void Start()
    {
        defaultGravity = PlayerState.instance.rb.gravityScale;

        // 컴포넌트 참조
        anim = PlayerState.instance.anim;
        rb = PlayerState.instance.rb;
        coll = PlayerState.instance.coll;
        sprite = PlayerState.instance.sprite;

        // 스크립트 참조
        playerMove = PlayerState.instance.playerMove;
        playerAttack = PlayerState.instance.playerAttack;
        playerGuard = PlayerState.instance.playerGuard;
    }

    private void Update()
    {
        Move();
        HandleJumpInput();
        Jump();
        dash();
    }

    private void FixedUpdate()
    {
        // Move
        float targetY = isWallSliding ? Mathf.Lerp(rb.linearVelocity.y, -wallSlideSpeed, 0.5f) : rb.linearVelocity.y;
        rb.linearVelocity = new Vector2(inputValueX * moveSpeed, targetY);

        isWallSliding = isTouchingWall && rb.linearVelocity.y < 0;

        // Jump

        // Dash
    }

    private void LateUpdate()
    {
        if (inputValueX != 0)
        {
            sprite.flipX = PlayerState.instance.isRight < 0;
        }
    }

    void Move()
    {
        // 벽점프, 대쉬, 공격, 방어 , 힐 중이면 이동 불가
        /*if (isWallJumping || isDashing || (playerAttack != null && playerAttack._currentCombo >= 0) ||
            (guard != null && guard.isGuard) || (playerHealth != null && PlayerState.instance.isHealing))
        {
            PlayerState.instance.canMove = false;
            return;
        }*/

        if (isWallJumping || isDashing)
        {
            PlayerState.instance.canMove = false;
        }

        if (!PlayerState.instance.canMove)
        {
            anim.SetFloat("SpeedX", 0);

            return;
        }

        // 입력값 받기
        inputValueX = Input.GetAxisRaw("Horizontal");

        // 애니메이션 파라미터 설정
        anim.SetFloat("SpeedX", Mathf.Abs(inputValueX));
        anim.SetFloat("SpeedY", rb.linearVelocityY);

        // 좌우 체크
        if (inputValueX != 0)
        {
            PlayerState.instance.isRight = inputValueX > 0 ? 1 : -1;
        }
    }

    // 점프버퍼 처리
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
            if (PlayerState.instance.canJump) return;

            // 아래점프 처리 (원웨이 플랫폼 통과)
            if (IsOnPlatform() && Input.GetKey(KeyCode.DownArrow) && !isDroppingDown)
            {
                Collider2D platform = GetPlatformBelow();
                if (platform != null)
                    StartCoroutine(DisableSinglePlatform(platform)); // 플랫폼 충돌 무시

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
                rb.AddForce(new Vector2(wallDir * wallJumpYForce, wallJumpXForce * 1.5f), ForceMode2D.Impulse);
                StartCoroutine(WallJumpLock(0.3f)); // 점프 후 이동 잠금
                jumpCount = 1; // 벽점프 후 더블점프 가능
                jumpBufferCounter = 0f;
                jumpTimeCounter = 0f; // 벽점프는 키 누른 시간 계산 안함
                return;
            }
            

            // 일반 점프 (지상/더블점프)
            if (coyoteTimeCounter > 0f || jumpCount < maxJumps)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce*1.5f); // 점프 초기 속도만 적용
                jumpCount++;
                jumpBufferCounter = 0f;
                jumpTimeCounter = jumpTimeMax; // 점프 키 유지 최대 시간
            }
        }
       
        // 짧은 점프 (점프 중 키를 떼면 즉시 떨어지도록 처리)
        if (Input.GetKeyUp(KeyCode.Z))
        {
            if (jumpTimeCounter > 0f) // 지정시간보다 빨리 떼면
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // 상승력 초기화 → 바로 떨어짐
                jumpTimeCounter = 0f;
            }
        }

        // 시간 카운트 감소
        if (jumpTimeCounter > 0f)
        {
            jumpTimeCounter -= Time.deltaTime;
        }
    }

    void dash()
    {
        PlayerState.instance.canDash = false;
        GetComponent<Animator>().SetBool("IsDash", isDashing);
        if (IsGrounded()) // 지상에서는 쿨다운 체크
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

        if (Input.GetKeyDown(KeyCode.X) && PlayerState.instance.canDash)
        {
            isDashing = true;
            dashTimeCounter = dashTime;

            if (IsGrounded()) // 땅에서 대쉬면 쿨다운 적용
                dashCooldownCounter = dashCooldown;

            if (!IsGrounded() && !isTouchingWall) // 공중대쉬 사용 후
                canAirDash = false; // 땅/벽 닿기 전까지 재사용 불가

            
            rb.gravityScale = 0f; // 중력 무시
        }

        // 대쉬 중 처리
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(sprite.flipX ? -dashSpeed : dashSpeed, 0f);
            dashTimeCounter -= Time.deltaTime;

            if (dashTimeCounter <= 0f)
            {
                isDashing = false;
                rb.gravityScale = defaultGravity; // 중력 복구
            }
        }

        // 땅에서 대쉬 쿨타임 감소
        if (dashCooldownCounter > 0f)
            dashCooldownCounter -= Time.deltaTime;
    }

    // 지상 체크 (코요테타임용)
    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.3f);
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
        Physics2D.IgnoreCollision(coll, platform, true);
        yield return new WaitForSeconds(0.3f); // 플랫폼 통과 시간
        Physics2D.IgnoreCollision(coll, platform, false);
    }

    private IEnumerator WallJumpLock(float duration)
    {
        // 벽 점프 중 이동 불가
        isWallJumping = true;
        PlayerState.instance.canMove = false;
        PlayerState.instance.canJump = true;
        
        // 이동 불가 해제
        yield return new WaitForSeconds(duration);
        isWallJumping = false;
        PlayerState.instance.canMove = true;
        PlayerState.instance.canJump = false;
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

            rb.linearVelocity = Vector2.zero; // 벽에 닿으면 속도 초기화
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
