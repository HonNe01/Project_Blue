using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// [보스 베이스 클래스]
///     - 체력 / 페이즈 전환 / I-Frame / 사망 처리
///     - 기본 FSM 상태 관리 ( Directing -> Idle -> ChoosePattern -> Attacking -> PhaseChange -> Die )
///     - 공통 패턴 시스템 (BossPattern) 제공 : 가중치 + 쿨타임 기반
///     - 자식 클래스는 Co_ChoosePattern()과 패턴 코루틴 등의 추가 구현 필요
/// </summary>
public abstract class BossBase : MonoBehaviour
{
    // Enum 상태 정의
    public enum BossState { Idle, ChoosePattern, Attacking, Directing, Sturn, Die }

    // 패턴 클래스
    [System.Serializable]
    public class BossPattern
    {
        public string name;     // 패턴 이름
        public float weight;    // 패턴 가중치
        public float cooldown;  // 패턴 쿨타임
        [HideInInspector] public float lastUsedTime = -999f;    // 마지막 사용 시간
        public System.Func<IEnumerator> execute;                // 실행할 패턴 함수(자식 클래스에서 등록)
    }


    private List<BossPattern> phase1Patterns = new List<BossPattern>();
    private List<BossPattern> phase2Patterns = new List<BossPattern>();

    [Header(" === Boss Base === ")]
    [Header("Boss Base Setting")]
    public float maxHp = 100f;                  // 최대 HP
    [Range(0f, 1f)] public float phase2ThresholdRatio = 0.5f;   // 페이즈 전환 체력 비율(0.5f = 50%)
    public float iFrameDuration = 0.5f;         // 피격 시 무적 시간 (초)
    public Animator anim;

    [Header("Boss Base Reference")]
    public Transform target;                    // 플레이어(Tag:Player)

    [Header("Boss Base State")]
    public BossState state = BossState.Directing;    // 보스 상태
    public float curHp;                         // 현재 체력
    public bool phaseChange = false;            // 페이즈 변경
    public bool inPhase2 = false;               // 페이즈 상태
    public bool isSturn = false;
    public bool isDie = false;

    // 현재 실행 중인 패턴
    protected Coroutine curPatternCoroutine;
    // I - Frame 플래그
    private bool isInvulnerable = false;


