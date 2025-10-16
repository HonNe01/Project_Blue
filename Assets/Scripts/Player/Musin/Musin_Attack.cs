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
    public GameObject skillUpPrefab;
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
        if (PlayerState.instance.UseGauge(20))
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 2);
            Debug.Log("윗스킬 사용");
            GameObject grenade = Instantiate(skillUpPrefab, transform.position + new Vector3(SkillUpOffsetX, SkillUpOffsetY, 0), Quaternion.identity);
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
}
