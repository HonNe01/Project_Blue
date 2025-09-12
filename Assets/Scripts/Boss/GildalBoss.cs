using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// [길달 클래스]
///     - 패턴 시작 전 : 패턴별 위치로 이동 후 은신 해제(EndStealth)
///     - 이동 완료 -> 은신 해제(피격 판정 On) -> 선딜레이 -> 공격 -> 후딜레이 -> 재은신(피격 판정 Off)
///     - 페이즈 1 / 2 별 패턴 테이블 선택
///     - 은신 중 무적
/// </summary>
public class GildalBoss : BossBase
{
    [Header(" === Gildal Boss === ")]
    [Header("All Pattern Setting")]
    public int bossLayer;
    public int playerAttackLayer;
    [SerializeField] private float[] floorHeights;
    [Tooltip("은신 해제 직후, 공격 전 선딜레이")]
    public float pre_Delay = 0.2f;
    [Tooltip("공격 후, 은신 돌입 직전 후딜레이")]
    public float post_Delay = 0.35f;
    [Tooltip("재은신 연출/대기 시간")]
    public float reStealth_Delay = 1f;

    [Header(" === 1 Phase Patterns === ")]
    [Header("Swing")]
    [Tooltip("Swing 가중치")]
    public float swing_weight = 3f;
    public float swing_cooldowwn = 2.0f;
    public float swing_preDelay = 0.2f;
    public float swing_postDelay = 0.4f;
    [Tooltip("Swing 히트 박스 오브젝트")]
    public GameObject swing_Hitbox;

    [Header("Slam")]
    [Tooltip("Slam 가중치")]
    public float slam_weight = 3f;
    public float slam_height = 5f;
    public float slam_cooldowwn = 2.0f;
    public float slam_preDelay = 0.2f;
    public float slam_postDelay = 0.4f;
    [Tooltip("Slam 히트 박스 오브젝트")]
    public GameObject slam_Hitbox;

    [Header("Dokkaebi Orb")]
    [Tooltip("Dokkaebi Orb 가중치")]
    public float dokkaebiOrb_weight = 3f;
    public float dokkaebiOrb_cooldowwn = 2.0f;
    public float dokkaebiOrb_preDelay = 0.2f;
    public float dokkaebiOrb_postDelay = 0.4f;
    [Tooltip("Dokkaebi Orb Drone 프리펩")]
    public GameObject dronePrefab;

    [Header(" === 2 Phase Patterns === ")]
    [Header("Double Slash")]
    [Header("Slam Slash")]
    [Header("Enhanced Dokkaebi Orb")]

    [Header("References")]
    [Tooltip("길달 본체 스프라이트 (flipX 제어용)")]
    public SpriteRenderer sprite;
    public Collider2D coll;

    // 길달 패턴 리스트
    private readonly List<BossPattern> phase1Patterns = new();
    private readonly List<BossPattern> phase2Patterns = new();

    private void Start()
    {
        if (sprite == null) sprite = GetComponent<SpriteRenderer>();
        if (coll == null) coll = GetComponent<Collider2D>();
        bossLayer = LayerMask.NameToLayer("Enemy");
        playerAttackLayer = LayerMask.NameToLayer("PlayerAttack");

        // ---- 페이즈1 패턴 등록 (가중치/쿨타임/실행코루틴 연결) ----
        phase1Patterns.Add(new BossPattern { name = "Swing", weight = swing_weight, cooldown = swing_cooldowwn, execute = () => Co_Swing() });
        swing_Hitbox.SetActive(false);
        phase1Patterns.Add(new BossPattern { name = "Slam", weight = slam_weight, cooldown = slam_cooldowwn, execute = () => Co_Slam() });
        slam_Hitbox.SetActive(false);
        phase1Patterns.Add(new BossPattern { name = "DokkaebiOrb", weight = dokkaebiOrb_cooldowwn, cooldown = dokkaebiOrb_cooldowwn, execute = () => Co_DokkaebiOrb() });

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

        // 패턴 실행
        yield return StartCoroutine(choose.execute());

        state = BossState.Idle;
        curPatternCoroutine = null;
    }

    // 은신 기믹
    private IEnumerator Co_DoStealth()
    {
        // 피격 판정 해제
        Physics2D.IgnoreLayerCollision(bossLayer, playerAttackLayer, true);

        // 은신 로직
        if (sprite != null)
        {
            float elapsed = 0f;
            Color c = sprite.color;

            while (elapsed < reStealth_Delay)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / reStealth_Delay);
                c.a = Mathf.Lerp(1f, 0f, t);
                sprite.color = c;
                yield return null;
            }

