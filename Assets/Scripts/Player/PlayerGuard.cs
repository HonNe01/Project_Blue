using UnityEngine;

public class PlayerGuard : MonoBehaviour
{
    [HideInInspector]
    public bool isGuard;
    [HideInInspector]
    public float guardingTime = 0f;


    private void Update()
    {
        Guard();
    }

    private void LateUpdate()
    {
        PlayerState.instance.anim.SetBool("IsGuard", isGuard);
    }

    private void Guard()
    {
        if (!PlayerState.instance.canGuard) return;

        isGuard = Input.GetKey(KeyCode.C);

        if (isGuard)
            guardingTime += Time.deltaTime;
        else
            guardingTime = 0f;
    }

    public bool IsGuarding()
    {
        return isGuard;
    }
}
