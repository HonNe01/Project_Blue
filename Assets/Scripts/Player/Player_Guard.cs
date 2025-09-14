using UnityEngine;

public class Player_Guard : MonoBehaviour
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
}
