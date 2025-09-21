using UnityEngine;

public class Player_Health : MonoBehaviour
{
    public int maxHP = 5;
    private int currentHP;

    public int CurrentHP => currentHP;

    void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int damage = 1)  // 데미지 호출
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        if (currentHP <= 0) Die();
    }

    public void Heal(int amount = 1)
    {
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
    }

    private void Die() // 나중에 씬전환이랑 애니메이션 넣기
    {
        Debug.Log("플레이어 사망!");
    }
}