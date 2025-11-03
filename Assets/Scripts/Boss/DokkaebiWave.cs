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

    private void Awake()
    {
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
        sprite = GetComponent<SpriteRenderer>();

        if (anim == null) Debug.LogError($"{name}: Animator is null!");
        if (coll == null) Debug.LogError($"{name}: Collider2D is null!");
        if (sprite == null) Debug.LogError($"{name}: SpriteRenderer is null!");
    }

    private void Start()
    {
        coll.enabled = false; // 충돌 비활성화
    }

    public void AE_Charge()
    {
        SoundManager.instance.PlaySFX(SoundManager.SFX.Special_Wave_Charge_Gildal);
    }

    public IEnumerator Fire(bool isRight)
    {
        yield return null;

        // 좌우반전
        sprite.flipX = !isRight;
        int dir = isRight ? 1 : -1;

        coll.enabled = true;
        coll.offset = new Vector2(9 * dir, 0);

        // 발사
        anim.SetTrigger("Wave");
        yield return new WaitForSeconds(fireTime);
        yield return StartCoroutine(WaveEnd());
    }

    public void AE_FireSound()
    {
        SoundManager.instance.PlaySFX(SoundManager.SFX.Special_Wave_Fire_Gildal);
    }

    public IEnumerator WaveEnd()
    {
        coll.enabled = false; // 충돌 비활성화
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
