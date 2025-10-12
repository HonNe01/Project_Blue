using UnityEngine;
using UnityEngine.UI;

public class Heart_UI : MonoBehaviour
{
    [Header("Health Setting")]
    public GameObject[] hearts; // 체력 UI

    [Header("Skill Gauge")]
    public Image[] gauges;      // 게이지 UI

    void Update()
    {
        UpdateHearts();
        UpdateGauge();
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            bool isActive = i < PlayerState.instance.CurHp;

            if (hearts[i].activeSelf != isActive)
                hearts[i].SetActive(isActive);
        }
    }

    void UpdateGauge()
    {
        float gaugePer = Mathf.Clamp(PlayerState.instance.currentGauge, 0f, 100f);
        float per = 20f;

        for (int i = 0; i < gauges.Length; i++)
        {
            float min = i * per;
            float max = (i + 1) * per;

            float fill = Mathf.Clamp01((gaugePer - min) / per);
            gauges[i].fillAmount = fill;
        }
    }
}