using UnityEngine;
using UnityEngine.UI;

public class Heart_UI : MonoBehaviour
{
    [Header("데이터 연결")]
    public Player_Health playerHealth;

    [Header("하트 이미지")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("회복 설정")]
    public float healHoldTime = 0.5f;
    private float healTimer = 0f;
    private bool healedThisPress = false;

    void Update()
    {
        HandleHealing();
        UpdateHearts();
    }

    void HandleHealing()
    {
        if (Input.GetKey(KeyCode.F))
        {
            if (!healedThisPress && playerHealth.CurrentHP < playerHealth.maxHP)
            {
                healTimer += Time.deltaTime;
                if (healTimer >= healHoldTime)
                {
                    playerHealth.Heal(1);
                    healedThisPress = true;
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            healTimer = 0f;
            healedThisPress = false;
        }
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = i < playerHealth.CurrentHP ? fullHeart : emptyHeart;
        }
    }
}
