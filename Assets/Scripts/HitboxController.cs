using UnityEngine;

public class HitboxController : MonoBehaviour
{
    public enum HitbotType { Player, Enemy , skill}

    [SerializeField] private int baseDamage = 1;
    [SerializeField] private HitbotType type;

    public PlayerState playerstate;


    void Awake()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (type)
        {
            case HitbotType.Player:
                if (collision.CompareTag("Enemy"))
                {
                    Debug.Log($"[Player] {collision.gameObject.name} Hit!");
                    playerstate.AddGauge(5);

                    var enemy = collision.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
            case HitbotType.Enemy:
                if (collision.CompareTag("Player"))
                {
                    Debug.Log($"[{gameObject.name}] Player Hit!");

                    PlayerState.instance.TakeDamage(baseDamage);
                }
                break;
            case HitbotType.skill:
                if (collision.CompareTag("Enemy"))
                {
                    Debug.Log($"[Player] {collision.gameObject.name} Hit!");

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
            case HitbotType.Player:
                if (collision.gameObject.CompareTag("Enemy"))
                {
                    Debug.Log($"[Player] {collision.gameObject.name} Hit!");

                    var enemy = collision.gameObject.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
            case HitbotType.Enemy:
                if (collision.gameObject.CompareTag("Player"))
                {
                    Debug.Log($"[{gameObject.name}] Player Hit!");

                    PlayerState.instance.TakeDamage(baseDamage);
                }
                break;
        }
    }

    public void ObjectOff()
    {
        gameObject.SetActive(false);
    }
}
