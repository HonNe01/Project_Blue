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
    public Vector2 OrbOffset;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float arriveThreshold = 0.15f;
    [SerializeField] private float limitTime = 1f;
    private int baseDamage = 1;

    private bool isExploded = false;
    private float spawntime;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        spawntime = Time.time;
        if (expColl) expColl.enabled = false;
        if (bodyColl) bodyColl.enabled = true;
    }

    // 드론 자동 조작: 은신 해제 -> 돌진/폭발
    public IEnumerator Co_DroneAuto(Vector2 target)
    {
        yield return StartCoroutine(Co_EndStealth());
        yield return null;

        StartCoroutine(Co_FireOrb(target));
    }

    // 은신
    public IEnumerator Co_DoStealth()
    {
        // 은신 로직
        if (sprite != null)
        {
            float elapsed = 0f;
            Color c = sprite.color;

            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / 0.5f);
                c.a = Mathf.Lerp(1f, 0f, t);
                sprite.color = c;
                yield return null;
            }

            c.a = 0f;
            sprite.color = c;
        }

        yield return null;
        Destroy(gameObject);
    }

    public IEnumerator Co_EndStealth()
    {
        // 은신 해제 로직
        if (sprite != null)
        {
            float elapsed = 0f;
            Color c = sprite.color;

            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / 0.5f);
                c.a = Mathf.Lerp(0f, 1f, t);
                sprite.color = c;
                yield return null;
            }

            c.a = 1f;
            sprite.color = c;
        }
    }

    public IEnumerator Co_FireOrb(Vector2 target)
    {
        Vector2 targetPos = target + OrbOffset;

        Vector2 dir = targetPos.sqrMagnitude > 0.0001f 
            ? targetPos.normalized : 
            (transform.localScale.x >= 0f ? Vector2.right : Vector2.left);

        float elapsed = 0f;

        while (elapsed < limitTime && !isExploded)
        {
            transform.Translate(dir * dashSpeed * Time.deltaTime, Space.World); 
            elapsed += Time.deltaTime;

            yield return null;
        }

        // 시간 만료
        if (!isExploded)
        {
            yield return StartCoroutine(Co_Explosion());
        }
    }

    private IEnumerator Co_Explosion()
    {
        if (isExploded) yield break;
        isExploded = true;

        // 콜라이더 전환
        if (bodyColl) bodyColl.enabled = false;
        if (expColl) expColl.enabled = true;

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
                StartCoroutine(Co_Explosion());
            }
        }
        else if (collision.CompareTag("Wall") || collision.CompareTag("Ground") || collision.CompareTag("OneWayPlatform"))
        {
            // 지형 충돌 : 폭발
            if (Time.time - spawntime >= limitTime)
            {
                StartCoroutine(Co_Explosion());
            }
        }
    }
}
