using Unity.Cinemachine;
using UnityEngine;
using System.Collections;
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

    [HideInInspector] public CinemachinePositionComposer cinemachineComposer;

    [Header("=== Player State ===")]
    [Header("State")]
    public bool isDamaged = false;
    [SerializeField] private float damagedTime = 0.2f;
    private bool isHit = false;
    [SerializeField] private float hitTime = 0.5f;
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
    public bool ishealing = false;

    [Header("=== Health State ===")]
    public int maxHP = 5;
    private int curHP;
    public int CurHp => curHP;


    [Header("Healing Setting")]
    public float healHoldTime = 1f;
    private float healTimer = 0f;
    private bool healPress = false;
    private bool healContinew = false;

    [Header("Skill Gauge")]
    public int maxGauge = 100;
    public int currentGauge = 100;

    [Header("Damaged")]
    public float damagedknockbackXForce = 10f;
    public float damagedknockbackYForce = 10f;

    public int GaugePercent => (currentGauge * 100) / maxGauge;


    private void Awake()
    {
        // 인스턴스
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
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

        CinemachineCamera vcam = GameObject.Find("CinemachineCamera").GetComponent<CinemachineCamera>();
        cinemachineComposer = vcam.GetComponent<CinemachinePositionComposer>();

        vcam.Follow = transform;
    }


    private void Update()
    {
        if (isDie || GameManager.instance.State == GameManager.GameState.Directing) return;

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
        if (cinemachineComposer != null)
        {
            var comp = cinemachineComposer.Composition.ScreenPosition;
            float screenX = comp.x;
            comp.x *= screenX * -isRight;

            cinemachineComposer.Composition.ScreenPosition = comp;
        }

        anim.SetBool("IsBehavior", isBehavior);

    }

    public void Healing()
    {
        if (!canHeal || curHP >= maxHP || currentGauge < 20)
        {
            return;
        }

        if (Input.GetKey(KeyCode.F))
        {
            isHeal = true;
            ishealing = true;
            healPress = true;
            anim.SetBool("Healing", ishealing);
            anim.SetBool("IsHeal", isHeal);
            canMove = false;
            rb.linearVelocity = Vector2.zero;
        }
        else if (Input.GetKeyUp(KeyCode.F))
        {
            isHeal = false;
            ishealing = false;
            healPress = false;
            anim.SetBool("IsHeal", isHeal);
            anim.SetBool("Healing", ishealing);
            healTimer = 0f;
            healContinew = false;
            StartCoroutine(DisableHeal());
            canMove = true;            
        }

        if (healPress)
        {
            healTimer += Time.deltaTime;

            // 첫 힐 (healHoldTime = 1초)
            if (!healContinew && healTimer >= healHoldTime)
            {
                Heal(1);
                isHeal = false;
                healTimer = 0f;
                healContinew = true; 
            }
            // 이후 반복 힐 (0.5초 간격)
            else if (healContinew && healTimer >= 0.5f)
            {
                Heal(1);
                isHeal = false;
                healTimer = 0f;
            }
        }
    }

    public void Heal(int amount = 1)
    {
        if (!UseGauge(20))
        {
            isHeal = false;
            ishealing = false;
            anim.SetBool("IsHeal", isHeal);
            anim.SetBool("Healing", ishealing);
        }
        else
        {
            healPress = false;
            healTimer = 0;
            curHP += amount;
            curHP = Mathf.Clamp(curHP, 0, maxHP);
            Debug.Log("[PlayerState] Player Heal! CurrentHP: " + curHP);
        }
    }

    public void HealSound()
    {
        SoundManager.instance.PlaySFX(SoundManager.SFX.Healing);
    }

    private IEnumerator DisableHeal()
    {
        yield return new WaitForSeconds(0.5f);
    }

    public void HPCheck()
    {
        if (!canHeal || curHP >= maxHP)
        {
            isHeal = false;
            ishealing = false;
            anim.SetBool("IsHeal", isHeal);
            anim.SetBool("Healing", ishealing);
        }
    }

    public void GaugeCheck()
    {
        if (currentGauge < 20)
        {
            isHeal = false;
            ishealing = false;
            healPress = false;
            healTimer = 0;
            anim.SetBool("IsHeal", isHeal);
            anim.SetBool("Healing", ishealing);
        }
    }
    
    public void TakeDamage(int damage = 1)
    {
        if (isHit ||isDie) return;
        isHit = true;

        StartCoroutine(Co_TakeDamage(damage));
    }

    private IEnumerator Co_TakeDamage(int damage)
    {
        yield return null;

        // 방어 판정
        if (playerGuard.IsGuard())
        {
            if (playerGuard.IsParry())
            {
                // 패링 성공
                isHit = false;
                playerGuard.Parry();
                playerGuard.OffGuarded();

                yield break;
            }
            else
            {
                // 방어 성공
                isHit = false;
                playerGuard.Guard();
                playerGuard.OffGuarded();

                yield break;
            }
        }
        // 피격 판정
        else
        {
            // 이동 불능
            StartCoroutine(Co_DisableAction(damagedTime));
            // 이미 피격 상태면 무시
            StartCoroutine(DisableHitbox());

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
    }

    IEnumerator DisableHitbox()
    {
        isHit = true;
        yield return new WaitForSeconds(hitTime);
        isHit = false;
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

    // skill gauge 관련
    public void AddGauge(int amount)    // 게이지 회복
    {
        currentGauge += amount;
        currentGauge = Mathf.Clamp(currentGauge, 0, maxGauge);
        Debug.Log("게이지 증가");
    }

    public bool UseGauge(int amount)    // 게이지 소모
    {
        if (currentGauge < amount)
        {
            Debug.Log("게이지 부족");
            return false;
        }

        currentGauge -= amount;
        return true;
    }

    IEnumerator Co_DisableAction(float duration)    // 피격시 행동 불능
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
        // 조작
        canMove = false;
        canJump = false;
        canDash = false;

        // 행동
        canGuard = false;
        canHeal = false;
        
        // 공격
        canAttack = false;
        canSkill = false;
    }

    private void EnableAction()
    {
        // 조작
        canMove = true;
        canJump = true;
        canDash = true;

        // 행동
        canGuard = true;
        canHeal = true;

        // 공격
        canAttack = true;
        canSkill = true;
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
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == GameManager.instance.mainMenuScene) Destroy(gameObject);

        // 씬 전환시 카메라 할당
        CinemachineCamera vcam = GameObject.Find("CinemachineCamera").GetComponent<CinemachineCamera>();
        cinemachineComposer = vcam.GetComponent<CinemachinePositionComposer>();

        vcam.Follow = transform;
    }

    private void OnDrawGizmos()
    {
        // Ground Check
        if (instance != null)
        {
            Gizmos.color = isGround ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position, groundCheck);
        }
    }
}


