using UnityEngine;

public class Player_Health : MonoBehaviour
{
    public int maxHP = 5;
    private int currentHP;
    public int CurrentHP => currentHP;

    [Header("힐 설정")]
    public float healHoldTime = 0.5f;
    private float healTimer = 0f;
    private bool healedThisPress = false;

    [HideInInspector]
    public bool isHealing = false; // PlayerMove에서 체크용

    void Awake()
    {
        currentHP = maxHP;
    }

    void Update()
    {
        HandleHealing();
    }

    void HandleHealing()
    {
        if (Input.GetKey(KeyCode.F))
        {
            isHealing = true;

            if (!healedThisPress && currentHP < maxHP)
            {
                healTimer += Time.deltaTime;
                if (healTimer >= healHoldTime)
                {
                    Heal(1);
                    healedThisPress = true;
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.F))
        {
            isHealing = false;
            healTimer = 0f;
            healedThisPress = false;
        }
    }

    public void TakeDamage(int damage = 1)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        Debug.Log("Damage");

        if (currentHP <= 0) Die();
    }

    public void Heal(int amount = 1)
    {
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        Debug.Log("Healed! CurrentHP: " + currentHP);
    }

    private void Die()
    {
        Debug.Log("플레이어 사망!");
    }
}
