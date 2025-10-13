using System.Collections;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("UI Reference")]
    private CanvasGroup canvas;
    [SerializeField] private float fadeSpeed = 1f;

    private Coroutine co_Fade;

    public bool IsOpen { get; private set; } = false;

    private void Awake()
    {
        canvas = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        canvas.alpha = 0;
        canvas.interactable = false;
        canvas.blocksRaycasts = false;

        if (GameManager.instance != null)
        {
            GameManager.instance.inventory = this;
        }
    }

    public void SetActiveUI(bool isShow)
    {
        if (co_Fade != null)
            StopCoroutine(co_Fade);

        co_Fade = StartCoroutine(Co_Fade(isShow));
    }

    IEnumerator Co_Fade(bool isShow)
    {
        IsOpen = isShow;
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
}
