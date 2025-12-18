using System.Collections;
using UnityEngine;

public class Monitor : MonoBehaviour, IDamageable
{
    public enum MonitorState
    {
        InActive,   // 비활성
        Active,     // 공격
        Destroyed,  // 파괴
        Repairing   // 수리
    }
    public enum MonitorType
    {
        Gun,        // 기관총 (패턴 1)
        Cannon,     // 함포   (패턴 3, 4)
        Railgun     // 레일건 (패턴 2)
    }


    [Header(" === Monitor Info === ")]
    [SerializeField] private MonitorType type = MonitorType.Gun;


    [Header(" === Monitor State === ")]
    [SerializeField] private MonitorState state = MonitorState.InActive;
    public float maxHp = 20;
    public float curHp = 20;

    public bool IsDestroyed => state == MonitorState.Destroyed;
    public bool IsActive => state == MonitorState.Active;
    public MonitorType Type => type;


    // Reference
    private Animator anim;
    private Collider2D coll;

    private void Awake()
    {
        curHp = maxHp;

        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
    }

    private void Start()
    {
        UpdateVisual();
    }

    public void OnAttack(bool on)
    {
        if (IsDestroyed) return;

        if (on)
        {
            if (state == MonitorState.Active) return;
            state = MonitorState.Active;
            Debug.Log($"[CheongRyu Monitor] {type} Active");

            switch (type)
            {
                case MonitorType.Gun:
                    if (on) StartCoroutine(Co_Gun());
                    break;
                case MonitorType.Cannon:
                    if (on) StartCoroutine(Co_Cannon());
                    break;
                case MonitorType.Railgun:
                    if (on) StartCoroutine(Co_Railgun());
                    break;
            }
        }
        else
        {
            state = MonitorState.InActive;
            StopAllCoroutines();
        }

        UpdateVisual();
    }

    private IEnumerator Co_Gun()
    {
        yield return null;
    }

    private IEnumerator Co_Cannon()
    {
        yield return null;
    }

    private IEnumerator Co_Railgun()
    {
        yield return null;
    }

    public IEnumerator Co_Repair(float repairTime = 1f)
    {
        if (state != MonitorState.Destroyed) yield break;

        state = MonitorState.Repairing;
        UpdateVisual();

        yield return new WaitForSeconds(repairTime);

        curHp = maxHp;
        state = MonitorState.InActive;
        UpdateVisual();

        Debug.Log($"[CheongRyu Monitor] {type} Repaired");
    }

    public void TakeDamage(float damage)
    {
        if (IsDestroyed) return;
        if (state != MonitorState.Active) return; // 활성 상태에서만 파괴 가능

        curHp -= damage;

        if (curHp <= 0)
        {
            curHp = 0;
            OnDestroyed();
        }
    }

    private void OnDestroyed()
    {
        state = MonitorState.Destroyed;
        StopAllCoroutines();
        UpdateVisual();

        Debug.Log($"[CheongRyu Monitor] {type} Destroyed");
    }

    private void UpdateVisual()
    {
        // 여기서 연출/콜라이더/이펙트 제어
        switch (state)
        {
            case MonitorState.InActive:
                // 비활성: 공격/피격 off
                anim.Play("InActive");
                coll.enabled = false;

                break;

            case MonitorState.Active:
                // 공격/피격 on
                anim.Play("Active");
                coll.enabled = true;

                break;

            case MonitorState.Destroyed:
                // 파괴 연출
                anim.Play("Destroyed");
                coll.enabled = false;

                break;

            case MonitorState.Repairing:
                // 수리 연출
                anim.Play("Repairing");

                break;
        }
    }
}
