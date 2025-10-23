using System.Collections;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("UI Reference")]
    private CanvasGroup canvas;
    [SerializeField] private float fadeSpeed = 1f;

    private Coroutine co_Fade;

    [Header(" === UI Slash === ")]
    public GameObject slashImage;

    [Header(" === UI Granade === ")]
    public GameObject impactImage;
    public GameObject electronicImage;
    public GameObject fireImage;


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

    public void SlashModuel()
    {
        if (((Musin_State)PlayerState.instance).swordSlash)
        {
            slashImage.SetActive(false);
            ((Musin_State)PlayerState.instance).swordSlash = false;
        }
        else
        {
            slashImage.SetActive(true);
            ((Musin_State)PlayerState.instance).swordSlash = true;
        }
    }

    public void FireModuel()
    {
        if (((Musin_State)PlayerState.instance).fireGranade)
        {
            fireImage.SetActive(false);
            ((Musin_State)PlayerState.instance).fireGranade = false;
        }
        else
        {
            fireImage.SetActive(true);
            impactImage.SetActive(false);
            electronicImage.SetActive(false);
            ((Musin_State)PlayerState.instance).fireGranade = true;
            ((Musin_State)PlayerState.instance).electricGranade = false;
            ((Musin_State)PlayerState.instance).impactGranade = false;
        }
    }

    public void ElectronicModuel()
    {
        if (((Musin_State)PlayerState.instance).electricGranade)
        {
            electronicImage.SetActive(false);
            ((Musin_State)PlayerState.instance).electricGranade = false;
        }
        else
        {
            fireImage.SetActive(false);
            impactImage.SetActive(false);
            electronicImage.SetActive(true);
            ((Musin_State)PlayerState.instance).fireGranade = false;
            ((Musin_State)PlayerState.instance).electricGranade = true;
            ((Musin_State)PlayerState.instance).impactGranade = false;
        }
    }

    public void ImpactModuel()
    {
        if (((Musin_State)PlayerState.instance).impactGranade)
        {
            impactImage.SetActive(false);
            ((Musin_State)PlayerState.instance).impactGranade = false;
        }
        else
        {
            fireImage.SetActive(false);
            impactImage.SetActive(true);
            electronicImage.SetActive(false);
            ((Musin_State)PlayerState.instance).fireGranade = false;
            ((Musin_State)PlayerState.instance).electricGranade = false;
            ((Musin_State)PlayerState.instance).impactGranade = true;
        }
    }

}
