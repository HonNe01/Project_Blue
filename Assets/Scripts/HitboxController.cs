using UnityEngine;

public class HitboxController : MonoBehaviour
{
    public enum HitbotType { PlayerAttack, PlayerSkill,PlayerJumpAttack,Trap, Enemy, Scarecrow}

    [SerializeField] private int baseDamage = 1;
    [SerializeField] private HitbotType type;

    private float bounceForce = 10f;


    void Awake()
    {
        if (type == HitbotType.Trap)
        {
            gameObject.SetActive(true);
        }

        else if (type != HitbotType.Scarecrow)
        {
            gameObject.SetActive(false);
        }
  
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (type)
        {
            case HitbotType.PlayerAttack:
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
            case HitbotType.PlayerJumpAttack:
                if (collision.CompareTag("Enemy"))
                {
                    Debug.Log($"[Player] {collision.gameObject.name} Hit!");
                    PlayerState.instance.AddGauge(5);
                    if (!PlayerState.instance.isGround)
                    {
                        AttackBounce();
                        PlayerState.instance.playerMove.JumpCountReset();
                    }

                    var enemy = collision.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
            case HitbotType.PlayerSkill:
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
            case HitbotType.Enemy:
                if (collision.CompareTag("Player"))
                {
                    Debug.Log($"[{gameObject.name}] Player Hit!");

                    PlayerState.instance.TakeDamage(baseDamage);
                } 
                break;
            case HitbotType.Trap:
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
            case HitbotType.PlayerAttack:

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
            case HitbotType.PlayerJumpAttack:

                if (collision.gameObject.CompareTag("Enemy"))
                {
                    Debug.Log($"[Player] {collision.gameObject.name} Hit!");
                    PlayerState.instance.AddGauge(5);
                    if (!PlayerState.instance.isGround)
                    {
                        AttackBounce();
                        PlayerState.instance.playerMove.JumpCountReset();
                    }

                    var enemy = collision.gameObject.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
            case HitbotType.PlayerSkill:
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
            case HitbotType.Enemy:
                if (collision.gameObject.CompareTag("Player"))
                {
                    Debug.Log($"[{gameObject.name}] Player Hit!");

                    PlayerState.instance.TakeDamage(baseDamage);
                }
                break;
            case HitbotType.Trap:
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
