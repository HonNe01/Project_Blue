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
    public int playerLayer;
    public int playerAttackLayer;
    public int groundLayer;
    [SerializeField] private float[] floorHeights;
    public float pattern_Delay = 0.35f;
    [Tooltip("재은신 연출/대기 시간")]
    public float reStealth_Delay = 1f;
    [Header("Map Limits")]
    public float wallXMin = -20f;      // 벽 x좌표
    public float wallXMax = 20f;

    [Header(" === 1 Phase Patterns === ")]
    [Header("Swing")]
    public float swing_weight = 3f;         // 가중치
    public float swing_cooldowwn = 2.0f;    // 대기시간
    public float swing_preDelay = 0.2f;     // 선딜
    public float swing_postDelay = 0.4f;    // 후딜
    [Tooltip("Swing 히트 박스 오브젝트")]
    public GameObject swing_Hitbox;

    [Header("Slam")]
    public float slam_weight = 3f;
    public float slam_height = 5f;          // 점프 높이
    public float slam_cooldowwn = 2.0f;
    public float slam_preDelay = 0.2f;
    public float slam_postDelay = 0.4f;
    [Tooltip("Slam 히트 박스 오브젝트")]
    public GameObject slam_Hitbox;

    [Header("Dokkaebi Orb")]
    public float dokkaebiOrb_weight = 3f;
    public float dokkaebiOrb_cooldowwn = 2.0f;
    public float dokkaebiOrb_preDelay = 0.2f;
    public float dokkaebiOrb_postDelay = 0.4f;
    [Tooltip("Dokkaebi Orb Drone 프리펩")]
    public GameObject dronePrefab;

    [Header(" === 2 Phase Patterns === ")]
    [Header("Double Slash")]
    public float slash_weight = 3f;
    public float slash_cooldowwn = 2.0f;
    public float slash_preDelay = 0.2f;
    public float slash_postDelay = 0.4f;
    [Tooltip("Slash 히트 박스 오브젝트")]
    public GameObject slash_Hitbox;

    [Header("Jump Slash")]
    public float jumpSlash_weight = 3f;
    public float jumpSlash_height = 5f;          // 점프 높이
    public float jumpSlash_cooldowwn = 2.0f;
    public float jumpSlash_preDelay = 0.2f;
    public float jumpSlash_postDelay = 0.4f;
    [Tooltip("Slam 히트 박스 오브젝트")]
    public GameObject jumpSlash_Hitbox;

    [Header("Enhanced Dokkaebi Orb")]
    public float eDokkaebiOrb_weight = 3f;
    public float eDokkaebiOrb_cooldowwn = 2.0f;
    public float eDokkaebiOrb_preDelay = 0.2f;
    public float eDokkaebiOrb_postDelay = 0.4f;

    [Header(" === Special Patterns === ")]
    [Header("Famine / abundance")]
    public float[] specialHpThresholds = new float[] { 75f, 50f, 25f };
    public float special_preDelay = 0.2f;
    public float special_postDelay = 0.2f;
    public int baseDronesPerLayer = 2;
    public float spawnSpacing = 5f;
    public float spawnDelay = 0.1f;

    private bool[] specialUsed;
    private int specialIndex = -1;
    private bool isSpecial = false;

    [Header("References")]
    [Tooltip("길달 본체 스프라이트 (flipX 제어용)")]
    public SpriteRenderer sprite;
    public Collider2D coll;
    private Material stealthMat;

    // 길달 패턴 리스트
    private readonly List<BossPattern> phase1Patterns = new();
    private readonly List<BossPattern> phase2Patterns = new();

    protected override void Awake()
    {
        base.Awake();

        specialUsed = new bool[specialHpThresholds.Length];

        if (sprite == null) sprite = GetComponent<SpriteRenderer>();
        if (stealthMat == null) stealthMat = sprite.material;
        if (coll == null) coll = GetComponent<Collider2D>();
    }

    private void Start()
    {
        bossLayer = LayerMask.NameToLayer("Enemy");
        playerLayer = LayerMask.NameToLayer("Player");
        playerAttackLayer = LayerMask.NameToLayer("PlayerAttack");
        groundLayer = LayerMask.NameToLayer("Ground");

        // ---- 페이즈1 패턴 등록 (가중치/쿨타임/실행코루틴 연결) ----
        phase1Patterns.Add(new BossPattern {name = "Swing", 
                                            weight = swing_weight, 
                                            cooldown = swing_cooldowwn, 
                                            execute = () => Co_Swing() });
        swing_Hitbox.SetActive(false);
        phase1Patterns.Add(new BossPattern {name = "Slam", 
                                            weight = slam_weight, 
                                            cooldown = slam_cooldowwn, 
                                            execute = () => Co_Slam() });
        slam_Hitbox.SetActive(false);
        phase1Patterns.Add(new BossPattern {name = "DokkaebiOrb", 
                                            weight = dokkaebiOrb_cooldowwn, 
                                            cooldown = dokkaebiOrb_cooldowwn, 
                                            execute = () => Co_DokkaebiOrb() });

        // ---- 페이즈2 패턴 등록 ----
        phase2Patterns.Add(new BossPattern {name = "DoubleSlash", 
                                            weight = slash_weight, 
                                            cooldown = slash_cooldowwn, 
                                            execute = () => Co_DoubleSlash() });
        slash_Hitbox.SetActive(false);
        phase2Patterns.Add(new BossPattern {name = "JumpSlash", 
                                            weight = jumpSlash_weight, 
                                            cooldown = jumpSlash_cooldowwn, 
                                            execute = () => Co_JumpSlash() });
        jumpSlash_Hitbox.SetActive(false);
        phase2Patterns.Add(new BossPattern {name = "EnhanceDokkaebi", 
                                            weight = eDokkaebiOrb_weight, 
                                            cooldown = eDokkaebiOrb_cooldowwn, 
                                            execute = () => Co_EDokkaebiOrb() });

        // 시작은 은신 상태 비주얼로(피격 Off는 추후 레이어 매트릭스 적용)
        StartCoroutine(Co_DoStealth());
    }

    protected override IEnumerator Co_ChoosePattern()
    {
        if (phaseChange)
        {
            phaseChange = false;
            StartCoroutine(Co_PhaseChange());
            yield break;
        }

        // 특수패턴 확인
        if (isSpecial)
        {
            // 패턴 실행
            yield return StartCoroutine(Co_RunSpecial(specialIndex));

            // 상태 복구
            isSpecial = false;
            specialIndex = -1;
        }
        else
        {
            // 현재 페이즈 풀에서 패턴 선택
            var pool = inPhase2 ? phase2Patterns : phase1Patterns;
            var choose = ChooseNextPattern(pool);
            choose.lastUsedTime = Time.time;

            // 패턴 실행
            yield return StartCoroutine(choose.execute());
        }

        // 패턴 딜레이
        yield return new WaitForSeconds(pattern_Delay);
        state = BossState.Idle;
        curPatternCoroutine = null;
    }

    // 은신 기믹
    private IEnumerator Co_DoStealth()
    {
        // 충돌 판정 해제
        Physics2D.IgnoreLayerCollision(bossLayer, playerLayer, true);
        Physics2D.IgnoreLayerCollision(bossLayer, playerAttackLayer, true);

        // 은신 로직
        if (sprite != null)
        {
            float elapsed = 0f;
            while (elapsed < reStealth_Delay)
            {
                elapsed += Time.deltaTime;
                float lerp = Mathf.Clamp01(elapsed / reStealth_Delay);
                stealthMat.SetFloat("_StealthAmount", lerp);
                yield return null;
            }
        }
    }
    private IEnumerator Co_EndStealth()
    {
        // 은신 해제 로직
        if (sprite != null)
        {
            float elapsed = 0f;
            while (elapsed < reStealth_Delay)
            {
                elapsed += Time.deltaTime;
                float lerp = Mathf.Clamp01(elapsed / reStealth_Delay);
                stealthMat.SetFloat("_StealthAmount", 1- lerp);
                yield return null;
            }
        }

        // 피격 판정 설정
        Physics2D.IgnoreLayerCollision(bossLayer, playerLayer, false);
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

    private IEnumerator Co_MoveTo(Vector2 pos, float duration, float margin = 0.5f)
    {
        if (sprite == null || target == null) yield break;

        // 위치 보정
        float clampX = Mathf.Clamp(pos.x, wallXMin + margin, wallXMax - margin);
        pos = new Vector2(clampX, pos.y);

        // 이동
        float speed = Vector2.Distance(transform.position, pos) / duration;
        while (Vector2.Distance(transform.position, pos) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, pos, speed * Time.deltaTime);

            yield return null;
        }

        // 충돌 복구
        int playerLayer = LayerMask.NameToLayer("Player");
        int groundLayer = LayerMask.NameToLayer("Ground");

        Physics2D.IgnoreLayerCollision(bossLayer, playerLayer, false);
        Physics2D.IgnoreLayerCollision(bossLayer, groundLayer, false);

        // 위치 보정
        transform.position = pos;
    }

    private void MoveTo(float offsetX = 0f, float offsetY = 0f)
    {
        if (target == null) return;

        // 1) 좌우 오프셋
        float ranOffsetX = 0;
        if (offsetX != 0)
            ranOffsetX = Random.value < 0.5f ? -offsetX : offsetX;

        // 2) y좌표 보정
        float groundY = GetFloorY(target.position.y);

        // 3) 목표 계산
        float targetX = target.position.x + ranOffsetX;
        float targetY = groundY + offsetY;

        // 4) x좌표 보정
        if (targetX < wallXMin) targetX = wallXMin + Mathf.Abs(ranOffsetX * 1.2f);
        if (targetX > wallXMax) targetX = wallXMax - Mathf.Abs(ranOffsetX * 1.2f);

        // 4) 이동
        transform.position = new Vector2(targetX, targetY);
    }

    // Y값 보정
    private float GetFloorY(float playerY, bool isCenter = false)
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
        MoveTo(2);
        FlipX(swing_Hitbox);

        // 2) 은신 해제
        yield return StartCoroutine(Co_EndStealth());
        yield return new WaitForSeconds(swing_preDelay);    // 은신 해제 후 공격까지의 딜레이

        // 3) 공격
        anim?.SetTrigger("Swing");
        yield return null;  // 1프레임 대기 -> Animator의 state 갱신 대기
        float animLength = anim.GetCurrentAnimatorStateInfo(1).length;
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
        MoveTo(1.5f, slam_height);
        FlipX();

        // 2) 은신 해제
        anim?.SetTrigger("SlamPrep");
        yield return StartCoroutine(Co_EndStealth());
        yield return new WaitForSeconds(slam_preDelay);

        // 충돌 무시 (복구는 Co_MoveTo에서)
        int playerLayer = LayerMask.NameToLayer("Player");
        int groundLayer = LayerMask.NameToLayer("Ground");

        Physics2D.IgnoreLayerCollision(bossLayer, playerLayer, true);
        Physics2D.IgnoreLayerCollision(bossLayer, groundLayer, true);

        // 3) 공격
        anim?.SetTrigger("Slam");
        Vector2 dest = new Vector2(target.position.x + 1f, transform.position.y - slam_height);
        StartCoroutine(Co_MoveTo(dest, 0.2f));
        yield return null;  // 1프레임 대기 -> Animator의 state 갱신 대기
        float animLength = anim.GetCurrentAnimatorStateInfo(1).length;
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
        MoveTo(5f);
        FlipX();

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
        yield return new WaitForSeconds(dokkaebiOrb_postDelay);
        yield return StartCoroutine(Co_DoStealth());
    }

