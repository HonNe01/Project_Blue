using UnityEngine;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour
{
    public void OnNextScene()
    {
        SceneManager.LoadScene(GameManager.instance.nextScene);
    }
}
