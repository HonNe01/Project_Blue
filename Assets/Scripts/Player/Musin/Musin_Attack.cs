using UnityEngine;

public class Musin_Attack : PlayerAttack
{
    public PlayerState PlayerState;
 

    public PlayerMove PlayerMove;
    private Rigidbody2D rb;

    [Header("Knockback")]
    public float KnockbackTime = 0.3f;
    public float KnockbackXForce = 7f;
    public float KnockbackYForce = 7f;

    private void Start()
    {
        base.Start();

    }

    /*
    public override void Skill_Front()
    {
        if (!PlayerState.instance.isGround)
        {
            StartCoroutine(PlayerMove.WallJumpLock(KnockbackTime));

            if (PlayerState.instance.isRight > 0)
            {
                rb.AddForce(new Vector2(KnockbackXForce,KnockbackYForce), ForceMode2D.Impulse);
            }
            else
            {
                rb.AddForce(new Vector2(-KnockbackXForce, KnockbackYForce), ForceMode2D.Impulse);
            }
        }
        
    }
    */
}
