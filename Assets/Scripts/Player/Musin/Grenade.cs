using System.Collections;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public enum GrenadeType { Normal, Fire, Impact, Electronic }
    [SerializeField] private GrenadeType type;
    [SerializeField] private Collider2D bodyColl;
    [SerializeField] private Collider2D expColl;
    private int baseDamage = 1;
    private bool exploded = false;

    private Rigidbody2D rb;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private IEnumerator Co_Explosion()
    {
        if (exploded) yield break;
        exploded = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
        }

        if (bodyColl) bodyColl.enabled = false;
        if (expColl) expColl.enabled = true;

        Debug.Log("explosion");
        anim.SetTrigger("Explosion");
        yield return null;
        float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);    
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (type)
        {
            case GrenadeType.Normal:

                if (collision.CompareTag("Ground"))
                {
                    StartCoroutine(Co_Explosion());
                }
                else if (collision.CompareTag("Enemy"))
                {
                    var enemy = collision.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
            case GrenadeType.Fire:
                if (collision.CompareTag("Ground"))
                {
                    StartCoroutine(Co_Explosion());
                }
                else if (collision.CompareTag("Enemy"))
                {
                    var enemy = collision.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
            case GrenadeType.Electronic:
                if (collision.CompareTag("Ground"))
                {
                    StartCoroutine(Co_Explosion());
                }
                else if (collision.CompareTag("Enemy"))
                {
                    var enemy = collision.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
            case GrenadeType.Impact:
                if (collision.CompareTag("Ground") || collision.CompareTag("Wall") || collision.CompareTag("OneWayPlatform"))
                {
                    StartCoroutine(Co_Explosion());
                }
                else if (collision.CompareTag("Enemy"))
                {
                    StartCoroutine(Co_Explosion());
                    var enemy = collision.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (type)
        {
            case GrenadeType.Normal:
                if (collision.gameObject.CompareTag("Ground"))
                {
                    StartCoroutine(Co_Explosion());
                }
                else if (collision.gameObject.CompareTag("Enemy"))
                {
                    var enemy = collision.gameObject.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
            case GrenadeType.Fire:
                if (collision.gameObject.CompareTag("Ground"))
                {
                    StartCoroutine(Co_Explosion());
                }
                else if (collision.gameObject.CompareTag("Enemy"))
                {
                    var enemy = collision.gameObject.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
            case GrenadeType.Electronic:
                if (collision.gameObject.CompareTag("Ground"))
                {
                    StartCoroutine(Co_Explosion());
                }
                else if (collision.gameObject.CompareTag("Enemy"))
                {
                    var enemy = collision.gameObject.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
            case GrenadeType.Impact:
                if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("OneWayPlayform"))
                {
                    StartCoroutine(Co_Explosion());
                }
                else if (collision.gameObject.CompareTag("Enemy"))
                {
                    StartCoroutine(Co_Explosion());
                    var enemy = collision.gameObject.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
        }
    }
}
