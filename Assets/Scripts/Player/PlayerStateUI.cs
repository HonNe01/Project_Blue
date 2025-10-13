using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateUI : MonoBehaviour
{
    [Header("State UI")]
    public CanvasGroup canvas;
    public float fadeSpeed = 1f;
    private bool isVisible = true;
    private Coroutine co_Fade;

    [Header("Health Setting")]
    public GameObject[] hearts; // 체력 UI

    [Header("Skill Gauge")]
    public Image[] gauges;      // 게이지 UI

    void Update()
    {
        bool isShow = GameManager.instance.State != GameManager.GameState.Directing;
        if (isShow != isVisible)
        {
            SetActiveUI(isShow);
        }


        if (isVisible)
        {
            UpdateHearts();
            UpdateGauge();
        }
    }

    void SetActiveUI(bool isShow)
    {
        if (co_Fade != null)
            StopCoroutine(co_Fade);

        co_Fade = StartCoroutine(Co_Fade(isShow));
    }

    IEnumerator Co_Fade(bool isShow)
    {
        isVisible = isShow;
        float targetAlpha = isShow ? 1f : 0f;
        float startAlpha = canvas.alpha;

        while (!Mathf.Approximately(canvas.alpha, targetAlpha))
        {
            canvas.alpha = Mathf.MoveTowards(canvas.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

            yield return null;
        }

        canvas.interactable = isShow;
        canvas.blocksRaycasts = isShow;
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
