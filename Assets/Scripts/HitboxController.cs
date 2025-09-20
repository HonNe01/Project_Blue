using UnityEngine;

public class HitboxController : MonoBehaviour
{
    public enum HitbotType { Player, Enemy }

    [SerializeField] private float baseDamage = 1f;
    [SerializeField] private HitbotType type;


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

                    var enemy = collision.GetComponent<BossBase>();
                    if (enemy != null)
                        enemy.TakeDamage(baseDamage);
                }
                break;
            case HitbotType.Enemy:
                if (collision.CompareTag("Player"))
                {
                    Debug.Log($"[{gameObject.name}] Player Hit!");

                    //collision.GetComponent<PlayerHealth>()?.TakeDamage(baseDamage);
                }
                break;
        }
    }

    public void ObjectOff()
    {
        gameObject.SetActive(false);
    }
}
