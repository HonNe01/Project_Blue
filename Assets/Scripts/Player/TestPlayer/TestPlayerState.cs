using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestPlayerState : MonoBehaviour
{
    public static TestPlayerState instance;

    // Player Reference �÷��̾� ����
    [HideInInspector] public Animator anim;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Collider2D coll;
    [HideInInspector] public SpriteRenderer sprite;

    [HideInInspector] public TestPlayerMove testplayerMove;
    [HideInInspector] public TestPlayerAttack testplayerAttack;
    [HideInInspector] public PlayerGuard playerGuard;

    [HideInInspector] public CinemachinePositionComposer cinemachineComposer;


    [Header("=== Player State ===")]
    [Header("State")]
    public bool isHit = false;                         // �ǰ� ���� ����
    private float damagedTime = 0.5f;  // �ǰ� �� ���� �ð�
    private float hitTime = 1.5f;      // �ǰ� �� ���� �ð�
    public bool isDie = false;                          // ��� ���� ����

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
        // �ν��Ͻ�
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        // ������Ʈ ����
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        sprite = GetComponent<SpriteRenderer>();

        // Ŭ���� ����
        testplayerMove = GetComponent<TestPlayerMove>();
        testplayerAttack = GetComponent<TestPlayerAttack>();
        playerGuard = GetComponent<PlayerGuard>();
    }

    private void Start()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == GameManager.instance.ruinsScene) curHP = 3;
        else curHP = maxHP;

        hitEffect.SetActive(false);

        // ���� ī�޶� �Ҵ�
        CinemachineCamera vcam = GameObject.Find("CinemachineCamera").GetComponent<CinemachineCamera>();
        cinemachineComposer = vcam.GetComponent<CinemachinePositionComposer>();

        vcam.Follow = transform;
    }


    private void Update()
    {
        // ���, ���� ������ �� ���� ����
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
            Debug.Log("�� ���");

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

            // ù �� (healHoldTime = 1��)
            if (!healContinew && healTimer >= healHoldTime)
            {
                Heal(1);
                isHeal = false;
                healTimer = 0f;
                healContinew = true;
            }
            // ���� �ݺ� �� (0.5�� ����)
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

        // ��� ����
        if (playerGuard.IsGuard())
        {
            if (playerGuard.IsParry())
            {
                // ���� �ǰ� ����
                StartCoroutine(DisableHitbox(hitTime * 0.5f));

                // �и� ����
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
            } // �и�
            else
            {
                // ���� �ǰ� ����
                StartCoroutine(DisableHitbox(hitTime * 0.5f));

                // ��� ����
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
            }                       // ���
        }
        // �ǰ� ����
        else
        {
            // �ൿ �Ҵ�
            StartCoroutine(Co_DisableAction(damagedTime));

            // ���� �ǰ� ����
            StartCoroutine(DisableHitbox(hitTime));

            // �ǰ� �˹�
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

            // ü�� ����
            curHP -= damage;
            curHP = Mathf.Clamp(curHP, 0, maxHP);

            // �ǰ� �ִϸ��̼�
            hitEffect.SetActive(true);
            anim.SetTrigger("IsDamaged");
            SoundManager.instance.PlaySFX(SoundManager.SFX.Damaged);
            Debug.Log($"[PlayerState] Damaged!, Current HP : {curHP}");
            
            // ��� ����
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
        Debug.Log("�÷��̾� ���!");

        // ���� ó��
        isDie = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        coll.enabled = false;

        // ��� �ִϸ��̼�
        anim.SetTrigger("IsDie");

        // ���� ����
        DisableAction();
    }

    // skill gauge ����
    public void AddGauge(int amount)    // ������ ȸ��
    {
        currentGauge += amount;
        currentGauge = Mathf.Clamp(currentGauge, 0, maxGauge);
        Debug.Log("������ ����");
    }
    public bool UseGauge(int amount)    // ������ �Ҹ�
    {
        if (GameManager.instance.isGod)
        {
            return true;
        }

        if (currentGauge < amount)
        {
            Debug.Log("������ ����");
            return false;
        }

        currentGauge -= amount;
        return true;
    }

    IEnumerator Co_DisableAction(float duration)    // �ǰݽ� �ൿ �Ҵ�
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
        // ����
        testplayerMove.enabled = false;

        // �ൿ
        playerGuard.enabled = false;
        canHeal = false;
        
        // ����
        testplayerAttack.enabled = false;
    }
    private void EnableAction()
    {
        // ����
        testplayerMove.enabled = true;

        // �ൿ
        playerGuard.enabled = true;
        canHeal = true;

        // ����
        testplayerAttack.enabled = true;
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

        // �� ��ȯ�� ���� ī�޶� �Ҵ�
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


