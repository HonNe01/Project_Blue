using UnityEngine;

public class HitboxController : MonoBehaviour
{
    public enum HitboxType { PlayerAttack, PlayerDownAttack, PlayerSkill, Trap, Enemy, Scarecrow }
    public enum EffectType { Slash, Explosion }


    [SerializeField] private int baseDamage = 1;
    [SerializeField] private HitboxType type;


    private float bounceForce = 10f;


    void Awake()
    {
        if (type == HitboxType.Trap)
        {
            gameObject.SetActive(true);
        }

        else if (type != HitboxType.Scarecrow)
        {
            gameObject.SetActive(false);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (type)
        {
            case HitboxType.PlayerAttack:
                if (collision.CompareTag("Enemy"))
                {
                    Debug.Log($"[Player] {collision.gameObject.name} Hit!");
                    PlayerState.instance.AddGauge(5);
                    if (!PlayerState.instance.isGround)
                    {
                        // 공중에선 점프 초기화
                        PlayerState.instance.playerMove.JumpCountReset();
                    }

                    var enemy = collision.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
            case HitboxType.PlayerDownAttack:
                if (collision.CompareTag("Enemy"))
                {
                    Debug.Log($"[Player] {collision.gameObject.name} Hit!");
                    PlayerState.instance.AddGauge(5);
                    if (!PlayerState.instance.isGround)
                    {
                        AttackBounce();
                        PlayerState.instance.playerMove.jumpCount = 1;
                    }

                    var enemy = collision.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
            case HitboxType.PlayerSkill:
                if (collision.CompareTag("Enemy"))
                {
                    Debug.Log($"[Player] {collision.gameObject.name} Hit!");
                    if (!PlayerState.instance.isGround)
                    {
                        // 공중에선 점프 초기화
                        PlayerState.instance.playerMove.JumpCountReset();
                    }

                    var enemy = collision.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
            case HitboxType.Enemy:
                if (collision.CompareTag("Player"))
                {
                    // 패링 확인
                    if (PlayerState.instance.playerGuard.IsParry())
                    {
                        // 보스 스턴
                        var boss = GetComponentInParent<BossBase>();
                        
                        if (boss != null)
                        {
                            boss.state = BossBase.BossState.Sturn;
                        }

                        return;
                    }

                    Debug.Log($"[{gameObject.name}] Player Hit!");

                    PlayerState.instance.TakeDamage(baseDamage);
                }
                break;
            case HitboxType.Trap:
                if (collision.CompareTag("Player"))
                {
                    Debug.Log($"[{gameObject.name}] Player Hit!");

                    PlayerState.instance.TakeDamage(baseDamage);
                }
                break;
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (type)
        {
            case HitboxType.PlayerAttack:
                if (collision.gameObject.CompareTag("Enemy"))
                {
                    Debug.Log($"[Player] {collision.gameObject.name} Hit!");
                    PlayerState.instance.AddGauge(5);
                    if (!PlayerState.instance.isGround)
                    {
                        // 공중에선 점프 초기화
                        PlayerState.instance.playerMove.JumpCountReset();
                    }
                    
                    var enemy = collision.gameObject.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
            case HitboxType.PlayerDownAttack:
                if (collision.gameObject.CompareTag("Enemy"))
                {
                    Debug.Log($"[Player] {collision.gameObject.name} Hit!");
                    PlayerState.instance.AddGauge(5);
                    if (!PlayerState.instance.isGround)
                    {
                        AttackBounce();
                        PlayerState.instance.playerMove.jumpCount = 1;
                    }

                    var enemy = collision.gameObject.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
            case HitboxType.PlayerSkill:
                if (collision.gameObject.CompareTag("Enemy"))
                {
                    Debug.Log($"[Player] {collision.gameObject.name} Hit!");
                    if (!PlayerState.instance.isGround)
                    {
                        // 공중에선 점프 초기화
                        PlayerState.instance.playerMove.JumpCountReset();
                    }                    

                    var enemy = collision.gameObject.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
            case HitboxType.Enemy:
                if (collision.gameObject.CompareTag("Player"))
                {
                    // 패링 확인
                    if (PlayerState.instance.playerGuard.IsParry())
                    {
                        // 보스 스턴
                        var boss = GetComponentInParent<BossBase>();

                        if (boss != null)
                        {
                            boss.state = BossBase.BossState.Sturn;
                        }

                        return;
                    }

                    Debug.Log($"[{gameObject.name}] Player Hit!");

                    PlayerState.instance.TakeDamage(baseDamage);
                }
                break;
            case HitboxType.Trap:
                if (collision.gameObject.CompareTag("Player"))
                {
                    Debug.Log($"[{gameObject.name}] Player Hit!");

                    PlayerState.instance.TakeDamage(baseDamage);
                }
                break;
        }
    }

    void AttackBounce()  // 플레이어를 위로 튕겨올리는 함수
    {
        Rigidbody2D rb = PlayerState.instance.GetComponent<Rigidbody2D>();
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
    }

    public void ObjectOff()
    {
        gameObject.SetActive(false);
    }
}
