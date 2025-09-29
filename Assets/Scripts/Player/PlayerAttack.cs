using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header(" === Default Attack === ")]
    [Header("Attack Setting")]
    public int maxCombo = 3;                // 최대 콤보
    [SerializeField] private int curCombo = 0;  // 현재 콤보

    public float comboTime = 1f;            // 콤보 유지 시간
    private float lastAttackTime = -1f;         // 마지막 공격 시작 시간

    /*
    [Header(" === Default Skill === ")]

    [Header(" === Up Skill === ")]

    [Header(" === Front Skill === ")]

    [Header(" === Down Skill === ")]
    */

    // 참조
    private Animator anim;
    private Rigidbody2D rb;

    private void Start()
    {
        anim = PlayerState.instance.anim;
        rb = PlayerState.instance.rb;
    }

    private void Update()
    {
        // 공격
        Attack();

        // 스킬
        Skill();
        Skill_Up();
        Skill_Front();
        Skill_Down();
    }

    public void Attack()
    {
        // 공격 실행
        if (Input.GetKeyDown(KeyCode.V) && PlayerState.instance.canAttack)
        {
            // 공격 중 아닐 때 (Combo = 0) -> 1타 시작
            if (curCombo == 0)
            {
                StartAttack();
            }
            // 공격 중 -> 다음 공격
            else if (curCombo < maxCombo)   
            {
                StartAttack();
            }
        }

        if (curCombo == 0) return;


        // 콤보 시간 초과 -> 초기화
        if (Time.time - lastAttackTime > comboTime)
        {
            ResetCombo();
            return;
        }
    }

    private void StartAttack() // AttackCount = 0
    {
        curCombo++;
        lastAttackTime = Time.time;

        DisableOtherAction();
        anim.SetTrigger("Attack");
        anim.SetInteger("AttackCombo", 0);
    }

    private void ResetCombo()
    {
        curCombo = 0;
        anim.SetInteger("AttackCombo", 0);

        EnableOtherAction();
    }


    public virtual void Skill() // AttackCount = 1
    {

    }

    public virtual void Skill_Up() // AttackCount = 2
    {

    }

    public virtual void Skill_Front() // AttackCount = 3
    {

    }

    public virtual void Skill_Down() // AttackCount = 4
    {

    }

    public void EnableOtherAction()
    {
        PlayerState.instance.canMove = true;
        PlayerState.instance.canHeal = true;
        PlayerState.instance.canGuard = true;
    }

    public void DisableOtherAction()
    {
        PlayerState.instance.canMove = false;
        PlayerState.instance.canHeal = false;
        PlayerState.instance.canGuard = false;
    }
}


