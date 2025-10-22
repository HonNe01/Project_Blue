using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Musin_Attack : PlayerAttack
{
    [Header(" === Default Skill === ")]
    public GameObject skillEffect;
    [Header("Skill Power")]
    public float skillknockbackXForce = 7f;
    public float skillknockbackYForce = 2f;
    public float skillknockbackTime = 0.3f;
    [Header("Skill Offset")]
    public float skillOffsetX;
    public float skillOffsetY;


    [Header(" === Up Skill === ")]
    public GameObject skillUpNormalPrefab;
    public GameObject skillUpImpactPrefab;
    public GameObject skillUpElectronicPrefab;
    public GameObject skillUpFirePrefab;
   
    [Header("Up Skill Offset")]
    public float SkillUpOffsetX;
    public float SkillUpOffsetY;
    [Header("Up Skill Power")]
    public float skillUpthrowXForce = 5f;
    public float skillUpthrowYForce = 5f;


    [Header(" === Down Skill === ")]
    public float skillDownKnockbackYForce = 7f;
    [SerializeField] private Vector3 skillDownOffset;
    public GameObject skillDownEffect;


    [Header(" === Slach Moduel === ")]
    public GameObject _slash1;
    public Vector3 slash1position;
    public Rigidbody2D _slash1rb;
    
    public GameObject _slash2;
    
    public GameObject _slash3;
    
    public GameObject _slashCharge;
    
    public GameObject _slashJump;
    public float slashspeed;
    


    [Header("참조")]
    public Musin_State musinstate;




    public override void Skill()
    {
        if (PlayerState.instance.UseGauge(20))
        {

            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 1);
            Debug.Log("일반 스킬 사용");


            // 넉백
            rb.linearVelocity = Vector2.zero;
            if (!PlayerState.instance.isGround)
            {
                StartCoroutine(Co_DisableOtherAction(skillknockbackTime));

                if (PlayerState.instance.isRight > 0)
                {
                    rb.AddForce(new Vector2(-skillknockbackXForce, skillknockbackYForce), ForceMode2D.Impulse);

                }
                else
                {
                    rb.AddForce(new Vector2(skillknockbackXForce, skillknockbackYForce), ForceMode2D.Impulse);

                }
            }
            else
            {
                StartCoroutine(Co_DisableOtherAction(skillknockbackTime));
            }

            

        }
    }

    public void SkillEfectOn()
    {
        if (PlayerState.instance.isRight > 0)
        {
            skillEffect.transform.position = gameObject.transform.position + new Vector3(skillOffsetX, skillOffsetY, 0);
            skillEffect.transform.rotation = Quaternion.Euler(0, 0, 0);
            skillEffect.SetActive(true);
        }
        else
        {
            skillEffect.transform.position = gameObject.transform.position + new Vector3(-skillOffsetX, skillOffsetY, 0);
            skillEffect.transform.rotation = Quaternion.Euler(0, 180, 0);
            skillEffect.SetActive(true);
        }
    }

    public override void Skill_Up()
    {
        if (Musin_State.instance.UseGauge(20) && musinstate.firegranade)
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 2);
            Debug.Log("윗스킬 사용");
            GameObject grenade = Instantiate(skillUpFirePrefab, transform.position + new Vector3(SkillUpOffsetX, SkillUpOffsetY, 0), Quaternion.identity);
            Rigidbody2D grenadeRb = grenade.GetComponent<Rigidbody2D>();

            if (PlayerState.instance.isRight > 0)
            {
                grenadeRb.AddForce(new Vector2(skillUpthrowXForce, skillUpthrowYForce), ForceMode2D.Impulse);
            }
            else
            {
                grenadeRb.AddForce(new Vector2(-skillUpthrowXForce, skillUpthrowYForce), ForceMode2D.Impulse);
            }
        }
        else if (musinstate.impactgranade)
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 2);
            Debug.Log("윗스킬 사용");
            GameObject grenade = Instantiate(skillUpImpactPrefab, transform.position + new Vector3(SkillUpOffsetX, SkillUpOffsetY, 0), Quaternion.identity);
            Rigidbody2D grenadeRb = grenade.GetComponent<Rigidbody2D>();

            if (PlayerState.instance.isRight > 0)
            {
                grenadeRb.AddForce(new Vector2(skillUpthrowXForce, skillUpthrowYForce), ForceMode2D.Impulse);
            }
            else
            {
                grenadeRb.AddForce(new Vector2(-skillUpthrowXForce, skillUpthrowYForce), ForceMode2D.Impulse);
            }
        }
        else if (musinstate.electricgranade)
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 2);
            Debug.Log("윗스킬 사용");
            GameObject grenade = Instantiate(skillUpElectronicPrefab, transform.position + new Vector3(SkillUpOffsetX, SkillUpOffsetY, 0), Quaternion.identity);
            Rigidbody2D grenadeRb = grenade.GetComponent<Rigidbody2D>();

            if (PlayerState.instance.isRight > 0)
            {
                grenadeRb.AddForce(new Vector2(skillUpthrowXForce, skillUpthrowYForce), ForceMode2D.Impulse);
            }
            else
            {
                grenadeRb.AddForce(new Vector2(-skillUpthrowXForce, skillUpthrowYForce), ForceMode2D.Impulse);
            }
        }
        else
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 2);
            Debug.Log("윗스킬 사용");
            GameObject grenade = Instantiate(skillUpNormalPrefab, transform.position + new Vector3(SkillUpOffsetX, SkillUpOffsetY, 0), Quaternion.identity);
            Rigidbody2D grenadeRb = grenade.GetComponent<Rigidbody2D>();

            if (PlayerState.instance.isRight > 0)
            {
                grenadeRb.AddForce(new Vector2(skillUpthrowXForce, skillUpthrowYForce), ForceMode2D.Impulse);
            }
            else
            {
                grenadeRb.AddForce(new Vector2(-skillUpthrowXForce, skillUpthrowYForce), ForceMode2D.Impulse);
            }
        }
    }



    public override void Skill_Down()
    {
        if (PlayerState.instance.UseGauge(20))
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 3);

            Debug.Log("아래 스킬 사용");

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(0f, skillDownKnockbackYForce), ForceMode2D.Impulse);
        }
    }

    public void DownSkillEffectOn()
    {
        if (PlayerState.instance.isRight > 0)
        {
            skillDownEffect.transform.position = gameObject.transform.position + skillDownOffset;
            skillDownEffect.transform.rotation = Quaternion.Euler(0, 0, -90);
            skillDownEffect.SetActive(true);
        }
        else
        {
            skillDownEffect.transform.position = gameObject.transform.position + skillDownOffset;
            skillDownEffect.transform.rotation = Quaternion.Euler(0, 180, -90);
            skillDownEffect.SetActive(true);
        }
    }
    
    IEnumerator Co_DisableOtherAction(float duration)
    {
        DisableOtherAction();

        yield return new WaitForSeconds(duration);

        EnableOtherAction();
    }

    public override void Attack1Start()
    {
        if (musinstate.swordSlash)
        {
            _slash1.SetActive(true);
            if (PlayerState.instance.isRight > 0)
            {
                _slash1rb.linearVelocity = slash1position + new Vector3(slashspeed, 0, 0);
            }
            else
            {
                _slash1rb.linearVelocity = slash1position + new Vector3(-slashspeed, 0, 0);
            }
        }
        else
        {
            _attack1.SetActive(true);
        }
        
    }
    public override void Attack1End()
    {
        if (musinstate.swordSlash)
        {
            _slash1.transform.position = slash1position;
            _slash1.SetActive(false);
        }
        else
        {
            _attack1.SetActive(false);
        }
    }
    public override void Attack2Start()
    {
        _attack2.SetActive(true);
    }
    public override void Attack2End()
    {
        _attack2.SetActive(false);

    }
    public override void Attack3Start()
    {
        _attack3.SetActive(true);
    }
    public override void Attack3End()
    {
        _attack3.SetActive(false);
    }

    public override void ChargeAttackStart()
    {
        _chargeAttack.SetActive(true);
    }
    public override void ChargeAttackEnd()
    {
        _chargeAttack.SetActive(false);
        isAttack = false;
    }
    public override void JumpAttackStart()
    {
        _jumpAttack.SetActive(true);
        isAttack = false;
    }
    public override void JumpAttackEnd()
    {
        _jumpAttack.SetActive(false);
        isAttack = false;
    }
}
