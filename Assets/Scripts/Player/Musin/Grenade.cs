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

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private IEnumerator Co_Explosion(bool hasDamage = true)
    {
        if (exploded) yield break;
        exploded = true;

        if (bodyColl) bodyColl.enabled = false;
        if (expColl && !hasDamage) expColl.enabled = true;

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
    }
    }
}
