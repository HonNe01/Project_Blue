using Unity.Cinemachine;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEditor.Experimental.GraphView;

public class PlayerState : MonoBehaviour
{
    public static PlayerState instance;

    // Player Reference 플레이어 참조
    [HideInInspector] public Animator anim;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Collider2D coll;
    [HideInInspector] public SpriteRenderer sprite;

    [HideInInspector] public PlayerMove playerMove;
    [HideInInspector] public PlayerAttack playerAttack;
    [HideInInspector] public PlayerGuard playerGuard;

    [HideInInspector] public CinemachinePositionComposer cinemachineCamera;

    [Header("=== Player State ===")]
    [Header("State")]
    public bool isDamaged = false;
    public bool isDie = false;

    [Header("Move")]
    public int isRight;
    public bool isGround = true;
    public bool canMove = true;
    public bool canDash = true;
    public bool canJump = true;
    [HideInInspector] public Vector2 groundCheck = new Vector2(0.5f, 0.05f);

    [Header("Attack")]
    public bool canAttack = true;
    public bool canSkill = true;

    [Header("Behavior")]
    public bool isBehavior = false;
    public bool canGuard = true;
    public bool canHeal = true;
    public bool isHeal = false;
    
    [Header("=== Health State ===")]
    public int maxHP = 5;
    private int curHP;
    public int CurHp => curHP;


    [Header("Healing Setting")]
    public float healHoldTime = 0.5f;
    private float healTimer = 0f;
    private bool healPress = false;

    [Header("Skill Gauge")]
    public int maxGauge = 100;
    public int currentGauge = 0;

    [Header("Damaged")]
    public float damagedknockbackXForce = 10f;
    public float damagedknockbackYForce = 10f;

    public int GaugePercent => (currentGauge * 100) / maxGauge;


    private void Awake()
    {
        // 인스턴스
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        // 컴포넌트 참조
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        sprite = GetComponent<SpriteRenderer>();

        // 클래스 참조
        playerMove = GetComponent<PlayerMove>();
        playerAttack = GetComponent<PlayerAttack>();
        playerGuard = GetComponent<PlayerGuard>();
    }

    private void Start()
    {
        curHP = maxHP; 

        Camera vcam = Camera.main;
        cinemachineCamera = vcam.GetComponent<CinemachinePositionComposer>();
    }


    private void Update()
    {
        if (isDie) return;

        // Ground Check
        if (rb.linearVelocityY <= 0)
        {
            isGround = Physics2D.OverlapBox(transform.position,
                                            groundCheck, 0f,
                                            LayerMask.GetMask("Ground"));
        }

        // Behavior Check
        isBehavior = isHeal || playerGuard.isGuard;

        Healing();
    }

    private void LateUpdate()
    {
        if (cinemachineCamera != null)
        {
            cinemachineCamera.Composition.ScreenPosition.x = 0.1f * isRight;
        }

        anim.SetBool("IsBehavior", isBehavior);
        anim.SetBool("IsHeal", isHeal);
    }

    private void Healing()
    {
        if (!canHeal) return;

        // 회복
        if (Input.GetKey(KeyCode.F) && curHP < maxHP)
        {
            isHeal = true;
            if (!healPress)
            {
                healTimer += Time.deltaTime;
                if (healTimer >= healHoldTime)
                {
                    Heal(1);
                    healPress = true;
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.F))
        {
            isHeal = false;
            healTimer = 0f;
            healPress = false;
        }
    }

    public void Heal(int amount = 1)
    {
        healTimer = 0;

        curHP += amount;
        curHP = Mathf.Clamp(curHP, 0, maxHP);
        Debug.Log("[PlayerState] Player Heal! CurrentHP: " + curHP);
    }

    public void TakeDamage(int damage = 1)
    {
        if (isDie) return;

        // 방어 판정
        if (playerGuard.IsGuard())
        {
            if (playerGuard.IsParry())
            {
                // 패링 성공
                playerGuard.Parry();

                return;
            }
            else
            {
                playerGuard.Guard();

                return;
            }
        }

        // 이동 불능
        Co_DisableAction(2f);

        // 피격 넉백
        if (isRight > 0)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(-damagedknockbackXForce, damagedknockbackYForce), ForceMode2D.Impulse);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(damagedknockbackXForce, damagedknockbackYForce), ForceMode2D.Impulse);
        }

        // 체력 감소
        curHP -= damage;
        curHP = Mathf.Clamp(curHP, 0, maxHP);

        // 피격 애니메이션
        anim.SetTrigger("IsDamaged");
        Debug.Log($"[PlayerState] Damaged!, Current HP : {curHP}");

        // 사망 판정
        if (curHP <= 0)
        {
            Die();
        }

    }

    private void Die()
    {
        Debug.Log("플레이어 사망!");

        // 조작 해제
        DisableAction();

        // 상태 처리
        isDie = true;
        rb.linearVelocity = Vector2.zero;   

        // 사망 애니메이션
        anim.SetTrigger("IsDie");
    }

    //skill gauge 관련
    public void AddGauge(int amount)
    {
        currentGauge += amount;
        currentGauge = Mathf.Clamp(currentGauge, 0, maxGauge);
        Debug.Log("게이지 증가");
    }

    public bool UseGauge(int amount)
    {
        if (currentGauge < amount)
        {
            Debug.Log("게이지 부족");
            return false;
        }

        currentGauge -= amount;
        return true;
    }

    private void OnDrawGizmos()
    {
        // Ground Check
        if (PlayerState.instance != null)
        {
            Gizmos.color = PlayerState.instance.isGround ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position, groundCheck);
        }
    }

    IEnumerator Co_DisableAction(float duration)
    {
        isDamaged = true;
        DisableAction();

        if (!isDie)
        {
            yield return new WaitForSeconds(duration);

            isDamaged = false;
            EnableAction();
        }
    }

    private void DisableAction()
    {
        canMove = false;
        canGuard = false;
        canHeal = false;
    }

    private void EnableAction()
    {
        canMove = true;
        canGuard = true;
        canHeal = true;
    }


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 전환시 카메라 할당
        Camera vcam = Camera.main;
        if (vcam != null)
            cinemachineCamera = vcam.GetComponent<CinemachinePositionComposer>();
    }
}


