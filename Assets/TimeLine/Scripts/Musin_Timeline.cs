using UnityEngine;

public class Musin_Timeline : MonoBehaviour
{
    SpriteRenderer sprite;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    public void AE_DashSound()
    {
        if (SoundManager.instance) SoundManager.instance.PlaySFX(SoundManager.SFX.Dash);
    }

    public void AE_DashEffect()
    {
        if (EffectManager.instance) EffectManager.instance.PlayEffect(EffectManager.EffectType.Dash, transform.position, sprite.flipX);
    }

    public void AE_Attack1Sound()
    {
        if (EffectManager.instance) EffectManager.instance.PlayEffect(EffectManager.EffectType.Attack1, transform.position, sprite.flipX);
    }
}
