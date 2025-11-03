using UnityEngine;
using UnityEngine.SceneManagement;

public class CharaterSelect : MonoBehaviour
{
    public bool musinSelected = false;
    public bool moonsinSelected = false;
    public GameObject musin;
    public GameObject moonsin;
    public GameObject musinEffect;
    public GameObject moonsinEffect;
    private bool musinSelect = false;
    private bool moonsinSelect = false;


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void SelectMusin()
    {
        musinSelect = true;
        moonsinSelect = false;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(GameManager.instance.outpostScene);
    }

    public void SelectMoonsin()
    {
        moonsinSelect = true;
        musinSelect = false;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(GameManager.instance.outpostScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameManager.instance.outpostScene)
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
