using UnityEngine;
using UnityEngine.UI;

public class Heart_UI : MonoBehaviour
{
    [Header("Health Setting")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    void Update()
    {
        UpdateHearts();
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = i < PlayerState.instance.CurHp ? fullHeart : emptyHeart;
        }
    }
}
