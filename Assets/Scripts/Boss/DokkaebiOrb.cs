using System.Collections;
using UnityEngine;

public class DokkaebiOrb : MonoBehaviour
{
    [Header("Projectile Setting")]
    public float speed = 10f;
    public float lifeTime = 10f;
    private float minLifeTime;
    [SerializeField] private int baseDamage = 1;

    private Vector2 dir;
    private Animator anim;
    [SerializeField] private Collider2D fireColl;
    [SerializeField] private Collider2D expColl;
    private Rigidbody2D rigid;

    private float spawnTime;
    private bool exploded = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();

        fireColl.enabled = true;
        expColl.enabled = false;

        spawnTime = Time.time;
        minLifeTime = lifeTime * 0.4f;
        
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(dir * speed * Time.deltaTime, Space.World);
    }

    private IEnumerator Explosion()
    {
        exploded = true;

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
            fireColl.enabled = false;

            collision.GetComponent<Player_Health>()?.TakeDamage(baseDamage);
            
            if (!exploded)
                StartCoroutine(Explosion());
        }
        else if (collision.CompareTag("Wall") || collision.CompareTag("Ground") || collision.CompareTag("OneWayPlatform"))
        {
            if (Time.time - spawnTime < minLifeTime)
            {
                return;
            }
            rigid.linearVelocity = Vector3.zero;
            fireColl.enabled = false;
            expColl.enabled = true;

            StartCoroutine(Explosion());
        }
    }
}
