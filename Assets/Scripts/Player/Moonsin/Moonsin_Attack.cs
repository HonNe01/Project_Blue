using System.Collections;
using UnityEngine;

public class Moonsin_Attack : PlayerAttack
{
    [Header("down skill")]
    public GameObject _downSkillEffect;
    public float downSkillOffsetY = 20f;
    public float skillDownTime = 0.3f;
    public float downSkillDisableOtherActionDuration = 0.5f;
    public float downSkillTime = 0f;
    private bool isSmash = false;

    [Header("up skill")]
    public GameObject skillUpPrefab;
    public float skillUpOffsetX = 1.5f;
    public float skillUpOffsetY = 2.5f;


    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.F) && Input.GetKeyDown(KeyCode.DownArrow) && PlayerState.instance.UseGauge(20))
        {
            if (downSkillTime < downSkillDisableOtherActionDuration && PlayerState.instance.isGround)
            {
                Debug.Log("ÀÌÆåÆ® Ãâ·Â");
                isSmash = true;
            }
            else
            {
                isSmash = false;
            }
        }


    }

    public override void Skill()
    {
        
    }

    public void SkillEffectOn()
    {
        
    }

    public override void Skill_Up()
    {
        if (PlayerState.instance.UseGauge(20))
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 2);

            if (PlayerState.instance.isRight > 0)
            {
                Vector3 spawnPosition = new Vector3(transform.position.x + skillUpOffsetX, transform.position.y + skillUpOffsetY, transform.position.z);
                Instantiate(skillUpPrefab, spawnPosition, Quaternion.identity);
            }
            else
            {
                Vector3 spawnPosition = new Vector3(transform.position.x - skillUpOffsetX, transform.position.y + skillUpOffsetY, transform.position.z);
                Instantiate(skillUpPrefab, spawnPosition, Quaternion.identity);
            }
        }
    }

    public void SkillUpEffectOn()
    {

    }

    public override void Skill_Down()
    {
        if (PlayerState.instance.UseGauge(20))
        {
            downSkillTime = 0f;
            isSmash = false;
            StartCoroutine(Co_DisableOtherAction(downSkillDisableOtherActionDuration));
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackSkill", 3);
            // ÂøÁö
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(0, -downSkillOffsetY), ForceMode2D.Impulse);
            isSmash = true;

            downSkillTime += Time.deltaTime;
            
        }
    }



    public void SkillDownEffectOn()
    {
        if (isSmash)
        {
            _downSkillEffect.transform.position = gameObject.transform.position ;
            _downSkillEffect.SetActive(true);
            Debug.Log("ÀÌÆåÆ® Ãâ·Â ¿Ï·á");
        }
    }


    IEnumerator Co_DisableOtherAction(float duration)
    {
        DisableOtherAction();
        yield return new WaitForSeconds(duration);
        EnableOtherAction();
    }
}
