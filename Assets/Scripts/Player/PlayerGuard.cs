using System.Collections;
using UnityEngine;

public class PlayerGuard : MonoBehaviour
{
    [Header("Guard Setting")]
    [SerializeField] public bool isGuard;               // 가드 버튼 누르는지
    [SerializeField] private float guardTime = 0f;      // 가드한 시간
    [SerializeField] private float guardDisableTime = 1.5f;

    [Header("Parry Setting")]
    [SerializeField] private float parrytime = 0.2f;    // 패링 판단 시간

    Rigidbody2D rb;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

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
        if (!PlayerState.instance.isGround) return;
        if (!PlayerState.instance.canGuard) return;

        isGuard = Input.GetKey(KeyCode.C);

        if (isGuard)
        {
            guardTime += Time.deltaTime;
            rb.linearVelocity = Vector2.zero;
            OnDisableActive();
        }
        else
        {
            guardTime = 0f;
            OnEnableActive();

        }
    }

    public void Guard()     // 가드 로직
    {
        Debug.Log("[PlayerState] Guard 성공!");

        StartCoroutine(GuardEnable());
        PlayerState.instance.anim.SetTrigger("IsBlock");
    }

    public bool IsGuard()   // 가드 했는지
    {
        return isGuard && guardTime > parrytime;
    }

    public bool IsParry()   // 패링 되는지
    {
        return isGuard && guardTime <= parrytime;
    }

    public void Parry()     // 패링 로직
    {
        Debug.Log("[PlayerState] Parry 성공!");

        StartCoroutine(GuardEnable());
        PlayerState.instance.anim.SetTrigger("IsParry");
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

    IEnumerator GuardEnable()
    {
        PlayerState.instance.canGuard = false;

        yield return new WaitForSeconds(guardDisableTime);

        PlayerState.instance.canGuard = true;
    }
}
