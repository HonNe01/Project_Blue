using System.Collections;
using UnityEngine;

public class PlayerGuard : MonoBehaviour
{
    [Header("Guard Setting")]
    [SerializeField] public bool isGuard;               // 가드 버튼 누르는지
    [SerializeField] private float guardTime = 0f;      // 가드한 시간
    [SerializeField] private float guardDisableTime = 0.5f;

    [Header("Parry Setting")]
    [SerializeField] private float parrytime = 0.2f;    // 패링 판단 시간

    private void Update()
    {
        GuardInput();
    }

    private void LateUpdate()
    {
        PlayerState.instance.anim.SetBool("IsGuard", isGuard);
    }

    private void GuardInput()
    {
        if (!PlayerState.instance.canGuard) return;

        isGuard = Input.GetKey(KeyCode.C);

        if (isGuard)
            guardTime += Time.deltaTime;
        else
            guardTime = 0f;
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

    IEnumerator GuardEnable()
    {
        PlayerState.instance.canGuard = false;

        yield return new WaitForSeconds(guardDisableTime);

        PlayerState.instance.canGuard = true;
    }
}
