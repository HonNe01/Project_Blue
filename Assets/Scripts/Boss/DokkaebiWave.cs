using System.Collections;
using UnityEngine;

public class DokkaebiWave : MonoBehaviour
{
    [Header("Projectile Setting")]
    public int fireTime = 2;
    [SerializeField] private int baseDamage = 1;

    private Animator anim;
    private Collider2D coll;
    private SpriteRenderer sprite;

    private void Start()
    {
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        sprite = GetComponent<SpriteRenderer>();

        if (anim == null) Debug.LogError($"{name}: Animator is null!");
        if (coll == null) Debug.LogError($"{name}: Collider2D is null!");
        if (sprite == null) Debug.LogError($"{name}: SpriteRenderer is null!");
    }

    public IEnumerator Fire(bool isRight)
    {
        // 메모리 세팅 대기
        yield return null;

        // 좌우반전
        sprite.flipX = !isRight;
        int dir = isRight ? 1 : -1;
        coll.offset = new Vector2(9 * dir, 0);

        // 발사
        anim.SetTrigger("Wave");
        yield return null;

        float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);    // anim 끝날 때까지 대기

        StartCoroutine(WaveEnd());
    }

    public IEnumerator WaveEnd()
    {
        anim.SetTrigger("End");
        yield return null;

        float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);    // anim 끝날 때까지 대기

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"[{gameObject.name}] Player Hit!");

            PlayerState.instance.TakeDamage(baseDamage);
        }
    }
}
