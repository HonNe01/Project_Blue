using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameState { MainMenu, Playing, Paused }
    public GameState State { get; private set; }

    [Header(" === Scene Names === ")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string outpostScene = "OutPostScene";
    [SerializeField] private string gildalScene = "GildalScene";
    [SerializeField] private string cheongryuScene = "CheongRyuScene";

    [Header(" === UI Reference === ")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject optionMenu;

    private void Awake()
    {
        // 인스턴스
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }
            
        State = GameState.MainMenu;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        if (State == GameState.Playing || State == GameState.Paused)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (State == GameState.Playing)
                {
                    GamePause();
                }
                else if (State == GameState.Paused)
                {
                    GameResume();
                }
            }
        }
    }

    public void GamePause()
    {
        if (State != GameState.Playing) return;

        State = GameState.Paused;
        Time.timeScale = 0f;
        if (pauseMenu != null) pauseMenu.SetActive(true);

        CursorEnable();
    }

    public void GameResume()
    {
        if (State != GameState.Paused) return;

        State = GameState.Playing;
        Time.timeScale = 1f;
        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (optionMenu != null) optionMenu.SetActive(false);

        CursorDisable();
    }

    public void GameStart()
    {
        SceneManager.LoadScene(outpostScene);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        State = GameState.MainMenu;
        SceneManager.LoadScene(mainMenuScene);

        CursorEnable();
    }

    public void GoToGD()
    {
        SceneManager.LoadScene(gildalScene);
    }

    public void GoToCR()
    {
        SceneManager.LoadScene(cheongryuScene);
    }

    public void GameQuit()
    {
        Application.Quit();
    }

    public void CursorEnable()
    {
        // 커서 보이기, 활성화
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CursorDisable()
    {
        // 커서 숨기기, 비활성화
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 바뀌면 UI 재연결, 상태 설정
        if (scene.name == mainMenuScene)
        {
            // 메인메뉴 진입시 -> 일시정지 UI 비활성화
            State = GameState.MainMenu;

            if (pauseMenu != null) pauseMenu.SetActive(false);
            if (optionMenu != null) optionMenu.SetActive(false);
        }
        else
        {
            // 인게임 진입시 -> UI 연결
            State = GameState.Playing;

            if (pauseMenu != null) pauseMenu.SetActive(false);
            if (optionMenu != null) optionMenu.SetActive(false);

            // 마우스 커서 비활성화
            CursorDisable();
        }
    }
}
