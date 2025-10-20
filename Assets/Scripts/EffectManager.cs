using UnityEngine;
using UnityEngine.Animations;


public class EffectManager : MonoBehaviour
{

    [HideInInspector]public Animator anim;

    private void Start()
    {
        anim = PlayerState.instance.anim;
    }
    public void EffectOff()
    {
        gameObject.SetActive(false);
        anim.SetInteger("AttackSkill", 0);


    }

}
