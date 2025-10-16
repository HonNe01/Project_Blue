using System.Collections;
using UnityEngine;

public class Musin_Attack : PlayerAttack
{
    [Header(" === Default Skill === ")]
    public GameObject shotGunEffect;
    [Header("Skill Power")]
    public float knockbackXForce = 7f;
    public float knockbackYForce = 2f;
    public float knockbackTime = 0.3f;
    [Header("Skill Offset")]
    public float skillOffsetX;
    public float skillOffsetY;


    [Header(" === Up Skill === ")]
    public GameObject boomPrefab;
    [Header("Up Skill Offset")]
    public float upSkillOffsetX;
    public float upSkillOffsetY;
    [Header("Up Skill Power")]
    public float throwXForce = 5f;
    public float throwYForce = 5f;


    [Header(" === Down Skill === ")]
    public float downKnockbackYForce = 7f;
    [SerializeField] private Vector3 downSkillOffset;


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

    public void SkillEfectOn()
    {
        if (PlayerState.instance.isRight > 0)
        {
            shotGunEffect.transform.position = gameObject.transform.position + new Vector3(skillOffsetX, skillOffsetY, 0);
            shotGunEffect.transform.rotation = Quaternion.Euler(0, 0, 0);
            shotGunEffect.SetActive(true);
        }
        else
        {
            shotGunEffect.transform.position = gameObject.transform.position + new Vector3(-skillOffsetX, skillOffsetY, 0);
            shotGunEffect.transform.rotation = Quaternion.Euler(0, 180, 0);
            shotGunEffect.SetActive(true);
        }
    }

    public override void Skill_Up()
    {
        if (PlayerState.instance.UseGauge(20))
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 2);
            Debug.Log("윗스킬 사용");
            GameObject grenade = Instantiate(boomPrefab, transform.position + new Vector3(upSkillOffsetX, upSkillOffsetY, 0), Quaternion.identity);
            Rigidbody2D grenadeRb = grenade.GetComponent<Rigidbody2D>();

            if (PlayerState.instance.isRight > 0)
            {
                grenadeRb.AddForce(new Vector2(throwXForce, throwYForce), ForceMode2D.Impulse);
            }
            else
            {
                grenadeRb.AddForce(new Vector2(-throwXForce, throwYForce), ForceMode2D.Impulse);
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
            rb.AddForce(new Vector2(0f, downKnockbackYForce), ForceMode2D.Impulse);
        }
    }

    public void DownSkillEffectOn()
    {
        if (PlayerState.instance.isRight > 0)
        {
            shotGunEffect.transform.position = gameObject.transform.position + downSkillOffset;
            shotGunEffect.transform.rotation = Quaternion.Euler(0, 0, -90);
            shotGunEffect.SetActive(true);
        }
        else
        {
            shotGunEffect.transform.position = gameObject.transform.position + downSkillOffset;
            shotGunEffect.transform.rotation = Quaternion.Euler(0, 180, -90);
            shotGunEffect.SetActive(true);
        }
    }
    
    IEnumerator Co_DisableOtherAction(float duration)
    {
        DisableOtherAction();

        yield return new WaitForSeconds(duration);

        EnableOtherAction();
    }
}
