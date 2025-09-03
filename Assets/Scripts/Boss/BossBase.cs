using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 보스 부모 클래스
///  - 체력/난이도 배율/페이즈 전환/무적(I-Frame)/히트 처리
///  - 타깃 탐지(기본 : Tag : "Player")와 기본 상태머신
///  - 은신(광학미채) 시스템 훅(길달)
///  - 물리 갱신 훅 : 입력, AI는 Update, 실제 이동/힘은 Fixed에서 처리하게 분리함.
///  
/// 패턴 스케줄링/실행은 자식 클래스에서 구현
///  - StartNextPattern(), IsRunningPattern(), StopCurrrentPattern() 등을 오버라이드/구현
/// </summary>
public abstract class BossBase : MonoBehaviour
{
    // 설정
    [Header("HP/Phase")]
    [SerializeField] protected float maxHP = 100f;
    [Tooltip("페이즈 전환 임계치(절대값) 혹은 비율. 둘 다 세팅하면 절대값이 우선")]
    [SerializeField] protected float phase2ThresholdAbs = -1f;
    [Tooltip("절대값 미사용시 비율로 전환. 예 : 0.5f => Hp 50% 이하 진입 시 페이즈2")]
    [Range(0.05f, 0.95f)]
    [SerializeField] protected float phase2ThreshholdRatio = 0.5f;

    [Header("Difficulty Multipliers (보스별로 다름)")]
    [Tooltip("데미지/체력/쿨다운 등에 곱해 쓸 배율. 자식 클래스에서 접근해서 사용.")]
    [SerializeField] protected float hpMultiplier = 1f;
    [SerializeField] protected float damageMultiplier = 1f;
    [SerializeField] protected float cooldownMultiplier = 1f;

    public float DamageMultiplier => damageMultiplier;

    [Header("References")]
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator anim;
    [SerializeField] protected Transform target;

    [Header("Flags")]
    [Tooltip("보스가 은신 시스템을 쓰는지(길달 = True, 청류 = False). 미사용 보스는 그냥 무시")]
    [SerializeField] protected bool supportsStealth = false;
    [Tooltip("은신 중 피격 불가.")]
    [SerializeField] protected bool invulnerableWhileStealth = true;

    [Header("Tuning")]
    [Tooltip("피격 후 잠깐 무적(I-frame) 시간(초). 필요 없으면 0")]
    [SerializeField] protected float hitIFrame = 0.1f;
    [Tooltip("보스 아레나 경계. 필요시 자식에서 사용.")]
    [SerializeField] protected Bounds arenaBounds;

    // 상태
    protected float hp;

    protected bool isDead;
    protected bool inPhase2;
    protected bool isStealthed;         // 현재 은신 상태
    protected bool isInvulnerable;      // 일시 무적(페이즈 전환/컷씬/히트 I-Frame)
    protected bool isBusy;              // 패턴/연출 중 임시 락

    protected Transform cachedTransform;
    protected SpriteRenderer cachedSR;  // 플립/가시성 토글용

