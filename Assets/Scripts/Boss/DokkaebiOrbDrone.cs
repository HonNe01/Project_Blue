using System.Collections;
using UnityEngine;

public class DokkaebiOrbDrone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider2D bodyColl;
    [SerializeField] private Collider2D expColl;
    [SerializeField] private GameObject wavePrefab;

    private SpriteRenderer sprite;
    private Animator anim;


    [Header("Orb Setting")]
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private float chaseRatio = 0.6f;
    [SerializeField] private float dashSpeed = 10f;
    public Vector2 OrbOffset;
    private Vector2 lastDir = Vector2.right;
    
    private int baseDamage = 1;

    private bool isChase = true;
    private bool isExploded = false;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (expColl) expColl.enabled = false;
        if (bodyColl) bodyColl.enabled = true;
    }

    // 드론 자동 조작: 은신 해제 -> 돌진/폭발
    public IEnumerator Co_DroneAuto(Transform player)
    {
        yield return StartCoroutine(Co_EndStealth());
        yield return null;

        StartCoroutine(Co_FireOrb(player));
    }

    // 은신
    public IEnumerator Co_DoStealth()
    {
        // 은신 로직
        anim.SetTrigger("Stealth");
        SoundManager.instance.PlaySFX(SoundManager.SFX.Stealth_Gidal);
        yield return null;

        float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);    // anim 끝날 때까지 대기

        Destroy(gameObject);
    }

    public IEnumerator Co_EndStealth()
    {
        // 은신 해제 로직
        SoundManager.instance.PlaySFX(SoundManager.SFX.Stealth_Gidal);
        yield return null;

        float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);    // anim 끝날 때까지 대기
    }

    public IEnumerator Co_FireOrb(Transform player)
    {
        isChase = true;
        float total = lifeTime;
        float chaseTime = Mathf.Clamp01(chaseRatio) * total;
        float coastTime = Mathf.Max(0f, total - chaseTime);

        float t = 0f;

        // 1) Chase : 플레이어 추적
        while (!isExploded && t < chaseTime)
        {
            Vector2 toTarget;
            if (player != null)
            {
                toTarget = (Vector2)player.position - (Vector2)transform.position + OrbOffset;
            }
            else
            {
                toTarget = lastDir.sqrMagnitude > 0.0001f ? lastDir : Vector2.right;
            }

            if (toTarget.sqrMagnitude > 0.0001f)
            {
                lastDir = toTarget.normalized;
            }

            transform.Translate(lastDir * dashSpeed * Time.deltaTime, Space.World);
            t += Time.deltaTime;
            yield return null;
        }

        // 2) Coast : 돌진
        isChase = false;
        float coastElapsed = 0f;
        while (!isExploded && coastElapsed < coastTime)
        {
            transform.Translate(lastDir * dashSpeed * Time.deltaTime, Space.World);
            coastElapsed += Time.deltaTime;
            yield return null;
        }

        // 3) 폭발
        if (!isExploded)
        {
            yield return StartCoroutine(Co_Explosion());
        }
    }

    private IEnumerator Co_Explosion(bool hasDamage = false)
    {
        if (isExploded) yield break;
        isExploded = true;

        // 콜라이더 전환
        if (bodyColl) bodyColl.enabled = false;
        if (expColl && !hasDamage) expColl.enabled = true;

        // 폭발 애니
        anim.SetTrigger("Explosion");
        yield return null;
        float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);    // anim 끝날 때까지 대기

        // 폭발 모션 이후 파괴
        Destroy(gameObject);
    }

    public IEnumerator Co_FireWave(bool isRight)
    {
        // 방향 설정
        int dir = isRight ? 1 : -1;
        Vector3 firePos = transform.position + Vector3.right * dir;

        // 발사
        var proj = Instantiate(wavePrefab, firePos, Quaternion.identity);
        yield return StartCoroutine(proj.GetComponent<DokkaebiWave>().Fire(isRight));

        StartCoroutine(Co_DoStealth());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어 충돌
        if (collision.CompareTag("Player"))
        {
            // 데미지
            Debug.Log($"[{gameObject.name}] Player Hit!");
            PlayerState.instance.TakeDamage(baseDamage);

            // 폭발 중 아니면 폭발
            if (!isExploded)
            {
                StartCoroutine(Co_Explosion(true));
            }
        }
        else if (collision.CompareTag("Wall") || collision.CompareTag("Ground") || collision.CompareTag("OneWayPlatform") && !isChase)
        {
            // 지형 충돌 : 폭발
            if (!isExploded)
            {
                StartCoroutine(Co_Explosion());
            }
        }
    }
}
