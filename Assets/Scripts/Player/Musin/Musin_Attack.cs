using System.Collections;
using UnityEngine;

public class Musin_Attack : PlayerAttack
{
    [Header(" === Default Skill === ")]
    public float knockbackTime = 0.3f;
    public float knockbackXForce = 7f;
    public float knockbackYForce = 2f;
    public GameObject shotGun;

    public GameObject _skill;

    [Header(" === Up Skill === ")]
    public GameObject _upSkill;
    public GameObject boomPrefab;
    public Vector3 boomspawnpos;
    public float throwXForce = 5f;
    public float throwYForce = 5f;


    [Header(" === Down Skill === ")]
    public GameObject _downSkill;
    public float downKnockbackYForce = 7f;



    public override void Skill()
    {
        if (PlayerState.instance.UseGauge(20))
        {

            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 1);
            anim.SetBool("IsGround", PlayerState.instance.isGround);
            Debug.Log("일반 스킬 사용");


            if (PlayerState.instance.isRight > 0)
            {
                shotGun.transform.position = gameObject.transform.position + new Vector3(1f, 0.5f, 0);
                shotGun.transform.rotation = Quaternion.Euler(0, 0, 0);
                shotGun.SetActive(true);
            }
            else
            {
                shotGun.transform.position = gameObject.transform.position + new Vector3(-1f, 0.5f, 0);
                shotGun.transform.rotation = Quaternion.Euler(0, 180, 0);
                shotGun.SetActive(true);
            }
            

            // 넉백
            rb.linearVelocity = Vector2.zero;
            if (!PlayerState.instance.isGround)
            {
                StartCoroutine(Co_DisableOtherAction(knockbackTime));

                if (PlayerState.instance.isRight > 0)
                {
                    rb.AddForce(new Vector2(-knockbackXForce, knockbackYForce), ForceMode2D.Impulse);

                }
                else
                {
                    rb.AddForce(new Vector2(knockbackXForce, knockbackYForce), ForceMode2D.Impulse);

                }
            }
            else
            {
                StartCoroutine(Co_DisableOtherAction(knockbackTime));
            }

        }
    }

    public override void Skill_Down()
    {
        if (PlayerState.instance.UseGauge(20))
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 4);

            Debug.Log("아래 스킬 사용");

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(0f, downKnockbackYForce), ForceMode2D.Impulse);
        }
    }

    public override void Skill_Up()
    {
        if (PlayerState.instance.UseGauge(20))
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 2);
            Debug.Log("윗스킬 사용");
            GameObject grenade = Instantiate(boomPrefab, transform.position + boomspawnpos, Quaternion.identity);
            Rigidbody2D rb = grenade.GetComponent<Rigidbody2D>();

            if (PlayerState.instance.isRight > 0)
            {
                rb.AddForce(new Vector2(throwXForce, throwYForce), ForceMode2D.Impulse);
            }
            else
            {
                rb.AddForce(new Vector2(-throwXForce, throwYForce), ForceMode2D.Impulse);
            }

        }

    }

    IEnumerator Co_DisableOtherAction(float duration)
    {
        DisableOtherAction();

        yield return new WaitForSeconds(duration);

        EnableOtherAction();
    }


    public override void SkillStart()
    {
        _skill.SetActive(true);    // 히트박스 활성화
    }
    public override void SkillEnd()
    {
        _skill.SetActive(false);   // 히트박스 비활성화
        anim.SetInteger("AttackSkill", 0);
    }

    public override void UpSkillStart()
    {
        _upSkill.SetActive(true);    // 히트박스 활성화
    }

    public override void UpSkillEnd()
    {
        _upSkill.SetActive(false);   // 히트박스 비활성화
        anim.SetInteger("AttackSkill", 0);
    }

    public override void DownSkillStart()
    {
        _downSkill.SetActive(true);    // 히트박스 활성화
    }

    public override void DownSkillEnd()
    {
        _downSkill.SetActive(false);   // 히트박스 비활성화
        anim.SetInteger("AttackSkill", 0);
    }


}
