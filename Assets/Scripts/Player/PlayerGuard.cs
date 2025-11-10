using System.Collections;
using UnityEngine;

public class PlayerGuard : MonoBehaviour
{
    [Header("Guard Setting")]
    [SerializeField] public bool isGuard;               // 가드 버튼 누르는지
    [SerializeField] private float guardTime = 0f;      // 가드한 시간
    [SerializeField] private float guardDisableTime = 1.5f;
    [field: SerializeField] public bool isGuarded { get; private set; }         // 가드 성공 여부
    [SerializeField] private LayerMask enemyAttackMask;


    [Header("Guard direction")]
    [SerializeField] private Vector2 guardSize;
    [SerializeField] private Vector2 guardOffset;


    [Header("Parry Setting")]
    [SerializeField] private float parrytime = 0.2f;    // 패링 판단 시간


    private void Update()
    {
        if (PlayerState.instance.isDie || GameManager.instance.State == GameManager.GameState.Directing) return;

        GuardInput();
    }

    private void LateUpdate()
    {
        PlayerState.instance.anim.SetBool("IsGuard", isGuard);
    }

    private void GuardInput()
    {
        if (!PlayerState.instance.canGuard || !PlayerState.instance.isGround) return;

        isGuard = Input.GetKey(KeyCode.C);

        if (isGuard)
        {
            guardTime += Time.deltaTime;
            PlayerState.instance.rb.linearVelocity = Vector2.zero;
            OnDisableActive();

            GuardCheck();
        }
        else
        {
            guardTime = 0f;
            isGuarded = false;

            OnEnableActive();
        }
    }

    private void GuardCheck()
    {
        float posX = transform.position.x + guardOffset.x * (PlayerState.instance ? PlayerState.instance.isRight : 1);
        float posY = transform.position.y + guardOffset.y;
        Vector2 pos = new Vector2(posX, posY);

        Collider2D[] hits = Physics2D.OverlapBoxAll(pos, guardSize, 0f, enemyAttackMask);
        if (hits != null && hits.Length > 0)
        {
            isGuarded = true;
        }
    }

    public void Guard()     // 가드 로직
    {
        Debug.Log("[PlayerState] Guard!");
        StartCoroutine(GuardEnable());
        StartCoroutine(Co_Guard());
    }
    private IEnumerator Co_Guard()
    {
        PlayerState.instance.anim.SetTrigger("IsBlock");
        yield return null;

        float animLength = PlayerState.instance.anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);

        OnEnableActive();
    }
    public bool IsGuard()   // 가드 했는지
    {
        return isGuarded && guardTime > parrytime;
    }

    public void Parry()     // 패링 로직
    {
        Debug.Log("[PlayerState] Parry!");
        StartCoroutine(Co_Parry());
    }
    private IEnumerator Co_Parry()
    {
        PlayerState.instance.anim.SetTrigger("IsParry");
        yield return null;

        float animLength = PlayerState.instance.anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);

        OnEnableActive();
    }
    public bool IsParry()   // 패링 했는지
    {
        return isGuarded && guardTime <= parrytime;
    }

    public void OffGuarded()
    {
        isGuarded = false;
    }

    private void OnDisableActive()
    {
        PlayerState.instance.canMove = false;
        PlayerState.instance.canJump = false;
        PlayerState.instance.canDash = false;
        PlayerState.instance.canAttack = false;
        PlayerState.instance.canHeal = false;
        PlayerState.instance.canSkill = false;
    }

    private void OnEnableActive()
    {
        PlayerState.instance.canMove = true;
        PlayerState.instance.canJump = true;
        PlayerState.instance.canDash = true;
        PlayerState.instance.canAttack = true;
        PlayerState.instance.canHeal = true;
        PlayerState.instance.canSkill = true;
    }

    private IEnumerator GuardEnable()
    {
        PlayerState.instance.canGuard = false;

        yield return new WaitForSeconds(guardDisableTime);

        PlayerState.instance.canGuard = true;
    }

    private void OnDrawGizmos() // 시각화 디버깅
    {
        float posX = transform.position.x + guardOffset.x * (PlayerState.instance ? PlayerState.instance.isRight : 1);
        float posY = transform.position.y + guardOffset.y;

        Vector2 pos = new Vector2(posX, posY);
        Gizmos.color = isGuard ? Color.green : Color.red;
        Gizmos.DrawWireCube(pos, guardSize);
    }
}
