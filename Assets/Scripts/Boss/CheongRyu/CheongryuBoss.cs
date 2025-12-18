using System.Collections;
using UnityEngine;

public class CheongryuBoss : BossBase
{
    public enum CheongRyuPhase
    {
        InActive,

        // Phase 1
        Phase1_OneMonitor,
        Phase1_PowerExposure,
        Phase1_Regroup,
        Phase1_Repair,

        // Phase 2
        Phase2_TwoMonitor,
        Phase2_PowerExposure,
        Phase2_Regroup,

        Die
    }
    [Header(" === CheongRyu === ")]
    [Header("CheongRyu References")]
    [SerializeField] private Monitor monitor1;      // 기관총
    [SerializeField] private Monitor monitor2;      // 함포
    [SerializeField] private Monitor monitor3;      // 레일건
    [SerializeField] private PowerCore powerCore;   // 동력원

    [Header("CheongRyu Setting")]
    [SerializeField] private float powerExposureDuration = 4f;  // 동력원 노출 시간
    [SerializeField] private float regroupDuration = 1f;        // 재정비 시간
    [SerializeField] private float repairDuration = 3f;         // 수리 시간
    private int nextMonitorID = -1;                             // One Monitor 패턴 모니터(1 or 3)

    [Header("RunTime")]
    [SerializeField] private CheongRyuPhase phase = CheongRyuPhase.InActive;
    private Coroutine phaseRoutine;

    private bool phase1_AltMonitor = false;


    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        StartCoroutine(Co_StartBattle());
    }

    public override IEnumerator Co_StartBattle()
    {
        if (phaseRoutine != null) yield break;
        yield return StartCoroutine(base.Co_StartBattle());

        phase = CheongRyuPhase.Phase1_OneMonitor;
        phaseRoutine = StartCoroutine(Co_RunPhase());
    }

    private IEnumerator Co_RunPhase()
    {
        while (phase != CheongRyuPhase.Die)
        {
            switch (phase)
            {
                case CheongRyuPhase.Phase1_OneMonitor:
                    yield return StartCoroutine(Co_Phase1_OneMonitor());
                    break;
                case CheongRyuPhase.Phase1_PowerExposure:
                    yield return StartCoroutine(Co_Phase1_PowerExposure());
                    break;
                case CheongRyuPhase.Phase1_Regroup:
                    yield return StartCoroutine(Co_Phase1_Regroup());
                    break;
                case CheongRyuPhase.Phase1_Repair:
                    yield return StartCoroutine(Co_Phase1_Repair());
                    break;
            }
        }
    }

    // ===== Phase 1 Coroutines =====

    // 모니터 활성화 ( 패턴 시작, 재정비 후 )
    private IEnumerator Co_Phase1_OneMonitor()
    {
        // 모니터 선택
        Debug.Log("[CheongRyu] Phase 1: One Monitor");
        Monitor target = phase1_AltMonitor ? monitor3 : monitor1;
        phase1_AltMonitor = !phase1_AltMonitor;

        // 모니터 공격 시작
        target.OnAttack(true);

        // 모니터 파괴 대기
        yield return new WaitUntil(() => target.IsDestroyed);

        // 모니터 공격 종료
        target.OnAttack(false);
        phase = CheongRyuPhase.Phase1_PowerExposure;
    }

    // 동력원 노출 ( 모니터 파괴 후 )
    private IEnumerator Co_Phase1_PowerExposure()   
    {
        // 노출
        Debug.Log("[CheongRyu] Phase 1: Power Exposure");
        powerCore.SetExposure(true);

        yield return new WaitForSeconds(powerExposureDuration);

        // 은폐
        powerCore.SetExposure(false);
        phase = CheongRyuPhase.Phase1_Regroup;
    }

    // 재정비 ( 동력원 노출 후 )
    private IEnumerator Co_Phase1_Regroup()
    {
        // 재정비 대기
        Debug.Log("[CheongRyu] Phase 1: Regroup");
        yield return new WaitForSeconds(regroupDuration);

        // 동력원 파괴시 페이즈 전환
        if (powerCore.IsDestroyed)
        {
            yield return StartCoroutine(Co_PhaseChange());
            phase = CheongRyuPhase.Phase2_TwoMonitor;
            yield break;
        }

        // 모니터 파괴 여부 체크
        bool aliveMonitor = !monitor1.IsDestroyed || !monitor3.IsDestroyed;
        if (aliveMonitor)
        {
            // 파괴되지 않은 모니터 활성화
            phase = CheongRyuPhase.Phase1_OneMonitor;
        }
        else
        {
            // 모니터 전부 파괴시 수리
            phase = CheongRyuPhase.Phase1_Repair;
        }
    }

    // 수리 ( 모니터 전부 파괴시 )
    private IEnumerator Co_Phase1_Repair()
    {
        Debug.Log("[CheongRyu] Phase 1: Repair");

        StartCoroutine(monitor1.Co_Repair(repairDuration));
        StartCoroutine(monitor3.Co_Repair(repairDuration));
        yield return new WaitForSeconds(repairDuration);

        phase = CheongRyuPhase.Phase1_OneMonitor;
    }


    protected override IEnumerator Co_PhaseChange()
    {
        yield return StartCoroutine(base.Co_PhaseChange());
        yield return null;
    }
}
