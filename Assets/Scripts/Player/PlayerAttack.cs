using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerAttack : MonoBehaviour
{
    [Header(" === Default Attack === ")]
    [Header("Attack Setting")]
    public int maxCombo = 3;
    public int curCombo = 0;
    public float comboTime = 1.5f;            // 콤보 유지 시간
    private float lastAttackTime = -1f;         // 마지막 공격 시작 시간
    [SerializeField ]public bool isAttack = false;
    [SerializeField] private bool comboQueue = false;        // 콤보 입력 대기 중인지 여부
    private bool isCharge = false;  

    [Header("ChargeAttack")]
    private float AttackTimer = 0f;
    private float AttackHoldTime = 2f;


    [Header("Skill")]
    private float skillTimer = 0f;
    private float skillHoldTime = 0.3f;
    private bool isSkillCharge = false;

    // 참조
    [HideInInspector] public Animator anim;
    [HideInInspector] public Rigidbody2D rb;

    public void Start()
    {
        anim = PlayerState.instance.anim;
        rb = PlayerState.instance.rb;
    }

    protected virtual void Update()
    {
        if (PlayerState.instance.isDie && GameManager.instance.State == GameManager.GameState.Directing) return;

        // 공격
        Attack();

        // 스킬
        if (Input.GetKeyDown(KeyCode.F) && PlayerState.instance.canAttack)
        {
            if (!PlayerState.instance.isGround)         // 공중
            {
                if (Input.GetKey(KeyCode.UpArrow))          // 윗 스킬
                {
                    Skill_Up();
                }
                else if (Input.GetKey(KeyCode.DownArrow))   // 아래 스킬
                {
                    Skill_Down();
                }
                else
                {
                    Skill();
                }
            }
            else                                        // 지상
            {
                if (Input.GetKey(KeyCode.UpArrow))          // 윗 스킬
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

    public void Attack()
    {
        // 콤보 시간 초과 -> 초기화
        if (Time.time - lastAttackTime > comboTime)
        {
            ResetCombo();
        }
        // 공격 실행
        if (Input.GetKeyDown(KeyCode.V) && PlayerState.instance.canAttack)
        {
            if (!PlayerState.instance.isGround)         // 공중
            {
                if (Input.GetKey(KeyCode.UpArrow))          // 윗 공격
                {
                    StartCoroutine(Co_UpAttack(PlayerState.instance.isGround));
                    Debug.Log("Up Attack");
                }
                else if (Input.GetKey(KeyCode.DownArrow))   // 아래 공격
                {
                    StartCoroutine(Co_DownAttack());
                    Debug.Log("Down Attack");
                }
                else
                {
                    StartCoroutine(Co_JumpAttack());        // 점프 공격
                }
            }
            else if (!isAttack && curCombo < maxCombo)  // 지상
            {
                if (Input.GetKey(KeyCode.UpArrow))          // 윗 공격
                {
                    StartCoroutine(Co_UpAttack(PlayerState.instance.isGround));
                    Debug.Log("Up Attack");
                }
                else
                {
                    if (!comboQueue)
                    {
                        curCombo++;
                        comboQueue = true;
                    }

                    StartCoroutine(Co_Attack());// 일반 공격
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
                
                Debug.Log("차지공격 준비");
            }
            yield return null;
        }
        
        // 차지 완료
        if (AttackTimer >= AttackHoldTime)
        {
            OffCharge();
            anim.SetTrigger("Attack");
            anim.SetTrigger("ChargeAttack");

            Debug.Log("차지공격 실행");
        }
        // 차지 취소
        else if (AttackTimer >= AttackHoldTime * 0.3f && AttackTimer <= AttackHoldTime)
        {
            OffCharging();

            yield break;
        }
        // 일반 공격
        else
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackCombo", curCombo);
            rb.linearVelocity = Vector2.zero;
            Debug.Log($"콤보{curCombo} 실행");
        }
        rb.linearVelocity = Vector2.zero;

        yield return null;
        yield return new WaitForEndOfFrame();

        // 공격 중 멈춤
        float attackTime = anim.GetCurrentAnimatorStateInfo(0).length * 0.7f;
        float timer = 0f;

        while (attackTime > timer)
        {
            DisableOtherAction();
            timer += Time.deltaTime;
            yield return null;
        }

        // 멈춤 해제
        EnableOtherAction();
        isCharge = false;
        AttackTimer = 0f;

        // 콤보 완료 -> 초기화
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
        EffectManager.instance.PlayEffect(EffectManager.EffectType.Attack1, transform.position, PlayerState.instance.isRight < 0);
    }   
    public virtual void Attack2Start()
    {
        AddCombo();
        EffectManager.instance.PlayEffect(EffectManager.EffectType.Attack2, transform.position, PlayerState.instance.isRight < 0);
    }
    public virtual void Attack3Start()
    {
        AddCombo();
        EffectManager.instance.PlayEffect(EffectManager.EffectType.Attack3, transform.position, PlayerState.instance.isRight < 0);
    }
    public virtual void ChargeAttackStart()
    {
        EffectManager.instance.PlayEffect(EffectManager.EffectType.ChargeAttack, transform.position, PlayerState.instance.isRight < 0);
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
        EffectManager.instance.PlayEffect(EffectManager.EffectType.JumpAttack, transform.position, PlayerState.instance.isRight < 0);
        isAttack = false;
    }

    private IEnumerator Co_JumpAttack()                 // 점프 공격
    {
        anim.SetTrigger("Attack");
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator Co_UpAttack(bool isGround)    // 윗 공격
    {
        anim.SetTrigger("IsUp");
        anim.SetTrigger("Attack");
        yield return new WaitForEndOfFrame();
    }
    public void UpAttackStart() 
    { 
        EffectManager.instance.PlayEffect(EffectManager.EffectType.UpAttack, transform.position, PlayerState.instance.isRight < 0);
    }

    private IEnumerator Co_DownAttack()                 // 아래 공격
    {
        Debug.Log("Down Attack Animation");
        anim.SetTrigger("IsDown");
        anim.SetTrigger("Attack");
        yield return new WaitForEndOfFrame();
    }
    public void DownAttackStart() 
    { 
        EffectManager.instance.PlayEffect(EffectManager.EffectType.DownAttack, transform.position, PlayerState.instance.isRight < 0);
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
                    Debug.Log("힐 준비");
                }
            }

            if (isSkillCharge)
            {
                PlayerState.instance.Healing();
                Debug.Log("힐 사용");
            }
            else if (!isSkillCharge && Input.GetKeyUp(KeyCode.F))
            {
                anim.SetTrigger("Attack");
                anim.SetInteger("AttackSkill", 1);
                Debug.Log("일반스킬 사용");
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
        if (PlayerState.instance.UseGauge(20))
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 2);

            Debug.Log("[PlayerAttack] 윗스킬 사용");
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
        if (PlayerState.instance.UseGauge(20))
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 3);

            Debug.Log("[PlayerAttack] 아랫스킬 사용");
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
        PlayerState.instance.canMove = true;
        PlayerState.instance.canHeal = true;
        PlayerState.instance.canGuard = true;
    }
    public void DisableOtherAction()
    {
        PlayerState.instance.canMove = false;
        PlayerState.instance.canHeal = false;
        PlayerState.instance.canGuard = false;
    }
}


