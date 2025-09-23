using UnityEngine;

public class PlayerGuard : MonoBehaviour
{
    [HideInInspector]
    public bool isGuard;
    [HideInInspector]
    public float guardingTime = 0f;


    void Update()
    {
        isGuard = Input.GetKey(KeyCode.C);
        GetComponent<Animator>().SetBool("_isGuard", isGuard);

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
