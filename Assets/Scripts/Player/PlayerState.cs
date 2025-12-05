using System.Collections;
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

    [HideInInspector] public CinemachinePositionComposer cinemachineComposer;


    [Header("=== Player State ===")]
    [Header("State")]
    public bool isHit = false;                         // 피격 상태 판정
    private float damagedTime = 0.5f;  // 피격 후 경직 시간
    private float hitTime = 1.5f;      // 피격 후 무적 시간
    public bool isDie = false;                          // 사망 상태 판정

    [Header("Move")]
    public int isRight;
    public bool isGround = true;
    public bool canMove = true;
    public bool canDash = true;
    public bool canJump = true;
    public Vector2 groundCheck = new Vector2(0.2f, 0.05f);

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
    public GameObject hitEffect;
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
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == GameManager.instance.ruinsScene) curHP = 3;
        else curHP = maxHP;

        hitEffect.SetActive(false);

        // 메인 카메라 할당
        CinemachineCamera vcam = GameObject.Find("CinemachineCamera").GetComponent<CinemachineCamera>();
        cinemachineComposer = vcam.GetComponent<CinemachinePositionComposer>();

        vcam.Follow = transform;
    }


    private void Update()
    {
        // 사망, 연출 상태일 때 동작 중지
        if (isDie || GameManager.instance.State == GameManager.GameState.Directing)
        {
            rb.linearVelocity = Vector2.zero;

            StartCoroutine(Co_DisableAction(1));

            return;
        }
            

        // Ground Check
        if (rb.linearVelocityY <= 0.1)
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
        if (!isGround) return;

        if (Input.GetKeyUp(KeyCode.D))
        {
            Debug.Log("힐 취소");

            isHeal = false;
            ishealing = false;
            healPress = false;

            EnableAction();
            StartCoroutine(Co_DisableHeal());

            anim.SetBool("IsHeal", isHeal);
            anim.SetBool("Healing", ishealing);

            healTimer = 0f;
            healContinew = false;
            return;
        }

        if (curHP >= maxHP || currentGauge < 20) return;

        if (Input.GetKey(KeyCode.D))
        {
            isHeal = true;
            ishealing = true;
            healPress = true;

            DisableAction();

            anim.SetBool("Healing", ishealing);
            anim.SetBool("IsHeal", isHeal);

            rb.linearVelocity = Vector2.zero;
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

    private IEnumerator Co_DisableHeal()
    {
        yield return null;

        float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);
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
        if (GameManager.instance.isGod) return;
        if (isHit || isDie) return;
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
                // 이후 피격 무시
                StartCoroutine(DisableHitbox(hitTime * 0.5f));

                // 패링 성공
                playerGuard.Parry();
                if (isRight > 0)
                {
                    EffectManager.instance.PlayEffect(EffectManager.EffectType.GuardHit, transform.position);
                }
                else
                {
                    EffectManager.instance.PlayEffect(EffectManager.EffectType.GuardHit, transform.position + new Vector3(-0, 0, 0), true);
                }
                playerGuard.OffGuarded();

                yield break;
            } // 패링
            else
            {
                // 이후 피격 무시
                StartCoroutine(DisableHitbox(hitTime * 0.5f));

                // 방어 성공
                playerGuard.Guard();
                StartCoroutine(Co_DisableGuard(playerGuard.guardDisableTime));
                if (isRight > 0)
                {
                    EffectManager.instance.PlayEffect(EffectManager.EffectType.GuardHit, transform.position);
                }
                else
                {
                    EffectManager.instance.PlayEffect(EffectManager.EffectType.GuardHit, transform.position + new Vector3(-0, 0, 0), true);
                }
                
                yield break;
            }                       // 방어
        }
        // 피격 판정
        else
        {
            // 행동 불능
            StartCoroutine(Co_DisableAction(damagedTime));

            // 이후 피격 무시
            StartCoroutine(DisableHitbox(hitTime));

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
            hitEffect.SetActive(true);
            anim.SetTrigger("IsDamaged");
            SoundManager.instance.PlaySFX(SoundManager.SFX.Damaged);
            Debug.Log($"[PlayerState] Damaged!, Current HP : {curHP}");
            
            // 사망 판정
            if (curHP <= 0)
            {
                Die();
            }
        }
    }

    private IEnumerator Co_DisableGuard(float time)
    {
        playerGuard.enabled = false;
        yield return new WaitForSeconds(time);
        playerGuard.enabled = true;
    }

    IEnumerator DisableHitbox(float time)
    {
        isHit = true;
        yield return new WaitForSeconds(time);
        isHit = false;
    }

    private void Die()
    {
        Debug.Log("플레이어 사망!");

        // 상태 처리
        isDie = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        coll.enabled = false;

        // 사망 애니메이션
        anim.SetTrigger("IsDie");

        // 조작 해제
        DisableAction();
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
        if (GameManager.instance.isGod)
        {
            return true;
        }

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
        DisableAction();

        if (!isDie)
        {
            yield return new WaitForSeconds(duration);

            EnableAction();
        }
    }
    private void DisableAction()
    {
        // 조작
        playerMove.enabled = false;

        // 행동
        playerGuard.enabled = false;
        canHeal = false;
        
        // 공격
        playerAttack.enabled = false;
    }
    private void EnableAction()
    {
        // 조작
        playerMove.enabled = true;

        // 행동
        playerGuard.enabled = true;
        canHeal = true;

        // 공격
        playerAttack.enabled = true;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameManager.instance.mainMenuScene) return;

        // 씬 전환시 메인 카메라 할당
        CinemachineCamera vcam = GameObject.Find("CinemachineCamera").GetComponent<CinemachineCamera>();
        cinemachineComposer = vcam.GetComponent<CinemachinePositionComposer>();

        vcam.Follow = transform;
    }

    private void OnDrawGizmos()
    {
        // Ground Check
        Gizmos.color = isGround ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, groundCheck);
    }
}


