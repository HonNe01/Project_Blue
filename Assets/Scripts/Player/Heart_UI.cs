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

    public Player_Health player_Health;

    void Update()
    {
       
        UpdateHearts();
    }

   

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = i < playerHealth.CurrentHP ? fullHeart : emptyHeart;
        }
    }
}
