using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header(" === Default Attack === ")]
    [Header("Attack Setting")]
    public int maxCombo = 3;                // 최대 콤보
    [SerializeField] private int curCombo = 0;  // 현재 콤보

    public float comboTime = 1f;            // 콤보 유지 시간
    private float lastAttackTime = -1f;         // 마지막 공격 시작 시간

    public float inputBufferTime = 0.2f;    // 버퍼 입력 시간
    private bool bufferInput = false;           // 버퍼 입력 여부
    private float lastInputTime = -1f;          // 마지먹 버퍼 입력 시간

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
            lastInputTime = Time.time;

            // 공격 중 아닐 때 (Combo = 0) -> 1타 시작
            if (curCombo == 0)
            {
                StartAttack(1);
            }
            // 공격 중일 때 (Combo != 0) -> 버퍼 입력
            else if (curCombo < maxCombo)   
            {
                bufferInput = true;
            }
            // 마지막 공격 중엔 무시
            else    
            {
                bufferInput = false;
            }
        }

        if (curCombo == 0) return;

        // 콤보 시간 초과 -> 초기화
        if (Time.time - lastAttackTime > comboTime)
        {
            ResetCombo();
            return;
        }

        // 버퍼 입력 -> 공격 실행
        if (bufferInput && Time.time - lastInputTime <= inputBufferTime)
        {
            bufferInput = false;
            if (curCombo < maxCombo)
            {
                StartAttack(curCombo + 1);
            }
            else
            {
                // 콤보 끝 -> 초기화
                ResetCombo();
            }
        }
    }

    private void StartAttack(int combo)
    {
        curCombo = combo;
        lastAttackTime = Time.time;
        bufferInput = false;

        PlayerState.instance.canMove = false;
        anim.SetTrigger($"Attack{combo}");
    }

    void ResetCombo()
    {
        curCombo = 0;
        bufferInput = false;

        PlayerState.instance.canMove = true;
    }

    public virtual void Skill()
    {

    }

    public virtual void Skill_Up()
    {

    }

    public virtual void Skill_Front()
    {

    }

    public virtual void Skill_Down()
    {

    }
}


