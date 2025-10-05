using UnityEngine;

public class PlayerGuard : MonoBehaviour
{
    [Header("Guard Setting")]
    public bool isGuard;
    [SerializeField] private float guardTime = 0f;

    [Header("Parry Setting")]
    [SerializeField] private float parrytime = 0.2f;

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

    public void Guard()
    {
        Debug.Log("[PlayerState] Guard 성공!");
        PlayerState.instance.anim.SetTrigger("IsGuardHit");
    }

    public bool IsGuard()
    {
        return isGuard;
    }

    public bool IsParry()
    {
        return isGuard && guardTime <= parrytime;
    }

    public void Parry()
    {
        Debug.Log("[PlayerState] Parry 성공!");
        PlayerState.instance.anim.SetTrigger("IsParry");
    }
}