    // ===================== Unity 생명주기 =====================
    protected virtual void Awake()
    {
        // 체력 초기화
        curHp = maxHp;

        // 타겟 초기화
        if (!target)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) target = go.transform;
        }
    }

    private void Update()
    {
        switch (state)
        {
            case BossState.Idle:
                // 패턴 선택으로 이동
                state = BossState.ChoosePattern;

                break;
            case BossState.ChoosePattern:
                // 패턴 선택
                StartPattern();

                break;
            case BossState.Attacking:
                // 패턴 실행 중

                break;
            case BossState.Directing:
                // 페이즈 전환
                
                break;
            case BossState.Sturn:
                // 스턴
                if (!isSturn)
                {
                    StopAllCoroutines();
                    StartCoroutine(Co_Sturn());
                }

                break;
            case BossState.Die:
                // 사망
                Die();

                break;
        }
    }


    // ===================== [전투 처리] =====================
    // 전투 시작
    public virtual IEnumerator Co_StartBattle()
    {
        Debug.Log("[Boss] Battle Start");

        yield return null;
    }

    /// <summary>
    /// 보스 전투 관련
    ///     - I-Frame 적용
    ///     - 체력 감소 / 사망 체크
    ///     - 페이즈 전환 체크
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        if (state == BossState.Die) return;
        if (isInvulnerable) return;     // I-Frame 중 무시

        curHp -= damage;

        // I-Frame
        if (isInvulnerable == false)
            StartCoroutine(Co_IFrame());

        // 사망 체크
        if (curHp <= 0f)
        {
            curHp = 0f;
            state = BossState.Die;

            return;
        }

        // 페이즈 체크
        if (!inPhase2 && curHp <= maxHp * phase2ThresholdRatio)
        {
            phaseChange = true;
        }
    }

    /// <summary>
    /// I-Frame
    ///     - 피격 시 일정 시간 무적
    /// </summary>
    private IEnumerator Co_IFrame()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(iFrameDuration);
        isInvulnerable = false;
    }

    protected virtual IEnumerator Co_Sturn()
    {
        Debug.Log("[BossBase] Boss Sturn");
        isSturn = true;

        yield return null;
    }

    /// <summary>
    /// 사망 처리
    ///     - 패턴 중단
    ///     - Die 연출
    /// </summary>
    private void Die()
    {
        if (isDie) return;
        Debug.Log("[Boss] 보스 사망");

        isDie = true;
        StopPattern();
        StopAllCoroutines();
        anim?.SetTrigger("Die");
    }

    public void BossDestroy()
    {
        // 보스 아이템 드랍...


        // 보스 파괴
        Destroy(gameObject);
    }

    /// <summary>
    /// 페이즈 전환
    ///     - 공통 구조 ( 자식이 Override로 구현 )
    /// </summary>
    protected virtual IEnumerator Co_PhaseChange()
    {
        Debug.Log("[Boss] 페이즈 전환");

        state = BossState.Directing;
        anim?.SetTrigger("PhaseChange");

        // 연출 대기
        yield return new WaitForSeconds(2f);

        inPhase2 = true;
        state = BossState.Idle;
    }


    // ===================== 패턴 관리 =====================
    // 패턴 시작
    protected void StartPattern()
    {
        if (curPatternCoroutine != null) return;

        curPatternCoroutine = StartCoroutine(Co_ChoosePattern());
    }

    // 패턴 중단
    protected void StopPattern()
    {
        if (curPatternCoroutine != null)
        {
            StopCoroutine(curPatternCoroutine);
            curPatternCoroutine = null;
        }
    }

    /// <summary>
    /// 패턴 선택 코루틴
    ///     - 가중치 / 쿨타임 기반
    ///     - 선택 패턴 실행 후 Idle 복귀
    /// </summary>
    protected virtual IEnumerator Co_ChoosePattern()
    {
        // 현재 페이즈의 패턴 풀 가져오기
        var pool = inPhase2 ? phase2Patterns : phase1Patterns;

        // 패턴 선택
        var choose = ChooseNextPattern(pool);
        choose.lastUsedTime = Time.time;

        // 패턴 실행
        state = BossState.Attacking;
        yield return StartCoroutine(choose.execute());

        // 패턴 종료
        curPatternCoroutine = null;
        state = BossState.Idle;
    }

    /// <summary>
    /// 패턴 선택 알고리즘
    ///     - 쿨타임 끝난 패턴 필터링
    ///     - 가중치 랜덤으로 선택
    ///     - 전부 쿨타임이면 모든 패턴에서 선택
    /// </summary>
    protected BossPattern ChooseNextPattern(List<BossPattern> patterns)
    {
        float now = Time.time;
        List<BossPattern> candidates = new List<BossPattern>(); // 패턴 리스트 나열
        float totalWeight = 0f;

        // 후보 필터링
        foreach (var p in patterns)
        {
            if (now - p.lastUsedTime >= p.cooldown)
            {
                candidates.Add(p);
                totalWeight += p.weight;
            }
        }

        // 전부 쿨타임이면 전체 풀에서 사용
        if (candidates.Count == 0)
        {
            candidates.AddRange(patterns);
            totalWeight = 0f;
            foreach (var p in candidates) totalWeight += p.weight;
        }

        // 가중치 랜덤 추첨
        float roll = Random.Range(0, totalWeight);
        float acc = 0f;
        foreach (var p in candidates)
        {
            acc += p.weight;
            if (roll <= acc) return p;
        }

        return candidates[0];
    }
}