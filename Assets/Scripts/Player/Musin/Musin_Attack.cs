using System.Collections;
using UnityEngine;

public class Musin_Attack : PlayerAttack
{
    [Header("ShotGun")]
    public float KnockbackTime = 0.3f;
    public float KnockbackXForce = 7f;
    public float KnockbackYForce = 2f;
    public GameObject shotGun;





    public override void Skill()
    {
        if ((Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) && Input.GetKeyDown(KeyCode.A) && PlayerState.instance.UseGauge(20))
        {
            shotGun.transform.position = gameObject.transform.position + new Vector3(1f, 0.5f, 0);
            shotGun.SetActive(true);
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 1);
            anim.SetBool("IsGround", PlayerState.instance.isGround);

            // ³Ë¹é
            rb.linearVelocity = Vector2.zero;
            if (!PlayerState.instance.isGround)
            {
                StartCoroutine(Co_DisableOtherAction(KnockbackTime));

                if (PlayerState.instance.isRight > 0)
                {
                    rb.AddForce(new Vector2(-KnockbackXForce, KnockbackYForce), ForceMode2D.Impulse);

                }
                else
                {
                    rb.AddForce(new Vector2(KnockbackXForce, KnockbackYForce), ForceMode2D.Impulse);

                }
            }
            else
            {
                StartCoroutine(Co_DisableOtherAction(KnockbackTime));
            }

        }
    }

    IEnumerator Co_DisableOtherAction(float duration)
    {
        DisableOtherAction();

        yield return new WaitForSeconds(duration);

        EnableOtherAction();
    }

}
