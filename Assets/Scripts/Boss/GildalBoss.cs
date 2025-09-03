using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 길달 보스 구현
///     - 평상시 은신(피격X). 패턴 시작 직전 은신 해제 -> 공격 수행 -> 종료 후 재은신.
///     - 페이즈 1/2 별 패턴 테이블을 가중치 기반으로 선택.
///     - 쿨다운/선후딜/재은신 대기 등을 통합 관리.
///     - BossBase의 supportsStealth, invulnerableWhileStealthed 사용.
/// </summary>


public class GildalBoss : BossBase
{
    [Header("Phase Pattern Tables")]
    [Tooltip("1페이즈 패턴들(예 : Swing, Slam, DokkaebiOrb)")]
    [SerializeField] private List<AttackPatternSO> phase1Patterns = new();
    [Tooltip("2페이즈 패턴들(예 : DoubleSlash, LeapSlash, EnhanceDokkaebiOrb)")]
    [SerializeField] private List<AttackPatternSO> phase2Patterns = new();

    [Header("Stealth Cycle")]
    [Tooltip("패턴 시작 전의 은신 해제 예고 시간(셔더/사운드/텔레그래프")]
    [SerializeField] private float revealLeadTime = 0.15f;
    [Tooltip("패턴 종료 후 재은신 시간")]
    [SerializeField] private float reStealthDelay = 0.1f;

    [Header("Pacing / Idle")]
    [Tooltip("패턴 간 최소 유휴 시간")]
    [SerializeField] private Vector2 idleBetweenPatterns = new Vector2(0.2f, 0.5f);

    [Header("Collision / Renderer (은신 시 토글용.")]
    [SerializeField] private Collider2D[] hitCollidersToToggle;
    [SerializeField] private GameObject[] visualsToToggle;


    // State
    private Coroutine patternRoutine;
    private List<AttackPatternSO> activePatterns;                       // 현재 페이즈의 패턴 목록
    private readonly Dictionary<string, float> cooldownEnds = new();    // 패턴별 쿨다운 시간


    protected override void Awake()
    {
        base.Awake();

        // 은신 사용
        supportsStealth = true;
        invulnerableWhileStealth = true;

        // 1페이즈 테이블 시작
        activePatterns = phase1Patterns;

        // 전투 시작
        EnterStealth();
    }

    protected override void StartNextPattern()
    {
        // 이미 돌고 있으면 무시
        if (patternRoutine != null) return;

        // 패턴 후보 필터링 + 가중치 선택
        var pattern = PickNextPattern();
        if (pattern == null)
        {
            // 실행 가능한 패턴 없으면 잠시 대기
            StartCoroutine(Co_ShortIdleRetry());
            return;
        }

        patternRoutine = StartCoroutine(Co_RunPatternCycle(pattern));
    }

    protected override void StopCurrentPattern()
    {
        if (patternRoutine != null)
        {
            StopCoroutine(patternRoutine);
            patternRoutine = null;
        }
        SetRunningPattern(false);

        // 재은신
        if (supportsStealth && !isStealthed)
            EnterStealth();
    }

    private IEnumerator Co_ShortIdleRetry()
    {
        SetRunningPattern(false);
        yield return new WaitForSeconds(0.1f);
        StartNextPattern();
    }

    // 길달 루틴
    //  1) 대기 -> 2) 은신 해제 예고 -> 3) 패턴 코루틴 실행
    //  4) 재은신 대기 -> 5) 패턴 종료

    private IEnumerator Co_RunPatternCycle(AttackPatternSO pattern)
    {
        SetRunningPattern(true);

        // 1) 패턴 간 템포
        float idleWait = Random.Range(idleBetweenPatterns.x, idleBetweenPatterns.y);
        yield return new WaitForSeconds(idleWait);

        // 2) 은신 해제 예고
        if (supportsStealth)
        {
            // 연출 타임
            if (revealLeadTime > 0f) yield return new WaitForSeconds(revealLeadTime);
            ExitStealth();
        }

        // 3) 쿨다운 시작
        cooldownEnds[pattern.name] = Time.time + pattern.cooldown * Mathf.Max(0.01f, cooldownMultiplier);
        yield return StartCoroutine(pattern.Execute(this));

        // 4) 재은신
        if (supportsStealth)
        {
            if (reStealthDelay > 0f) yield return new WaitForSeconds(reStealthDelay);
            EnterStealth();
        }

        // 5) 후처리
        if (pattern.recoveryTime > 0f)
            yield return new WaitForSeconds(pattern.recoveryTime);

        // 종료
        patternRoutine = null;
        SetRunningPattern(false);
    }

    // 패턴 선택
    private AttackPatternSO PickNextPattern()
    {
        var now = Time.time;

        // 실행 가능한 후보 필터링(쿨다운/조건)
        var candidates = ListPool<AttackPatternSO>.Get();
        foreach (var p in activePatterns)
        {
            if (p == null) continue;

            // 쿨다운 체크
            if (cooldownEnds.TryGetValue(p.name, out float end) && now < end)
                continue;

            // 조건 판단
            if (!p.CanExecute(this)) continue;

            candidates.Add(p);
        }

        if (candidates.Count == 0)
        {
            ListPool<AttackPatternSO>.Release(candidates);

            return null;
        }

        // 가중치 합
        float sum = 0f;
        for (int i = 0; i < candidates.Count; i++)
            sum += Mathf.Max(0.0001f, candidates[i].weight);

        float r = Random.value * sum;
        for (int i = 0; i < candidates.Count; i++)
        {
            r -= Mathf.Max(0.0001f, candidates[i].weight);
            if (r <= 0f)
            {
                var picked = candidates[i];
                ListPool<AttackPatternSO>.Release(candidates);
                return picked;
            }
        }

        var fallback = candidates[Random.Range(0, candidates.Count)];
        ListPool<AttackPatternSO>.Release(candidates);

        return fallback;
    }

    // 페이즈 전환/훅
    protected override IEnumerator OnPhaseChangeCutscene()
    {
        // 변신 연출
        // 은신 유지 + 무적은 Base
        anim?.SetTrigger("PhaseChange");
        yield return new WaitForSeconds(1.0f);  // 필요시 타임라인/시네머신 연동
    }

    // 이동 AI 없음, 패턴 내에서 이동 수행.
    protected override void OnUpdateTick() { }
    protected override void OnFixedTick() { }

    protected override void OnHit(float damageTaken)
    {
        // 은신 중 Base에서 피격 무시
        // 피격 애니/사운드
        if (!isStealthed) anim?.SetTrigger("Hit");
    }

    // 은신/피격
    protected override void OnStealthEnter()
    {
        // 시각/피격 Off
        ToggleVisuals(false);
        ToggleHitColliders(false);

        // 셰이더 스위치/파티클 종료 등..
    }

    protected override void OnStealthExit()
    {
        // 시각/피격 On
        ToggleVisuals(true);
        ToggleHitColliders(true);

        // 이펙트 등..
    }

    private void ToggleHitColliders(bool on)
    {
        if (hitCollidersToToggle == null) return;

        foreach (var c in hitCollidersToToggle)
            if (c) c.enabled = on;
    }

    private void ToggleVisuals(bool on)
    {
        if (visualsToToggle == null) return;
        
        foreach (var go in visualsToToggle)
            if (go) go.SetActive(on);
    }
}