#if UNITY_EDITOR
    [ContextMenu("Test Damage 10")]
    public void TestTakeDamage()    // 테스트용 데미지 10
    {
        TakeDamage(10);
    }
#endif

    // 페이즈 변경
    protected override IEnumerator Co_PhaseChange()
    {
        Debug.Log("[Gildal] 페이즈 전환");
        state = BossState.Directing;

        // 1) 플레이어 근처로 이동
        MoveTo(5f);
        FlipX();

        // 2) 은신 해제
        yield return StartCoroutine(Co_EndStealth());
        Physics2D.IgnoreLayerCollision(bossLayer, playerAttackLayer, true);

        // 3) 페이즈 변경 연출
        anim?.SetTrigger("PhaseChange");
        yield return null;
        float animLength = anim.GetCurrentAnimatorStateInfo(1).length;
        yield return new WaitForSeconds(animLength);
        yield return null;

        // 4) Animator Layer 변경
        anim?.SetLayerWeight(2, 1f);
        anim?.SetLayerWeight(1, 0f);

        /* 5) 페이즈 시작 연출
        anim?.SetTrigger("PhaseChaange");
        yield return null;
        animLength = anim.GetCurrentAnimatorStateInfo(2).length;
        yield return new WaitForSeconds(animLength);
        yield return null;
        */

        // 6) 재은신
        yield return StartCoroutine(Co_DoStealth());
        Physics2D.IgnoreLayerCollision(bossLayer, playerAttackLayer, false);
        
        // 7) 페이즈 변경 종료
        inPhase2 = true;
        state = BossState.Idle;
        Debug.Log("[Gildal] 2페이즈 돌입");
    }

    // 2페이즈 패턴
    private IEnumerator Co_DoubleSlash()
    {
        Debug.Log("[Gildal] DoubleSlash");

        // 1) 플레이어 위치로 이동
        MoveTo(2f);
        FlipX(swing_Hitbox);

        // 2) 은신 해제
        yield return StartCoroutine(Co_EndStealth());
        yield return new WaitForSeconds(slash_preDelay);    // 은신 해제 후 공격까지의 딜레이

        // 3) 공격
        Physics2D.IgnoreLayerCollision(bossLayer, playerLayer, true);
        anim?.SetTrigger("Slash");
        yield return null;  // 1프레임 대기 -> Animator의 state 갱신 대기
        float animLength = anim.GetCurrentAnimatorStateInfo(2).length;
        yield return new WaitForSeconds(animLength);    // anim 끝날 때까지 대기
        Physics2D.IgnoreLayerCollision(bossLayer, playerLayer, false);

        // 4) 재은신
        yield return new WaitForSeconds(slash_postDelay);   // 공격 후 재은신까지의 딜레이
        yield return StartCoroutine(Co_DoStealth());

        yield return null;
    }

    public void OnAttackForward(float distance)
    {
        Vector3 dir = sprite.flipX ? Vector2.left : Vector2.right;

        Vector2 targetPos = transform.position + dir * distance;

        StartCoroutine(Co_MoveTo(targetPos, 0.2f));
    }

    public void OnSlashHitStart() { if (slash_Hitbox) slash_Hitbox.SetActive(true); }
    public void OnSlashHitEnd() { if (slash_Hitbox) slash_Hitbox.SetActive(false); }

    private IEnumerator Co_JumpSlash()
    {
        Debug.Log("[Gildal] JumpSlash");

        // 1) 플레이어 위치로 이동
        MoveTo(0, slam_height);
        FlipX();
        
        // 2) 은신 해제
        anim?.SetTrigger("JumpSlashPrep");
        yield return StartCoroutine(Co_EndStealth());
        yield return new WaitForSeconds(jumpSlash_preDelay);

        // 충돌 무시 (복구는 Co_MoveTo에서)
        Physics2D.IgnoreLayerCollision(bossLayer, playerLayer, true);

        // 3) 공격
        anim?.SetTrigger("JumpSlash");
        Vector2 dest = new Vector2(transform.position.x, transform.position.y - slam_height);
        StartCoroutine(Co_MoveTo(dest, 0.1f));
        yield return null;  // 1프레임 대기 -> Animator의 state 갱신 대기
        float animLength = anim.GetCurrentAnimatorStateInfo(2).length;
        yield return new WaitForSeconds(animLength);    // anim 끝날 때까지 대기

        // 3) 공격2
        Physics2D.IgnoreLayerCollision(bossLayer, playerLayer, true);
        anim?.SetTrigger("JumpSlash");
        yield return null;  // 1프레임 대기 -> Animator의 state 갱신 대기
        animLength = anim.GetCurrentAnimatorStateInfo(2).length;
        yield return new WaitForSeconds(animLength);    // anim 끝날 때까지 대기
        Physics2D.IgnoreLayerCollision(bossLayer, playerLayer, false);

        // 4) 재은신
        yield return new WaitForSeconds(jumpSlash_postDelay);
        yield return StartCoroutine(Co_DoStealth());
    }
    public void OnJumpSlashHitStart() { if (jumpSlash_Hitbox) jumpSlash_Hitbox.SetActive(true); }
    public void OnJumpSlashHitEnd() { if (jumpSlash_Hitbox) jumpSlash_Hitbox.SetActive(false); }

    private IEnumerator Co_EDokkaebiOrb()
    {
        Debug.Log("[Gildal] Enhance Dokkaeni Orb");

        // 1) 플레이어 근처로 이동
        MoveTo(5f);
        FlipX();
        
        // 2) 은신 해제
        yield return StartCoroutine(Co_EndStealth());
        yield return new WaitForSeconds(eDokkaebiOrb_preDelay);
        anim?.SetTrigger("DokkaebiOrb");

        // 3) 드론 3체 소환
        bool playerIsRight = target.position.x > transform.position.x;
        int sign = playerIsRight ? 1 : -1;

        for (int i = 0; i < 3; i++)
        {
            // 드론 위치 조금식 오프셋
            Vector2 spawnPos = transform.position + new Vector3(sign * (2f + i * 1.5f), 2f + i * 1.2f, 0);

            var droneObj = Instantiate(dronePrefab, spawnPos, Quaternion.identity);
            var drone = droneObj.GetComponent<DokkaebiOrbDrone>();

            // 드론 조작
            Vector2 droneTarget = new Vector2(target.position.x, target.position.y + 1f);
            StartCoroutine(drone.Co_DroneAuto(droneTarget));

            // 다음 드론 소환 딜레이
            yield return new WaitForSeconds(eDokkaebiOrb_preDelay);
        }

        // 4) 모션 대기
        float animLength = anim.GetCurrentAnimatorStateInfo(2).length;
        yield return new WaitForSeconds(animLength);    // anim 끝날 때까지 대기

        // 5) 재은신
        StartCoroutine(Co_DoStealth());
        yield return new WaitForSeconds(eDokkaebiOrb_postDelay);
    }

    new public void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        CheckSpecialTrigger();
    }

    // 특수 패턴
    private void CheckSpecialTrigger()
    {
        if (isSpecial) return;

        for (int i = 0; i < specialHpThresholds.Length; i++)
        {
            if (!specialUsed[i] && curHp <= specialHpThresholds[i])
            {
                isSpecial = true;
                specialIndex = i;

                break;
            }
        }
    }

    private IEnumerator Co_RunSpecial(int idx)  // 흉년, 풍년 패턴
    {
        specialUsed[idx] = true;

        // 0) 준비
        state = BossState.Directing;

        float stageCenterX = (wallXMin + wallXMax) * 0.5f;
        bool isOnRightWall = transform.position.x > stageCenterX;
        bool isHyungNyeon = !isOnRightWall; // 서쪽(왼쪽) 흉년

        // 1) 플레이어 근처로 이동
        MoveTo(3f);
        FlipX();

        // 2) 은신 해제
        yield return StartCoroutine(Co_EndStealth());
        yield return new WaitForSeconds(special_preDelay);
        
        // 3) 패턴 이동 연출
        anim?.SetTrigger("SpecialPrep1");
        if (inPhase2)
        {
            yield return null;  // 1프레임 대기 -> Animator의 state 갱신 대기
            float animLength = anim.GetCurrentAnimatorStateInfo(2).length;
            yield return new WaitForSeconds(animLength);    // anim 끝날 때까지 대기
        }
        else
        {
            yield return null;  // 1프레임 대기 -> Animator의 state 갱신 대기
            float animLength = anim.GetCurrentAnimatorStateInfo(1).length;
            yield return new WaitForSeconds(animLength);    // anim 끝날 때까지 대기
        }

        // 4) 벽 이동;
        float groundYWall = floorHeights[1];
        float wallX = isHyungNyeon ? wallXMin : wallXMax;
        transform.position = new Vector2(wallX, groundYWall);
        anim?.SetTrigger("SpecialPrep2");
        if (inPhase2)
        {
            yield return null;  // 1프레임 대기 -> Animator의 state 갱신 대기
            float animLength = anim.GetCurrentAnimatorStateInfo(2).length;
            yield return new WaitForSeconds(animLength);    // anim 끝날 때까지 대기
        }
        else
        {
            yield return null;  // 1프레임 대기 -> Animator의 state 갱신 대기
            float animLength = anim.GetCurrentAnimatorStateInfo(1).length;
            yield return new WaitForSeconds(animLength);    // anim 끝날 때까지 대기
        }

        // 4) 공격
        anim?.SetTrigger("SpecialAttack");
        if (isHyungNyeon)
            yield return StartCoroutine(Co_DoHyungNyeon());
        else
            yield return StartCoroutine(Co_DoPungNyeon());

        yield return StartCoroutine(Co_DoStealth());
    }

    // 각 층 중심 y좌표 반환
    private float[] GetFloorCenter()
    {
        if (floorHeights == null || floorHeights.Length == 0)
            return new float[0];

        // 층 개수
        float[] centers = new float[floorHeights.Length];

        for (int i = 0; i < floorHeights.Length; i++)
        {
            if (i < floorHeights.Length - 1)
            {
                // 현재 층 중간 값
                centers[i] = 0.5f * (floorHeights[i] + floorHeights[i + 1]);
            }
            else
            {
                if (floorHeights.Length >= 2)
                {
                    // 마지막 층 외삽
                    float prevGap = floorHeights[i] - floorHeights[i - 1];
                    centers[i] = floorHeights[i] + 0.5f * prevGap;
                }
                else
                {
                    centers[i] = floorHeights[i];
                }
            }
        }

        return centers;
    }

    private IEnumerator Co_DoPungNyeon()    // 두 층
    {
        Debug.Log("[Gildal] 풍년이로구나!");

        float[] centers = GetFloorCenter();

        // 1) 층 선택
        int idxA = Random.Range(0, centers.Length);
        int idxB = idxA;
        while (idxB == idxA)
            idxB = Random.Range(0, centers.Length);

        // 드론 배열
        DokkaebiOrbDrone[] drones = new DokkaebiOrbDrone[2];

        // 2) 드론 소환
        Vector2 spawnPosA = new Vector2(transform.position.x, centers[idxA]);
        var objA = Instantiate(dronePrefab, spawnPosA, Quaternion.identity);
        drones[0] = objA.GetComponent<DokkaebiOrbDrone>();

        Vector2 spawnPosB = new Vector2(transform.position.x, centers[idxB]);
        var objB = Instantiate(dronePrefab, spawnPosB, Quaternion.identity);
        drones[1] = objB.GetComponent<DokkaebiOrbDrone>();

        // 3) 은신 해제
        StartCoroutine(drones[0].Co_EndStealth());
        yield return StartCoroutine(drones[1].Co_EndStealth());

        // 4) 공격
        yield return new WaitForSeconds(dokkaebiOrb_preDelay);
        StartCoroutine(drones[0].FireWave(false));
        yield return StartCoroutine(drones[1].FireWave(false));

        yield return new WaitForSeconds(dokkaebiOrb_postDelay);
    }

    private IEnumerator Co_DoHyungNyeon()   // 한 층
    {
        Debug.Log("[Gildal] 흉년이로구나..");

        // 1) 층 선택
        float[] centers = GetFloorCenter();
        int floor = Random.Range(0, centers.Length);

        // 2) 드론 소환
        Vector2 spawnPos = new Vector2(transform.position.x, centers[floor]);
        var obj = Instantiate(dronePrefab, spawnPos, Quaternion.identity);
        var drone = obj.GetComponent<DokkaebiOrbDrone>();

        // 3) 은신해제
        yield return StartCoroutine(drone.Co_EndStealth());

        // 4) 공격
        yield return new WaitForSeconds(dokkaebiOrb_preDelay);
        yield return StartCoroutine(drone.FireWave(true));

        yield return new WaitForSeconds(dokkaebiOrb_postDelay);
    }
}
