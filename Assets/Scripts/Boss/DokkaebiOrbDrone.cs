using System.Collections;
using UnityEngine;

public class DokkaebiOrbDrone : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private GameObject orbPrefab;
    [SerializeField] private GameObject wavePrefab;
    [SerializeField] private float fireSpeed = 10f;

    // 드론 자동 조작
    public IEnumerator Co_DroneAuto(Vector2 target)
    {
        yield return StartCoroutine(Co_EndStealth());
        yield return null;

        FireOrb(target);
        yield return null;

        StartCoroutine(Co_DoStealth());
    }

    // 은신
    public IEnumerator Co_DoStealth()
    {
        // 은신 로직
        if (sprite != null)
        {
            float elapsed = 0f;
            Color c = sprite.color;

            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / 1f);
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

            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / 1f);
                c.a = Mathf.Lerp(0f, 1f, t);
                sprite.color = c;
                yield return null;
            }

            c.a = 1f;
            sprite.color = c;
        }
    }

    public void FireOrb(Vector2 target)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;
        var proj = Instantiate(orbPrefab, transform.position, Quaternion.identity);
        proj.GetComponent<Rigidbody2D>().linearVelocity = dir * fireSpeed;

        StartCoroutine(Co_DoStealth());
    }

    public IEnumerator FireWave(bool isRight)
    {
        // 방향 설정
        int dir = isRight ? 1 : -1;
        Vector3 firePos = transform.position + Vector3.right * dir;

        // 발사
        var proj = Instantiate(wavePrefab, firePos, Quaternion.identity);
        var wave = proj.GetComponent<DokkaebiWave>();
        yield return StartCoroutine(wave.Fire(isRight));

        StartCoroutine(Co_DoStealth());
    }
}
