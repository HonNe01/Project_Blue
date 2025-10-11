using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        curHP += amount;
        curHP = Mathf.Clamp(curHP, 0, maxHP);
        Debug.Log("[PlayerState] Player Heal! CurrentHP: " + curHP);
    }

    public void TakeDamage(int damage = 1)
    {
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

        curHP -= damage;
        curHP = Mathf.Clamp(curHP, 0, maxHP);

        Debug.Log("[PlayerState] Damaged!");
        anim.SetTrigger("IsDamaged");

        if (curHP <= 0) Die();
    }

    private void Die()
    {
        canMove = false;

        Debug.Log("플레이어 사망!");
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
        Debug.Log("게이지 사용");
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


