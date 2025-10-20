using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header(" === Default Attack === ")]
    [Header("Attack Setting")]
    public int maxCombo = 3;
    public int curCombo = 0;
    public float comboTime = 1.5f;            // 콤보 유지 시간
    private float lastAttackTime = -1f;         // 마지막 공격 시작 시간
    private bool isAttack = false;
    private bool comboQueue = false;        // 콤보 입력 대기 중인지 여부


    public GameObject _attack1;
    public GameObject _attack2;
    public GameObject _attack3;
    public GameObject _chargeAttack;
    public GameObject _jumpAttack;

    [Header("High Attack")]
    public GameObject _upAttack;

    [Header("Down Attack")]
    public GameObject _downattack;
    

    [Header("ChargeAttack")]
    private bool AttackPress = false;
    private float AttackTimer = 0f;
    private float AttackHoldTime = 0.5f;

    // 참조
    [HideInInspector] public Animator anim;
    [HideInInspector] public Rigidbody2D rb;

    public void Start()
    {
        anim = PlayerState.instance.anim;
        rb = PlayerState.instance.rb;
    }

    private void Update()
    {
        // 공격
        Attack();
        
        // 아래스킬
        if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.F) && !PlayerState.instance.isGround)
        {
            Skill_Down();
            return;
        }
        // 윗스킬
        else if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.F))
        {
            Skill_Up();
            return;

        }
        // 일반스킬
        else if (Input.GetKeyDown(KeyCode.F))
        {
            Skill();
            return;
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

    private IEnumerator Co_Attack()
    {
        if(isAttack && curCombo == 0) yield break;
            isAttack = true;
            AttackTimer = 0f;
        


        while (Input.GetKey(KeyCode.V))
        {
            AttackTimer += Time.deltaTime;
            yield return null;
        }
        


        if (AttackTimer >= AttackHoldTime)
        {
            anim.SetTrigger("Attack");
            anim.SetTrigger("ChargeAttack");
            Debug.Log("강공격 준비 완료");
        }
        else 
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackCombo", curCombo);
            Debug.Log("일반공격 실행");
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
                isAttack = false;
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
        comboQueue = false;
        lastAttackTime = Time.time;
        Debug.Log($"콤보 증가 {curCombo}");
    }

    private void ResetCombo()
    {
        curCombo = 0;
        anim.SetInteger("AttackCombo", 0);
    }
    public void Attack1Start() 
    { 
        _attack1.SetActive(true); 
    }   
    public void Attack1End()
    {
        _attack1.SetActive(false);

    }
    public void Attack2Start()
    {
        _attack2.SetActive(true);
    }
    public void Attack2End()
    {
        _attack2.SetActive(false);

    }
    public void Attack3Start()
    {
        _attack3.SetActive(true);
    }
    public void Attack3End()
    {
        _attack3.SetActive(false);
    }

    public void ChargeAttackStart()
    {
        _chargeAttack.SetActive(true);
    }
    public void ChargeAttackEnd()
    {
        _chargeAttack.SetActive(false);
        isAttack = false;
    }
    public void JumpAttackStart()
    {
        _jumpAttack.SetActive(true);
        isAttack = false;
    }
    public void JumpAttackEnd()
    {
        _jumpAttack.SetActive(false);
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
        _upAttack.SetActive(true); 
    }
    public void UpAttackEnd()
    {
        _upAttack.SetActive(false);   // 히트박스 비활성화
        isAttack = false;
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
        _downattack.SetActive(true); 
    }
    public void DownAttackEnd()
    {
        _downattack.SetActive(false);   // 히트박스 비활성화
        isAttack = false;
    }

    public virtual void Skill() // AttackSkill = 1
    {
        if (Input.GetKeyDown(KeyCode.F) && PlayerState.instance.UseGauge(20))
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 1);

            Debug.Log("[PlayerAttack] 앞스킬 사용");
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
        if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.F) && PlayerState.instance.UseGauge(20))
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
        if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.F) && PlayerState.instance.UseGauge(20))
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