            c.a = 0f;
            sprite.color = c;
        }
    }

    private IEnumerator Co_EndStealth()
    {
        // 은신 해제 로직
        if (sprite != null)
        {
            float elapsed = 0f;
            Color c = sprite.color;

            while (elapsed < reStealth_Delay)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / reStealth_Delay);
                c.a = Mathf.Lerp(0f, 1f, t);
                sprite.color = c;
                yield return null;
            }

            c.a = 1f;
            sprite.color = c;
        }

        // 피격 판정 설정
        Physics2D.IgnoreLayerCollision(bossLayer, playerAttackLayer, false);
    }

    // 좌우 반전
    private void FlipX(GameObject hitbox = null)
    {
        if (sprite == null || target == null) return;

        // FlipX ( 길달 FlipX = true는 왼쪽, -1 )
        bool playerIsRight = target.position.x > transform.position.x;
        sprite.flipX = !playerIsRight;

        if (hitbox != null)
        {
            int sign = playerIsRight ? 1 : -1;
            hitbox.transform.localScale = new Vector3(sign, 1, 1);
        }
    }

    private IEnumerator Co_MoveTo(Vector2 pos, float duration)
    {
        if (sprite == null || target == null) yield break;

        // B : MoveTowards (속도 일정)
        float speed = Vector2.Distance(transform.position, pos) / duration;
        while (Vector2.Distance(transform.position, pos) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, pos, speed * Time.deltaTime);

            yield return null;
        }

        // 충돌 복구
        if (coll != null) coll.isTrigger = false;

        // 위치 보정
        transform.position = pos;
    }

    // Y값 보정
    private float GetFloorY(float playerY)
    {
        float chosen = floorHeights[0];

        for (int i = 0; i < floorHeights.Length; i++)
        {
            if (playerY < floorHeights[i])
            {
                chosen = i == 0 ? floorHeights[0] : floorHeights[i - 1];

                return chosen;
            }
        }

        return floorHeights[floorHeights.Length - 1];
    }

    // 1페이즈 패턴
    private IEnumerator Co_Swing()
    {
        Debug.Log("[Gildal] Swing");

        // 1) 플레이어 위치로 이동
        if (target != null)
        {
            // 플레이어의 왼쪽/오른쪽 판단
            int offsetX = Random.value < 0.5f ? -1 : 1;

            // 플레이어의 현재 층으로 y좌표 보정
            float groundY = GetFloorY(target.position.y);

            // 길달 이동
            Vector2 swing_destination = new Vector2(target.position.x + offsetX * 2f, groundY);
            transform.position = swing_destination;
            FlipX(swing_Hitbox);
        }

        // 2) 은신 해제
        yield return StartCoroutine(Co_EndStealth());
        yield return new WaitForSeconds(swing_preDelay);    // 은신 해제 후 공격까지의 딜레이

        // 3) 공격
        anim?.SetTrigger("Swing");
        yield return null;  // 1프레임 대기 -> Animator의 state 갱신 대기
        float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);    // anim 끝날 때까지 대기

        // 4) 재은신
        yield return new WaitForSeconds(swing_postDelay);   // 공격 후 재은신까지의 딜레이
        yield return StartCoroutine(Co_DoStealth());
    }

    public void OnSwingHitStart() { if (swing_Hitbox) swing_Hitbox.SetActive(true); }
    public void OnSwingHitEnd() { if (swing_Hitbox) swing_Hitbox.SetActive(false); }

    private IEnumerator Co_Slam()
    {
        Debug.Log("[Gildal] Slam");

        // 1) 플레이어 위치로 이동
        if (target != null)
        {
            // 플레이어의 왼쪽/오른쪽 판단
            int offsetX = Random.value < 0.5f ? -1 : 1;

            // 플레이어의 현재 층으로 y좌표 보정
            float groundY = GetFloorY(target.position.y);

            // 길달 이동
            Vector2 swing_destination = new Vector2(target.position.x + offsetX * 1.5f, groundY + slam_height);
            transform.position = swing_destination;
            FlipX();
        }

        // 2) 은신 해제
        anim?.SetTrigger("SlamPrep");
        yield return StartCoroutine(Co_EndStealth());
        yield return new WaitForSeconds(slam_preDelay);

        // 충돌 무시 (복구는 Co_MoveTo에서)
        if (coll != null) coll.isTrigger = true;

        // 3) 공격
        anim?.SetTrigger("Slam");
        Vector2 dest = new Vector2(transform.position.x, transform.position.y - slam_height);
        StartCoroutine(Co_MoveTo(dest, 0.2f));
        yield return null;  // 1프레임 대기 -> Animator의 state 갱신 대기
        float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);    // anim 끝날 때까지 대기

        // 4) 재은신
        yield return new WaitForSeconds(slam_postDelay);
        yield return StartCoroutine(Co_DoStealth());
    }
    public void OnSlamHitStart() { if (slam_Hitbox) slam_Hitbox.SetActive(true); }
    public void OnSlamHitEnd() { if (slam_Hitbox) slam_Hitbox.SetActive(false); }

    private IEnumerator Co_DokkaebiOrb()
    {
        Debug.Log("[Gildal] Dokkaebi Orb");

        // 1) 플레이어 근처로 이동
        if (target != null)
        {
            // 플레이어의 왼쪽/오른쪽 판단
            int offsetX = Random.value < 0.5f ? -1 : 1;

            // 플레이어의 현재 층으로 y좌표 보정
            float groundY = GetFloorY(target.position.y);

            // 길달 이동
            Vector2 dokkaebiOrb_Destination = new Vector2(target.position.x + offsetX * 5f, groundY);
            transform.position = dokkaebiOrb_Destination;
            FlipX();
        }

        // 2) 은신 해제
        yield return StartCoroutine(Co_EndStealth());
        yield return new WaitForSeconds(dokkaebiOrb_preDelay);
        anim?.SetTrigger("DokkaebiOrb");

        // 3) 드론 소환
        bool playerIsRight = target.position.x > transform.position.x;
        int sign = playerIsRight ? 1 : -1;
        Vector2 spawnPos = transform.position + new Vector3(sign * 2f, 2f, 0);

        var droneObj = Instantiate(dronePrefab, spawnPos, Quaternion.identity);
        var drone = droneObj.GetComponent<DokkaebiOrbDrone>();
        yield return StartCoroutine(drone.Co_EndStealth());

        // 5) 공격 명령
        Vector2 droneTarget = new Vector2(target.position.x, target.position.y + 1f);
        drone.FireOrb(droneTarget);
        
        // 6) 재은신
        StartCoroutine(Co_DoStealth());
        yield return new WaitForSeconds(dokkaebiOrb_postDelay);
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
