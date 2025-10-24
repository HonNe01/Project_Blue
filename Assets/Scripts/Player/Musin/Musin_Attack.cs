using System.Collections;
using UnityEngine;

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


    [Header(" === Slash Moduel === ")]
    public GameObject _slash1;
    public Rigidbody2D _slash1rb;

    public GameObject _slash2;
    public Rigidbody2D _slash2rb;

    public GameObject _slash3;
    public Rigidbody2D _slash3rb;

    public GameObject _slashCharge;

    public GameObject _slashJump;
    public float slashspeed = 20f;


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
            EffectManager.instance.PlayEffect(EffectManager.EffectType.Skill, transform.position, PlayerState.instance.isRight < 0);
        }
        else
        {
            EffectManager.instance.PlayEffect(EffectManager.EffectType.Skill, transform.position, PlayerState.instance.isRight < 0);
        }
    }

    public override void Skill_Up()
    {
        if (PlayerState.instance.UseGauge(20))
        {
            if (((Musin_State)PlayerState.instance).fireGranade)
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
            else if (((Musin_State)PlayerState.instance).impactGranade)
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
            else if (((Musin_State)PlayerState.instance).electricGranade)
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
        EffectManager.instance.PlayEffect(EffectManager.EffectType.SkillDown, transform.position, PlayerState.instance.isRight < 0);
    }

    IEnumerator Co_DisableOtherAction(float duration)
    {
        DisableOtherAction();

        yield return new WaitForSeconds(duration);

        EnableOtherAction();
    }

    

    public override void Attack1Start()
    {
        if (((Musin_State)PlayerState.instance).swordSlash)
        {
            EffectManager.instance.PlayEffect(EffectManager.EffectType.Attack1_Module, transform.position, PlayerState.instance.isRight < 0);
            EffectManager.instance.MoveEffect(EffectManager.EffectType.Attack1_Module, slashspeed);
        }
        else
        {
            EffectManager.instance.PlayEffect(EffectManager.EffectType.Attack1, transform.position, PlayerState.instance.isRight < 0);
        }
    }
    public override void Attack2Start()
    {
        if (((Musin_State)PlayerState.instance).swordSlash)
        {
            EffectManager.instance.PlayEffect(EffectManager.EffectType.Attack2_Module, transform.position, PlayerState.instance.isRight < 0);
            EffectManager.instance.MoveEffect(EffectManager.EffectType.Attack2_Module, slashspeed);
        }
        else
        {
            EffectManager.instance.PlayEffect(EffectManager.EffectType.Attack2, transform.position, PlayerState.instance.isRight < 0);
        }
    }
    public override void Attack3Start()
    {
        if (((Musin_State)PlayerState.instance).swordSlash)
        {
            EffectManager.instance.PlayEffect(EffectManager.EffectType.Attack3_Module, transform.position, PlayerState.instance.isRight < 0);
            EffectManager.instance.MoveEffect(EffectManager.EffectType.Attack3_Module, slashspeed);
        }
        else
        {
            EffectManager.instance.PlayEffect(EffectManager.EffectType.Attack3, transform.position, PlayerState.instance.isRight < 0);
        }
    }

    public override void ChargeAttackStart()
    {
        if (((Musin_State)PlayerState.instance).swordSlash)
        {
            EffectManager.instance.PlayEffect(EffectManager.EffectType.ChargeAttack_Module, transform.position, PlayerState.instance.isRight < 0);
            EffectManager.instance.MoveEffect(EffectManager.EffectType.ChargeAttack_Module, slashspeed);
        }
        else 
        {
            EffectManager.instance.PlayEffect(EffectManager.EffectType.ChargeAttack, transform.position, PlayerState.instance.isRight < 0);
        }

    }
    public override void JumpAttackStart()
    {
        if (((Musin_State)PlayerState.instance).swordSlash)
        {
            EffectManager.instance.PlayEffect(EffectManager.EffectType.JumpAttack_Module, transform.position, PlayerState.instance.isRight < 0);
            EffectManager.instance.MoveEffect(EffectManager.EffectType.JumpAttack_Module, slashspeed);
        }
        else
        {
            EffectManager.instance.PlayEffect(EffectManager.EffectType.JumpAttack, transform.position, PlayerState.instance.isRight < 0);
        }
    }
}
