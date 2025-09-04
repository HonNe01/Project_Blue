using UnityEngine;

public class HitboxController : MonoBehaviour
{
    [SerializeField] private float baseDamage = 1f;
    private BossBase owner;

    void Awake()
    {
        owner = GetComponentInParent<BossBase>();
        gameObject.SetActive(false);
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

    public void Activate(float duration)
    {
        StartCoroutine(Co_Active(duration));
    }

    private System.Collections.IEnumerator Co_Active(float duration)
    {
        gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false);
    }
}
