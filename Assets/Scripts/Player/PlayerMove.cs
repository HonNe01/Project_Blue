using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("이동 및 점프 설정")]
    public float moveSpeed = 5f;              // 이동속도
    public float jumpForce = 7f;           // 점프높이
    public float wallJumpForce = 5f;      // 벽점프 수평 힘
    public float wallJumpVertical = 7f;   // 벽점프 수직 힘
    private Rigidbody2D rb;
    private Collider2D playerCollider;

    [Header("점프 설정")]
    private int jumpCount = 0;
    private int maxJumps = 2;           // 최대점프 횟수 정하기

    [Header("벽 점프")]
    private bool isTouchingWall = false;
    private int wallDir = 0; // 1 = 오른쪽, -1 = 왼쪽

    private bool isRising = false;      // 점프 중 위로 올라가는 중
    private bool isDroppingDown = false; // 아래점프 중인지 체크

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
        Jump();
    }

    void Move()
    {
        Vector2 move = Vector2.zero;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            move = Vector2.left;

            //transform.localScale = new Vector2(-1f, 1f);
            sprite.flipX = true;    // 좌우 대칭 코드 수정. 크기 변경이 아닌 Sprite 컴포넌트 플립 기능 이용.
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            move = Vector2.right;

            //transform.localScale = new Vector2(1f, 1f);
            sprite.flipX = true;
        }

        rb.linearVelocity = new Vector2(move.x * moveSpeed, rb.linearVelocity.y);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            // 아래점프 처리              *수정 필요* => 일반 점프 중 점프키를 누르고 있지 않아도, 아래키를 누르면 플랫폼에 안올라가지는 아래점프 상태 버그 존재.
            if (IsOnPlatform() && Input.GetKey(KeyCode.DownArrow))
            {
                if (!isDroppingDown)
                {
                    Collider2D platformCollider = GetPlatformBelow();
                    if (platformCollider != null)
                    {
                        StartCoroutine(DisableSinglePlatform(platformCollider));
                    }
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                    isDroppingDown = true; // 아래점프 상태 시작
                }
                return;
            }

            // 아래점프 중이면 점프 입력 무시
            if (isDroppingDown) return;

            // 벽점프 처리           *수정 필요* => 벽 점프시 좌우 입력 해제하여 반대 방향으로 점프하도록 해야함.
            if (isTouchingWall)
            {
                rb.linearVelocity = new Vector2(-wallDir * wallJumpForce, wallJumpVertical);
                jumpCount = 1;
                return;
            }

            // 일반 점프
            if (jumpCount < maxJumps)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                jumpCount++;
                isRising = true;
                StartCoroutine(EnablePlatformCollisionAtPeak());
            }
        }
    }

    // 플레이어가 플랫폼 위에 있는지 체크
    private bool IsOnPlatform()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f);
        if (hit.collider != null && hit.collider.CompareTag("OneWayPlatform"))
        {
            return true;
        }
        return false;
    }

    // 바로 아래 플랫폼 하나 가져오기
    private Collider2D GetPlatformBelow()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f);
        if (hit.collider != null && hit.collider.CompareTag("OneWayPlatform"))
        {
            return hit.collider;
        }
        return null;
    }

    // 아래점프: 단일 플랫폼 무시
    private IEnumerator DisableSinglePlatform(Collider2D platform)
    {
        Physics2D.IgnoreCollision(playerCollider, platform, true);
        yield return new WaitForSeconds(2f);                                  // 플랫폼을 통과할 수 있는 시간 조정 
        Physics2D.IgnoreCollision(playerCollider, platform, false);
    }

    // 점프 중: 최대 높이에 도달하면 플랫폼 충돌 다시 적용
    private IEnumerator EnablePlatformCollisionAtPeak()
    {
        Collider2D platform = GetPlatformBelow();
        if (platform != null)
        {
            Physics2D.IgnoreCollision(playerCollider, platform, true);

            // 올라가는 동안 기다리기
            while (rb.linearVelocity.y > 0)
            {
                yield return null;
            }

            // 최고점 도달하면 충돌 다시 적용
            Physics2D.IgnoreCollision(playerCollider, platform, false);
            isRising = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("OneWayPlatform"))
        {
            jumpCount = 0;
            isTouchingWall = false;
            isDroppingDown = false; // 아래점프 상태 해제
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            isTouchingWall = true;
            wallDir = (collision.transform.position.x > transform.position.x) ? 1 : -1;
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
