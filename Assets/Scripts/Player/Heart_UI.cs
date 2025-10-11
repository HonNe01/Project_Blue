using UnityEngine;
using UnityEngine.UI;

public class Heart_UI : MonoBehaviour
{
    [Header("Health Setting")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("Skill Gauge")]
    public PlayerState playerstate;
    public Image gaugeFill;

    void Update()
    {
        UpdateHearts();
        if (playerstate != null && gaugeFill != null)
        {
            gaugeFill.fillAmount = playerstate.GaugePercent / 100f;
        }
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = i < PlayerState.instance.CurHp ? fullHeart : emptyHeart;
        }
    }
}
