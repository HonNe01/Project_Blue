using System.Collections;
using UnityEngine;

public class DokkaebiOrb : MonoBehaviour
{
    [Header("Projectile Setting")]
    public float speed = 10f;
    public float lifeTime = 10f;
    [SerializeField] private float baseDamage = 1f;

    private Vector2 dir;
    private Animator anim;
    private Collider2D coll;
    private Rigidbody2D rigid;
    

    private void Start()
    {
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        rigid = GetComponent<Rigidbody2D>();

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(dir * speed * Time.deltaTime, Space.World);
    }

    private IEnumerator Explosion()
    {
        anim.SetTrigger("Explosion");
        yield return null;
        float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);    // anim 끝날 때까지 대기

        // 폭발 모션 이후 파괴
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"[{gameObject.name}] Player Hit!");
            rigid.linearVelocity = Vector3.zero;
            coll.enabled = false;

            float damage = baseDamage;
            //collision.GetComponent<PlayerHealth>()?.TakeDamage(damage);

            StartCoroutine(Explosion());
        }
        else if (collision.CompareTag("Wall") || collision.CompareTag("Ground") || collision.CompareTag("OneWayPlatform"))
        {
            rigid.linearVelocity = Vector3.zero;
            coll.enabled = false;

            StartCoroutine(Explosion());
        }
    }
}
