using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameState { MainMenu, Playing, Directing, Paused }
    public GameState State { get; private set; }

    [Header(" === Scene Names === ")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string outpostScene = "OutPostScene";
    [SerializeField] private string gildalScene = "GildalScene";
    [SerializeField] private string cheongryuScene = "CheongRyuScene";

    [Header(" === UI Reference === ")]
    [SerializeField] private GameObject curMenu;

    private GameObject mainMenu;
    private GameObject pauseMenu;
    private GameObject optionMenu;

    private void Awake()
    {
        // 인스턴스
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
            
        State = GameState.MainMenu;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        if (State == GameState.Playing || State == GameState.Paused)
        {
            // 일시정지
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

            // 인벤토리
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (State == GameState.Playing)
                {

                }
            }
        }
    }

    public void GamePause()
    {
        if (State != GameState.Playing) return;

        State = GameState.Paused;
        Time.timeScale = 0f;
        curMenu = pauseMenu;

        if (pauseMenu != null) pauseMenu.SetActive(true);

        CursorEnable();
    }

    public void GameResume()
    {
        if (State != GameState.Paused) return;

        State = GameState.Playing;
        Time.timeScale = 1f;
        curMenu = null;

        if (pauseMenu != null) pauseMenu.SetActive(false);
        if (optionMenu != null) optionMenu.SetActive(false);

        CursorDisable();
    }

    public void GameStart()
    {
        SceneManager.LoadScene(outpostScene);
    }

    public void GameOption()
    {
        if (optionMenu == null) return;

        // 기존 UI 닫기
        if (SceneManager.GetActiveScene().name == mainMenuScene)
        {
            if (mainMenu != null) mainMenu.SetActive(false);
        }
        else
        {
            if (pauseMenu != null) pauseMenu.SetActive(false);
        }

        // 옵션 UI 열기
        optionMenu.SetActive(true);

        curMenu = optionMenu;
    }

    public void PanelClose()
    {
        if (curMenu != null) curMenu.SetActive(false);

        // 이전 UI 열기
        if (SceneManager.GetActiveScene().name == mainMenuScene)
        {
            if (mainMenu != null) mainMenu.SetActive(true);

            curMenu = null;
        }
        else
        {
            if (pauseMenu != null) pauseMenu.SetActive(true);

            curMenu = pauseMenu;
        }
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
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
        curMenu = null;
        Time.timeScale = 1f;

        // 씬 바뀌면 UI 재연결, 상태 설정
        if (pauseMenu == null)
        {
            pauseMenu = FindChildInactive("Pause Panel");
            pauseMenu.SetActive(false);
        }
        else
        {
            pauseMenu.SetActive(false);
        }

        if (optionMenu == null)
        {
            optionMenu = FindChildInactive("Option Panel");
            optionMenu.SetActive(false);
        }
        else
        {
            optionMenu.SetActive(false);
        }

        if (scene.name == mainMenuScene)
        {
            State = GameState.MainMenu;

            // 메인메뉴 진입시 -> 메인메뉴 UI 연결
            if (mainMenu == null)
            {
                mainMenu = GameObject.Find("Main Panel");
            }
        }
        else
        {
            State = GameState.Playing;

            // 인게임 진입시 -> Menu UI 비활성화
            if (pauseMenu != null) pauseMenu.SetActive(false);
            if (optionMenu != null) optionMenu.SetActive(false);

            // 마우스 커서 비활성화
            CursorDisable();
        }
    }

    GameObject FindChildInactive(string childName)
    {
        Transform[] all = GetComponentsInChildren<Transform>(true);

        foreach (var t in all)
        {
            if (t.name == childName) return t.gameObject;
        }

        return null;
    }
}
