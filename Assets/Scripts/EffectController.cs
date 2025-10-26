using UnityEngine;

public class EffectController : MonoBehaviour
{
    public void EffectOff()
    {
        gameObject.SetActive(false);
    }

    public void AttackEffectOff()
    {
        PlayerState.instance.playerAttack.isAttack = false;

        EffectOff();
    }

    public void SkillEffectOff()
    {
        PlayerState.instance.anim.SetInteger("AttackSkill", 0);

        EffectOff();
    }

    public void HitEffectOff()
    {
        gameObject.SetActive(false);
    }
}
