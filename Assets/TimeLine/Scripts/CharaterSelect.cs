using UnityEngine;
using UnityEngine.SceneManagement;

public class CharaterSelect : MonoBehaviour
{
    public static CharaterSelect instance;

    [Header("Select")]
    public GameObject selectPanel;

    [Header("Musin")]
    public bool musinSelected = false;
    private bool musinSelect = false;
    public GameObject musin;
    public GameObject musinEffect;

    [Header("Moonsin")]
    public bool moonsinSelected = false;
    private bool moonsinSelect = false;
    public GameObject moonsin;
    public GameObject moonsinEffect;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("[Character] Instance Destroy");
            Destroy(gameObject);
        }
    }

    public void OnSelect()
    {
        Debug.Log("[Character] Select Panel Open");

        selectPanel.SetActive(true);
    }

    public void SelectMusin()
    {
        Debug.Log("[Character] Musin Select");

        musinSelect = true;
        moonsinSelect = false;

        selectPanel.SetActive(false);
        TimelineManager.instance.ResumeTimeline();
    }

    public void SelectMoonsin()
    {
        Debug.Log("[Character] Moonsin Select");

        moonsinSelect = true;
        musinSelect = false;

        selectPanel.SetActive(false);
        TimelineManager.instance.ResumeTimeline();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameManager.instance.fallenScene)
        {
            if (musinSelect)
            {
                Instantiate(musin, Vector2.zero, Quaternion.identity);
                Instantiate(musinEffect, Vector2.zero, Quaternion.identity);
            }
            else if (moonsinSelect)
            {
                Instantiate(moonsin, Vector2.zero, Quaternion.identity);
                Instantiate(moonsinEffect, Vector2.zero, Quaternion.identity);
            }
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject);
        }
    }

}
