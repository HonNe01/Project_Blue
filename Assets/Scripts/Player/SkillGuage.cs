using UnityEngine;
using UnityEngine.UI; 

public class SkillGuage : MonoBehaviour
{
    [Header("게이지 설정")]
    public int maxGuage = 100;
    public int currentGuage = 0;
    public Image guageFill;       // UI 이미지

    // 게이지 비율
    public float GaugeRatio => currentGuage / maxGuage;

    void Update()
    {
        if (guageFill != null)
            guageFill.fillAmount = GaugeRatio;
    }

    public void AddGuauge(int amount)      // 공격을 할때 증가
    {
        currentGuage += amount;
        currentGuage = Mathf.Clamp(currentGuage, 0, maxGuage);
        Debug.Log("5 증가");
    }

    public bool UseGuage(int amount)     // 스킬을 사용할때 감소
    {
        if (currentGuage < amount)
        {
            Debug.Log("게이지 부족");
            return false;
        }

        currentGuage -= amount;
        Debug.Log("게이지 사용");
        return true;
    }
}
