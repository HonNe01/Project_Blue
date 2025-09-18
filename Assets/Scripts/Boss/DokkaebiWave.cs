using System.Collections;
using UnityEngine;

public class DokkaebiWave : MonoBehaviour
{
    [Header("Projectile Setting")]
    public int fireTime = 2;
    [SerializeField] private float baseDamage = 1f;

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
        // 좌우반전
        sprite.flipX = !isRight;
        int dir = isRight ? 1 : -1;
        coll.offset = new Vector2(9 * dir, 0);

        // 발사
        anim.SetTrigger("Wave");
        yield return null;  // 1프레임 대기 -> Animator의 state 갱신 대기
        float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength * fireTime);    // anim 끝날 때까지 대기

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"[{gameObject.name}] Player Hit!");

            float damage = baseDamage;
            //collision.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }
    }
}
