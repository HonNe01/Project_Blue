using UnityEngine;

public class HitboxController : MonoBehaviour
{
    public enum HitboxType { PlayerAttack, PlayerDownAttack, PlayerSkill, Trap, Enemy, EnemyAttack, Scarecrow }
    public enum EffectType { Slash, Explosion }

    public enum ActiveType { True, False }


    [SerializeField] private int baseDamage = 1;
    [SerializeField] private HitboxType type;
    [SerializeField] private ActiveType actType;
    

    private float bounceForce = 10f;
    private Collider2D coll;
    private Vector2 hitPos;


    void Awake()
    {
        if (actType == ActiveType.False)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }

        coll = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        coll.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Hit(collision.gameObject);

        switch (type)
        {
            case HitboxType.PlayerAttack:
                if (collision.CompareTag("Enemy"))
                {
                    Debug.Log($"[Player] {collision.gameObject.name} Hit!");

                    // 피격 이펙트
                    GetEffectPos(collision, out hitPos);
                    EffectManager.instance.PlayEffect(EffectManager.EffectType.AttackHit, hitPos, PlayerState.instance.isRight < 0);

                    // 공중에선 점프 초기화
                    if (!PlayerState.instance.isGround)
                    {
                        PlayerState.instance.playerMove.JumpCountReset();
                    }

                    // 피격 처리
                    PlayerState.instance.AddGauge(5);
                    var enemy = collision.GetComponent<BossBase>();
                    if (enemy != null)
                    {
                        coll.enabled = false;
                        enemy.TakeDamage(baseDamage);
                        SoundManager.instance.PlaySFX(SoundManager.SFX.Attack_Hit);
                    }
                }
                break;
            case HitboxType.PlayerDownAttack:
                if (collision.CompareTag("Enemy"))
                {
                    Debug.Log($"[Player] {collision.gameObject.name} Hit!");
                    
                    // 피격 이펙트
                    GetEffectPos(collision, out hitPos);
                    EffectManager.instance.PlayEffect(EffectManager.EffectType.AttackHit, hitPos, PlayerState.instance.isRight < 0);

                    // 공중에선 점프 초기화 및 튕겨올리기
                    if (!PlayerState.instance.isGround)
                    {
                        AttackBounce();
                        PlayerState.instance.playerMove.jumpCount = 1;
                        Debug.Log("튀어오르기");
                    }

                    // 피격 처리
                    PlayerState.instance.AddGauge(5);
                    var enemy = collision.GetComponent<BossBase>();

                    if (enemy != null)
                    {
                        coll.enabled = false;
                        enemy.TakeDamage(baseDamage);
                        SoundManager.instance.PlaySFX(SoundManager.SFX.Attack_Hit);
                    }
                }
                break;
            case HitboxType.PlayerSkill:
                if (collision.CompareTag("Enemy"))
                {
                    Debug.Log($"[Player] {collision.gameObject.name} Hit!");

                    // 피격 이펙트
                    GetEffectPos(collision, out hitPos);
                    EffectManager.instance.PlayEffect(EffectManager.EffectType.SkillHit, hitPos, PlayerState.instance.isRight < 0);

                    // 공중에선 점프 초기화
                    if (!PlayerState.instance.isGround)
                    {
                        PlayerState.instance.playerMove.JumpCountReset();
                    }

                    // 피격 처리
                    var enemy = collision.GetComponent<BossBase>();
                    if (enemy != null)
                    {
                        coll.enabled = false;
                        enemy.TakeDamage(baseDamage);
                    }
                }
                break;
            case HitboxType.Enemy:
                if (collision.CompareTag("Player"))
                {
                    Debug.Log($"[{gameObject.name}] Player Hit!");

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

                    // 피격 처리
                    PlayerState.instance.TakeDamage(baseDamage);
                }
                break;
            case HitboxType.EnemyAttack:
                if (collision.CompareTag("Player"))
                {
                    Debug.Log($"[{gameObject.name}] Player Hit!");

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

                    // 피격 처리
                    coll.enabled = false;
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
                    EffectManager.instance.PlayEffect(EffectManager.EffectType.AttackHit, transform.position, PlayerState.instance.isRight < 0);
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
                    EffectManager.instance.PlayEffect(EffectManager.EffectType.AttackHit, transform.position, PlayerState.instance.isRight < 0);
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
                    EffectManager.instance.PlayEffect(EffectManager.EffectType.SkillHit, transform.position, PlayerState.instance.isRight < 0);
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

    private void Hit(GameObject target)
    {
        switch(type)
        {
            case HitboxType.PlayerAttack:
                break;
            case HitboxType.PlayerDownAttack:
                break;
            case HitboxType.PlayerSkill:
                break;
            case HitboxType.Enemy:
            case HitboxType.EnemyAttack:
                break;
            case HitboxType.Trap:
                break;
        }
    }

    private void HitPlayerAttack(GameObject target)
    {

    }


    private bool GetEffectPos(Collider2D b, out Vector2 hitPos)
    {
        hitPos = Vector2.zero;

        Bounds A = coll.bounds;
        Bounds B = b.bounds;

        // AABB 충돌 체크
        if (!A.Intersects(B)) return false;

        // 충돌 영역 계산
        Vector3 min = Vector3.Max(A.min, B.min);
        Vector3 max = Vector3.Min(A.max, B.max);

        // 중심
        Vector3 center = (min + max) / 2f;
        hitPos = new Vector2(center.x, center.y);

        return true;
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
