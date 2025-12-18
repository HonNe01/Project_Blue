using UnityEngine;

public class PowerCore : MonoBehaviour
{
    public enum CoreState
    {
        Hiding,     // 은폐
        Exposure,   // 노출
        Destroyed,  // 파괴
    }


    [Header(" === Core State === ")]
    public CoreState state = CoreState.Hiding;  // 동력원 상태
    public int maxHp = 10;                      // 동력원 최대 체력
    public int curHp;                           // 동력원 체력

    public bool isExposure = false;             // 동력원 노출 여부
    public bool IsDestroyed => state == CoreState.Destroyed;    // 동력원 파괴 여부

    // Reference
    private Animator anim;
    private Collider2D coll;

    private void Start()
    {
        UpdateVisual();
    }

    public void SetExposure(bool exposure)
    {
        isExposure = exposure;
    }

    public void TakeDamage(int damage)
    {
        if (IsDestroyed) return;
        if (state != CoreState.Exposure) return; // 활성 상태에서만 파괴 가능

        curHp -= damage;

        if (curHp <= 0)
        {
            curHp = 0;
            OnDestroyed();
        }
    }

    private void OnDestroyed()
    {
        state = CoreState.Destroyed;
        StopAllCoroutines();
        UpdateVisual();

        Debug.Log($"[CheongRyu Power Core] Destroyed");
    }

    private void UpdateVisual()
    {
        // 여기서 연출/콜라이더/이펙트 제어
        switch (state)
        {
            case CoreState.Hiding:
                // 비활성: 공격/피격 off
                coll.enabled = false;

                break;

            case CoreState.Exposure:
                // 공격/피격 on
                coll.enabled = true;

                break;

            case CoreState.Destroyed:
                // 파괴 연출
                coll.enabled = false;

                break;
        }
    }
}
