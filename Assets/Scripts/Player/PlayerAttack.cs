using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header(" === Default Attack === ")]
    [Header("Attack Setting")]
    private bool isAttack = false;
    public int maxCombo = 3;
    public int curCombo = 0;
    public float comboTime = 1f;            // 콤보 유지 시간
    private float lastAttackTime = -1f;         // 마지막 공격 시작 시간
    public GameObject _attack;

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
        // 공격 실행
        if (Input.GetKeyDown(KeyCode.V) && PlayerState.instance.canAttack)
        {
            if (!PlayerState.instance.isGround)         // 공중
            {
                if (Input.GetKey(KeyCode.UpArrow))          // 윗 공격
                {
                    StartCoroutine(Co_HighAttack(PlayerState.instance.isGround));
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
                    StartCoroutine(Co_HighAttack(PlayerState.instance.isGround));
                    Debug.Log("Up Attack");
                }
                else
                {
                    StartCoroutine(Co_Attack());// 일반 공격
                }
            }
        }

        // 콤보 시간 초과 -> 초기화
        if (Time.time - lastAttackTime > comboTime)
        {
            ResetCombo();
        }

        // 강공격
        if (Input.GetKey(KeyCode.V))
        {
            isAttack = true;
            if (!AttackPress)
            {
                AttackTimer += Time.deltaTime;
                if (AttackTimer >= AttackHoldTime)
                {
                    Debug.Log("강공격 준비완료");
                    AttackPress = true;
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.V))
        {
            anim.SetTrigger("Attack");
            anim.SetTrigger("ChargeAttack");
            isAttack = false;
            AttackTimer = 0f;
            AttackPress = false;
        }
    }

    private IEnumerator Co_Attack()
    {
        // 상태 정리
        curCombo++;
        isAttack = true;
        lastAttackTime = Time.time;
        rb.linearVelocity = Vector2.zero;
        
        // 공격 애니메이션 실행
        anim.SetTrigger("Attack");
        anim.SetInteger("AttackCombo", curCombo);

        yield return new WaitForEndOfFrame();

        // 공격 중 멈춤
        float attackTime = anim.GetCurrentAnimatorStateInfo(0).length * 0.7f;
        float timer = 0;

        while (attackTime > timer)
        {
            DisableOtherAction();
            timer += Time.deltaTime;
            yield return null;
        }

        // 멈춤 해제
        EnableOtherAction();
        isAttack = false;

        // 콤보 완료 -> 초기화
        if (curCombo >= maxCombo)
        {
            ResetCombo();
        }
    }
    public void AttackStart() { _attack.SetActive(true); }   // 공격 애니메이션
    public void AttackEnd()      // 공격 애니메이션 
    {
        _attack.SetActive(false);   // 히트박스 비활성화
        isAttack = false;
    }
    private IEnumerator Co_JumpAttack()
    {
        anim.SetTrigger("Attack");
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator Co_HighAttack(bool isGround)
    {
        anim.SetTrigger("IsUp");
        anim.SetTrigger("Attack");
        yield return new WaitForEndOfFrame();
    }
    public void HighAttackStart() { _upAttack.SetActive(true); }    // 히트박스 활성화 
    public void HighAttackEnd()
    {
        _upAttack.SetActive(false);   // 히트박스 비활성화
        isAttack = false;
    }

    private IEnumerator Co_DownAttack()
    {
        Debug.Log("Down Attack Animation");
        anim.SetTrigger("IsDown");
        anim.SetTrigger("Attack");
        yield return new WaitForEndOfFrame();
    }
    public void DownAttackStart() { _downattack.SetActive(true); } // 히트박스 활성화
    public void DownAttackEnd()
    {
        _downattack.SetActive(false);   // 히트박스 비활성화
        isAttack = false;
    }

    private void ResetCombo()
    {
        curCombo = 0;
        anim.SetInteger("AttackCombo", 0);
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


