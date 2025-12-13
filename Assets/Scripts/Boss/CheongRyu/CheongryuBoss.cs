using UnityEngine;

public class CheongryuBoss : BossBase
{
    public enum CheongRyuPhase
    {
        // Phase 1
        Phase1_OneMonitor,
        Phase1_PowerExposure,
        Phase1_Regroup,

        Phase1_AltOneMonitor,
        Phase1_Repair,

        // Phase 2
        Phase2_TwoMonitor,
        Phase2_PowerExposure,
        Phase2_Regroup,

        Die
    }

    [Header(" === CheongRyu References === ")]
    [SerializeField] private Monitor monitor1;      // 기관총
    [SerializeField] private Monitor monitor2;      // 함포
    [SerializeField] private Monitor monitor3;      // 레일건
    [SerializeField] private PowerCore powerCore;   // 동력원

    [Header(" === CheongRyu Setting === ")]
    [SerializeField] private float powerExposureDuration = 4f; // 동력원 노출 시간
    [SerializeField] private float regroupDuration = 1f;       // 재정비 시간
    [SerializeField] private float repairDuration = 3f;        // 수리 시간
}
