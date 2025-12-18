using System.Collections;
using UnityEngine;

public class TestPlayerAttack : MonoBehaviour
{
    [Header(" === Default Attack === ")]
    [Header("Attack Setting")]
    public int maxCombo = 3;
    public int curCombo = 0;
    public float comboTime = 1.5f;            // �޺� ���� �ð�
    private float lastAttackTime = -1f;         // ������ ���� ���� �ð�
    [SerializeField ]public bool isAttack = false;
    [SerializeField] private bool comboQueue = false;        // �޺� �Է� ��� ������ ����
    private bool isCharge = false;  

    [Header("ChargeAttack")]
    private float AttackTimer = 0f;
    private float AttackHoldTime = 2f;


    [Header("Skill")]
    private float skillTimer = 0f;
    private float skillHoldTime = 0.3f;
    private bool isSkillCharge = false;

    [Header("Test")]
    private bool stop = false;

    // ����
    [HideInInspector] public Animator anim;
    [HideInInspector] public Rigidbody2D rb;

    public void Start()
    {
        anim = TestPlayerState.instance.anim;
        rb = TestPlayerState.instance.rb;
    }

    protected virtual void Update()
    {
        if (TestPlayerState.instance.isDie && GameManager.instance.State == GameManager.GameState.Directing) return;

        // ����
        Attack();

        // ��ų
        if (Input.GetKeyDown(KeyCode.F) && TestPlayerState.instance.canAttack)
        {
            if (!TestPlayerState.instance.isGround)         // ����
            {
                if (Input.GetKey(KeyCode.UpArrow))          // �� ��ų
                {
                    Skill_Up();
                }
                else if (Input.GetKey(KeyCode.DownArrow))   // �Ʒ� ��ų
                {
                    Skill_Down();
                }
                else
                {
                    Skill();
                }
            }
            else                                        // ����
            {
                if (Input.GetKey(KeyCode.UpArrow))          // �� ��ų
                {
                    Skill_Up();
                }
                else
                {
                    Skill();
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (stop)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void Attack()
    {
        // �޺� �ð� �ʰ� -> �ʱ�ȭ
        if (Time.time - lastAttackTime > comboTime)
        {
            ResetCombo();
        }
        // ���� ����
        if (Input.GetKeyDown(KeyCode.V) && TestPlayerState.instance.canAttack)
        {
            if (!TestPlayerState.instance.isGround)         // ����
            {
                if (Input.GetKey(KeyCode.UpArrow))          // �� ����
                {
                    StartCoroutine(Co_UpAttack(TestPlayerState.instance.isGround));
                }
                else if (Input.GetKey(KeyCode.DownArrow))   // �Ʒ� ����
                {
                    StartCoroutine(Co_DownAttack());
                }
                else
                {
                    StartCoroutine(Co_JumpAttack());        // ���� ����
                }
            }
            else if (!isAttack && curCombo < maxCombo)  // ����
            {
                if (Input.GetKey(KeyCode.UpArrow))          // �� ����
                {
                    StartCoroutine(Co_UpAttack(TestPlayerState.instance.isGround));
                }
                else
                {
                    if (!comboQueue)
                    {
                        curCombo++;
                        comboQueue = true;
                    }

                    StartCoroutine(Co_Attack());// �Ϲ� ����
                }
            }
        }
    }

    public virtual IEnumerator Co_Attack()
    {
        if(isAttack || curCombo == 0) yield break;
        isAttack = true;
        AttackTimer = 0f;
        
        while (Input.GetKey(KeyCode.V))
        {
            AttackTimer += Time.deltaTime;

            if (AttackTimer >= AttackHoldTime * 0.3f && !isCharge)
            {
                OnCharging();
            }

            if (AttackTimer >= AttackHoldTime && !isCharge)
            {
                OnCharge();
                OffCharging();
                isCharge = true;
                
                Debug.Log("�������� �غ�");
            }
            yield return null;
        }
        
        // ���� �Ϸ�
        if (AttackTimer >= AttackHoldTime)
        {
            OffCharge();
            anim.SetTrigger("Attack");
            anim.SetTrigger("ChargeAttack");

            Debug.Log("�������� ����");
        }
        // ���� ���
        else if (AttackTimer >= AttackHoldTime * 0.3f && AttackTimer <= AttackHoldTime)
        {
            OffCharging();

            yield break;
        }
        // �Ϲ� ����
        else
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackCombo", curCombo);
            stop = true;
        }
        stop = true;

        yield return null;
        yield return new WaitForEndOfFrame();

        // ���� �� ����
        float attackTime = anim.GetCurrentAnimatorStateInfo(0).length * 0.7f;
        float timer = 0f;

        while (attackTime > timer)
        {
            DisableOtherAction();
            timer += Time.deltaTime;
            yield return null;
        }

        // ���� ����
        EnableOtherAction();
        isCharge = false;
        AttackTimer = 0f;
        stop = false;

        // �޺� �Ϸ� -> �ʱ�ȭ
        if (curCombo >= maxCombo)
        {
            ResetCombo();
        }
        else
        {
            lastAttackTime = Time.time;
        }
    }

    private void AddCombo()
    {
        isAttack = false;
        comboQueue = false;
        lastAttackTime = Time.time;
    }

    private void ResetCombo()
    {
        isAttack = false;
        comboQueue = false;
        curCombo = 0;
        anim.SetInteger("AttackCombo", 0);
    }
    public virtual void Attack1Start() 
    {
        AddCombo();
        EffectManager.instance.PlayEffect(EffectManager.EffectType.Attack1, transform.position, TestPlayerState.instance.isRight < 0);
    }   
    public virtual void Attack2Start()
    {
        AddCombo();
        EffectManager.instance.PlayEffect(EffectManager.EffectType.Attack2, transform.position, TestPlayerState.instance.isRight < 0);
    }
    public virtual void Attack3Start()
    {
        AddCombo();
        EffectManager.instance.PlayEffect(EffectManager.EffectType.Attack3, transform.position, TestPlayerState.instance.isRight < 0);
    }
    public virtual void ChargeAttackStart()
    {
        EffectManager.instance.PlayEffect(EffectManager.EffectType.ChargeAttack, transform.position, TestPlayerState.instance.isRight < 0);
    }
    public void ChargeAttackEnd()
    {
        isAttack = false;
        comboQueue = false;
    }
    public virtual void OnCharging()
    {

    }
    public virtual void OffCharging()
    {

    }
    public virtual void OnCharge()
    {

    }
    public virtual void OffCharge()
    {

    }

    public virtual void JumpAttackStart()
    {
        EffectManager.instance.PlayEffect(EffectManager.EffectType.JumpAttack, transform.position, TestPlayerState.instance.isRight < 0);
        isAttack = false;
    }

    private IEnumerator Co_JumpAttack()                 // ���� ����
    {
        anim.SetTrigger("Attack");
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator Co_UpAttack(bool isGround)    // �� ����
    {
        anim.SetTrigger("IsUp");
        anim.SetTrigger("Attack");
        yield return new WaitForEndOfFrame();
    }
    public void UpAttackStart() 
    { 
        EffectManager.instance.PlayEffect(EffectManager.EffectType.UpAttack, transform.position, TestPlayerState.instance.isRight < 0);
    }

    private IEnumerator Co_DownAttack()                 // �Ʒ� ����
    {
        anim.SetTrigger("IsDown");
        anim.SetTrigger("Attack");
        yield return new WaitForEndOfFrame();
    }
    public void DownAttackStart() 
    { 
        EffectManager.instance.PlayEffect(EffectManager.EffectType.DownAttack, transform.position, TestPlayerState.instance.isRight < 0);
    }

    public virtual void Skill() // AttackSkill = 1
    {
        if (Input.GetKey(KeyCode.F))
        {
            skillTimer += Time.deltaTime;
            
            
            if (Input.GetKey(KeyCode.F))
            {
                skillTimer += Time.deltaTime;
                if (skillTimer >= skillHoldTime && !isSkillCharge)
                {
                    isSkillCharge = true;
                    Debug.Log("�� �غ�");
                }
            }

            if (isSkillCharge)
            {
                TestPlayerState.instance.Healing();
                Debug.Log("�� ���");
            }
            else if (!isSkillCharge && Input.GetKeyUp(KeyCode.F))
            {
                anim.SetTrigger("Attack");
                anim.SetInteger("AttackSkill", 1);
                Debug.Log("�Ϲݽ�ų ���");
            }
            rb.linearVelocity = Vector2.zero;
            skillTimer = 0f;
            isSkillCharge = false;
        }
    }

    public virtual void SkillStart()
    {
    }
    public virtual void SkillEnd()
    {

    }

    public virtual void Skill_Up() // AttackSkill = 2
    {
        if (TestPlayerState.instance.UseGauge(20))
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 2);

            Debug.Log("[TestPlayerAttack] ����ų ���");
        }
    }
    public virtual void UpSkillStart()
    {

    }
    public virtual void UpSkillEnd()
    {

    }

    public virtual void Skill_Down() // AttackSkill = 3
    {
        if (TestPlayerState.instance.UseGauge(20))
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 3);

            Debug.Log("[TestPlayerAttack] �Ʒ���ų ���");
        }
    }
    public virtual void DownSkillStart()
    {

    }
    public virtual void DownSkillEnd()
    {

    }

    
    public void EnableOtherAction()
    {
        TestPlayerState.instance.canMove = true;
        TestPlayerState.instance.canHeal = true;
        TestPlayerState.instance.canGuard = true;
    }
    public void DisableOtherAction()
    {
        TestPlayerState.instance.canMove = false;
        TestPlayerState.instance.canHeal = false;
        TestPlayerState.instance.canGuard = false;
    }
}