    // 자식 클래스의 패턴 진행 여부 보고 프로퍼티
    public bool IsRunningPattern { get; private set; }
    protected void SetRunningPattern(bool value) => IsRunningPattern = value;

    
    protected virtual void Awake()
    {
        cachedTransform = transform;
        if (!rb)    rb = GetComponent<Rigidbody2D>();
        if (!anim)  anim = GetComponent<Animator>();
        cachedSR = GetComponentInChildren<SpriteRenderer>();

        // Hp 초기화
        hp = Mathf.Max(1f, maxHP * Mathf.Max(0.01f, hpMultiplier));

        // 플레이어 탐지
        if (!target)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go) target = go.transform;
        }
    }

    protected virtual void OnEnable() { }

    protected virtual void OnDisable() { }

    protected virtual void Update()
    {
        if (isDead) return;

        // 페이즈 전환 체크
        TryEnterPhase2();

        // 패턴이 안 돌고 있으면 자식 클래스가 다음 패턴을 시작하도록 요청
        if (!IsRunningPattern && !isBusy)
        {
            StartNextPattern(); // 자식에서 구현 : 조건 검사 + 패턴 코루틴 시작
        }

        // 보스별 Update 로직 훅
        OnUpdateTick();
    }

    protected virtual void FixedUpdate()
    {
        if (isDead) return;

        // 실제 물리 이동/힘 적용
        OnFixedTick();
    }

    // 전투, 피격, 사망
    public virtual void TakeDamage(float rawDamage)
    {
        if (isDead) return;

        if (isInvulnerable) return;
        if (supportsStealth && isStealthed && invulnerableWhileStealth) return;

        float dmg = rawDamage * Mathf.Max(0.01f, damageMultiplier);
        hp -= dmg;

        if (hp <= 0f)
        {
            hp = 0f;
            Die();
            return;
        }

        // 피격 리액션
        OnHit(dmg);

        // I - Frame
        if (hitIFrame > 0f) StartCoroutine(Co_TempInvuln(hitIFrame));
    }

    protected virtual IEnumerator Co_TempInvuln(float sec)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(sec);
        isInvulnerable = false;
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        // 현재 패턴 정지
        StopCurrentPattern();

        // 연출
        isInvulnerable = true;
        anim?.SetTrigger("Die");

        // 정리
        OnDie();
    }

    // 페이즈 전환
    protected virtual void TryEnterPhase2()
    {
        if (inPhase2) return;

        float threshold = phase2ThresholdAbs > 0f ? phase2ThresholdAbs : maxHP * phase2ThreshholdRatio;

        if (hp <= threshold)
        {
            StartCoroutine(Co_PhaseChange());
        }
    }

    protected virtual IEnumerator Co_PhaseChange()
    {
        inPhase2 = true;
        isBusy = true;
        isInvulnerable = true;

        // 현재 패턴/행동 정지
        StopCurrentPattern();

        // 전환 연출 훅
        yield return OnPhaseChangeCutscene();

        isInvulnerable = false;
        isBusy = false;

        OnEnterPhase2();
    }

    // 은신
    public virtual void EnterStealth()
    {
        if (!supportsStealth) return;
        isStealthed = true;

        // 시각적 가시성 Off
        if (cachedSR) cachedSR.enabled = false;

        OnStealthEnter();
    }

    public virtual void ExitStealth()
    {
        if (!supportsStealth) return;
        isStealthed = false;

        if (cachedSR) cachedSR.enabled = true;

        OnStealthExit();
    }

    // Player의 현재 위치 조준
    public virtual Vector2 GetAimPoint() => target ? (Vector2)target.position : (Vector2)cachedTransform.position;

    // 영역 밖으로 나가지 않게 클램프 (필요시 자식에서 사용)
    protected void ClampInsideArena()
    {
        if (arenaBounds.size == Vector3.zero) return;
        var p = cachedTransform.position;
        p.x = Mathf.Clamp(p.x, arenaBounds.min.x, arenaBounds.max.x);
        p.y = Mathf.Clamp(p.y, arenaBounds.min.y, arenaBounds.max.y);
        cachedTransform.position = p;
    }

    // 다음 패턴 시작
    protected abstract void StartNextPattern();

    // 현재 패턴 강제 중지. 페이즈 전환/사망
    protected abstract void StopCurrentPattern();

    // 보스별 Update 로직 (플립, 추적, 대기)
    protected virtual void OnUpdateTick() { }

    // 보스별 FixedUpdate 로직 (물리 이동/힘 적용)
    protected virtual void OnFixedTick() { }

    // 피격 연출
    protected virtual void OnHit(float damageTaken) { }

    // 페이즈 전환 연출
    protected virtual IEnumerator OnPhaseChangeCutscene() { yield return null; }

    // 페이즈2 진입 초기화
    protected virtual void OnEnterPhase2() { }

    // 은신 시작 처리
    protected virtual void OnStealthEnter() { }

    // 은신 해제 처리
    protected virtual void OnStealthExit() { }

    // 사망 시 정리(드론/투사체 제거, 문 열기, 보상 스폰 등)
    protected virtual void OnDie() { }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        // 아레나 경계 시각화
        if (arenaBounds.size != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(arenaBounds.center, arenaBounds.size);
        }

        // 조준점 라인
        if (target)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
#endif
}

static class ListPool<T>
{
    private static readonly Stack<List<T>> pool = new();

    public static List<T> Get()
    {
        return pool.Count > 0 ? pool.Pop() : new List<T>();
    }

    public static void Release(List<T> list)
    {
        list.Clear();
        pool.Push(list);
    }
}