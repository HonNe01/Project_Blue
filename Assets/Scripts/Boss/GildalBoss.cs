using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;


/// <summary>
/// [길달 클래스]
///     - 패턴 시작 전 : 패턴별 위치로 이동 후 은신 해제(EndStealth)
///     - 이동 완료 -> 은신 해제(피격 판정 On) -> 선딜레이 -> 공격 -> 후딜레이 -> 재은신(피격 판정 Off)
///     - 페이즈 1 / 2 별 패턴 테이블 선택
///     - 은신 무적은 추후 구현 예정
/// </summary>
public class GildalBoss : BossBase
{
    [Header("전체 패턴 딜레이")]
    [Tooltip("은신 해제 직후, 공격 전 선딜레이")]
    public float pre_Delay = 0.2f;
    [Tooltip("공격 후, 은신 돌입 직전 후딜레이")]
    public float post_Delay = 0.35f;
    [Tooltip("재은신 연출/대기 시간")]
    public float reStealth_Delay = 1f;

    [Header(" === 1 Phase Patterns === ")]
    [Header("Swing")]
    [Tooltip("Swing 공격 모션 시간")]
    public float swing_Duration = 0.9f;
    public float swing_weight = 3f;
    public float swing_cooldowwn = 2.0f;
    [Tooltip("Swing 히트 박스 오브젝트")]
    public GameObject swing_Hitbox;

    [Header("Slam")]
    [Tooltip("Slam 공격 모션 시간")]
    public float slam_Duration = 0.9f;
    public float slam_weight = 3f;
    public float slam_cooldowwn = 2.0f;
    [Tooltip("Slam 히트 박스 오브젝트")]
    public GameObject slam_Hitbox;

    [Header("Dokkaebi Orb")]
    [Tooltip("Dokkaebi Orb 공격 모션 시간")]
    public float dokkaebiOrb_Duration = 0.9f;
    public float dokkaebiOrb_weight = 3f;
    public float dokkaebiOrb_cooldowwn = 2.0f;
    [Tooltip("Dokkaebi Orb 히트 박스 오브젝트")]
    public GameObject dokkaebiOrb_Hitbox;

    [Header(" === 2 Phase Patterns === ")]

    [Header("References")]
    [Tooltip("길달 본체 스프라이트 (flipX 제어용)")]
    public SpriteRenderer sprite;

    // 길달 패턴 리스트
    private readonly List<BossPattern> phase1Patterns = new();
    private readonly List<BossPattern> phase2Patterns = new();
    private Transform _transform;

    private void Start()
    {
        _transform = transform;
        if (sprite == null) sprite = GetComponent<SpriteRenderer>();

        // ---- 페이즈1 패턴 등록 (가중치/쿨타임/실행코루틴 연결) ----
        phase1Patterns.Add(new BossPattern { name = "Swing", weight = swing_weight, cooldown = swing_cooldowwn, execute = () => Co_Swing() });
        //phase1Patterns.Add(new BossPattern { name = "Slam", weight = slam_weight, cooldown = slam_cooldowwn, execute = () => Co_Slam() });
        //phase1Patterns.Add(new BossPattern { name = "DokkaebiOrb", weight = dokkaebiOrb_cooldowwn, cooldown = dokkaebiOrb_cooldowwn, execute = () => Co_DokkaebiOrb() });

        // ---- 페이즈2 패턴 등록 ----
        //phase2Patterns.Add(new BossPattern { name = "DoubleSlash", weight = 3f, cooldown = 2.2f, execute = () => Co_DoubleSlash() });
        //phase2Patterns.Add(new BossPattern { name = "LeapSlash", weight = 2f, cooldown = 3.2f, execute = () => Co_LeapSlash() });
        //phase2Patterns.Add(new BossPattern { name = "EnhanceDokkaebi", weight = 2f, cooldown = 4.5f, execute = () => Co_EDokkaebiOrb() });

        // 시작은 은신 상태 비주얼로(피격 Off는 추후 레이어 매트릭스 적용)
        StartCoroutine(Co_DoStealth());
    }

    protected override IEnumerator Co_ChoosePattern()
    {
        // 현재 페이즈 풀에서 패턴 선택
        var pool = inPhase2 ? phase2Patterns : phase1Patterns;
        var choose = ChooseNextPattern(pool);
        choose.lastUsedTime = Time.time;

        // 위치 이동
        var attackStartPos = CalcPreAttackPosition(choose.name);
        MoveTo(attackStartPos);

        // 은신 해제
        yield return StartCoroutine(Co_EndStealth());

        // 패턴 실행
        yield return StartCoroutine(choose.execute());

        // 재은신
        yield return StartCoroutine(Co_DoStealth());

        state = BossState.Idle;
        curPatternCoroutine = null;
    }

    // 은신 기믹
    private IEnumerator Co_DoStealth()
    {
        // 현재는 단순 sprite 토글
        if (sprite != null) sprite.enabled = false;

        yield return new WaitForSeconds(reStealth_Delay);
    }

    private IEnumerator Co_EndStealth()
    {
        // 현재난 단순 sprite 토글
        if (sprite != null) sprite.enabled = true;

        yield return null;
    }

    private Vector2 CalcPreAttackPosition(string patternName)
    {
        if (target == null) return _transform.position;

        if (patternName == "Swing")
        {
            float playerX = target.position.x;
            float playerY = target.position.y;

            // 추후 player의 왼쪽, 오른쪽 선택 로직 추가 필요
            return new Vector2(playerX + 1.5f, playerY);
        }
        // 추후 다른 패턴의 이동 로직 분기 추가 필요.

        return _transform.position;
    }

    private void MoveTo(Vector2 pos)
    {
        if (sprite == null || target == null) return;

        // 좌표 이동
        _transform.position = pos;

        // FlipX ( 길달 FlipX = False는 왼쪽 )
        bool playerIsRight = target.position.x > _transform.position.x;
        sprite.flipX = playerIsRight;
    }

    // 1페이즈 패턴
    private IEnumerator Co_Swing()
    {
        Debug.Log("[Gildal] Swing");

        yield return new WaitForSeconds(pre_Delay);

        anim?.SetTrigger("Swing");

        if (swing_Hitbox) swing_Hitbox.SetActive(true);
        yield return new WaitForSeconds(swing_Duration);
        if (swing_Hitbox) swing_Hitbox.SetActive(false);

        yield return new WaitForSeconds(post_Delay);
    }

    private IEnumerator Co_Slam()
    {
        Debug.Log("[Gildal] Slam");

        yield return null;
    }

    private IEnumerator Co_DokkaebiOrb()
    {
        Debug.Log("[Gildal] Dokkaebi Orb");

        yield return null;
    }

    // 2페이즈 패턴
    private IEnumerator Co_DoubleSlash()
    {
        Debug.Log("[Gildal] DoubleSlash");

        yield return null;
    }

    private IEnumerator Co_LeapSlash()
    {
        Debug.Log("[Gildal] LeapSlash");

        yield return null;
    }

    private IEnumerator Co_EDokkaebiOrb()
    {
        Debug.Log("[Gildal] Enhance Dokkaeni Orb");

        yield return null;
    }
}
