using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header(" === Default Attack === ")]
    [Header("Attack Setting")]
    public int maxCombo = 3;
    public int curCombo = 0;

    private bool isAttack = false;

    public float comboTime = 1f;            // 콤보 유지 시간
    private float lastAttackTime = -1f;         // 마지막 공격 시작 시간

    public GameObject _attack;
    public GameObject _downattack;
    public GameObject _upAttack;

    

    [Header("Skill")]
    public PlayerState playerstate; //게이지 호출


    [Header(" === Default Skill === ")]
    public GameObject _skill;

    [Header(" === Up Skill === ")]
    public GameObject _upSkill;

    [Header(" === Front Skill === ")]
    public GameObject _frontSkill;

    [Header(" === Down Skill === ")]
    public GameObject _downSkill;


    // 참조
    private Animator anim;
    private Rigidbody2D rb;

    private void Start()
    {
        anim = PlayerState.instance.anim;
        rb = PlayerState.instance.rb;
    }

    private void Update()
    {
        // 공격
        Attack();

        // 스킬
        
        Skill();
        Skill_Up();
        Skill_Front();
        Skill_Down();
        
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
                else if (Input.GetKey(KeyCode.DownArrow))   // 아래 공격
                {
                    StartCoroutine(Co_DownAttack());
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

    private IEnumerator Co_DownAttack()
    {
        Debug.Log("Down Attack Animation");
        anim.SetTrigger("IsDown");
        anim.SetTrigger("Attack");
        yield return new WaitForEndOfFrame();
       
    }

    private void ResetCombo()
    {
        curCombo = 0;
        anim.SetInteger("AttackCombo", 0);
    }



    public virtual void Skill() // AttackSkill = 1
    {

        if (Input.GetKeyDown(KeyCode.A)&&playerstate.UseGauge(20))
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 1);
            anim.SetBool("IsGround", playerstate.isGround);

        }
    }

    public virtual void Skill_Up() // AttackSkill = 2
    {
        if (Input.GetKey(KeyCode.UpArrow) && Input.GetKeyDown(KeyCode.A) && playerstate.UseGauge(20))
        {

            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 2);
            anim.SetBool("IsGround", playerstate.isGround);

        }
    }

    public virtual void Skill_Front() // AttackSkill = 3
    {
        if ((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) && Input.GetKeyDown(KeyCode.A) && playerstate.UseGauge(20))
        {

            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 3);
            anim.SetBool("IsGround", playerstate.isGround);

        }
    }

    public virtual void Skill_Down() // AttackSkill = 4
    {
        if (Input.GetKey(KeyCode.DownArrow) && Input.GetKeyDown(KeyCode.A) &&playerstate.UseGauge(20))
        {

            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 4);
            anim.SetBool("IsGround", playerstate.isGround);

        }
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


    public void AttackStart()    // 공격 애니메이션 
    {
        _attack.SetActive(true);    // 히트박스 활성화
    }
    public void AttackEnd()      // 공격 애니메이션 
    {
        _attack.SetActive(false);   // 히트박스 비활성화
        isAttack = false;
    }

    public void UpAttackStart()
    {
        _upAttack.SetActive(true);    // 히트박스 활성화
    }
    public void UpAttackEnd()
    {
        _upAttack.SetActive(false);   // 히트박스 비활성화
        isAttack = false;
    }

    public void DownAttackStart()
    {
        _downattack.SetActive(true);    // 히트박스 활성화
    }
    public void DownAttackEnd()
    {
        _downattack.SetActive(false);   // 히트박스 비활성화
        isAttack = false;
    }

    public void SkillStart()
    {
        _skill.SetActive(true);    // 히트박스 활성화
    }
    public void SkillEnd()
    {
        _skill.SetActive(false);   // 히트박스 비활성화
        anim.SetInteger("AttackSkill", 0);
    }

    public void UpSkillStart()
    {
        _upSkill.SetActive(true);    // 히트박스 활성화
    }

    public void UpSkillEnd()
    {
        _upSkill.SetActive(false);   // 히트박스 비활성화
        anim.SetInteger("AttackSkill", 0);
    }

    public void DownSkillStart()
    {
        _downSkill.SetActive(true);    // 히트박스 활성화
    }

    public void DownSkillEnd()
    {
        _downSkill.SetActive(false);   // 히트박스 비활성화
        anim.SetInteger("AttackSkill", 0);
    }

    public void FrontSkillStart()
    {
        _frontSkill.SetActive(true);    // 히트박스 활성화
    }
    public void FrontSkillEnd()
    {
        _frontSkill.SetActive(false);   // 히트박스 비활성화
        anim.SetInteger("AttackSkill", 0);
    }


}


