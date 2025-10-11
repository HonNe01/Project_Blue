using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public enum GameState { MainMenu, Playing, Directing, Paused }
    public GameState State { get; private set; }

    public enum MenuType { None, Pause, Option, Graphic, Audio, Control }
    private Stack<MenuType> menuStack = new Stack<MenuType>();

    [Header(" === Scene Names === ")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string outpostScene = "OutPostScene";
    [SerializeField] private string gildalScene = "GildalScene";
    [SerializeField] private string cheongryuScene = "CheongRyuScene";

    [Header(" === UI Reference === ")]
    [SerializeField] private MenuType curMenu = MenuType.None;
    [SerializeField] private GameObject[] menuPanels;

    private GameObject mainMenu;

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

        // UI 상태 초기화
        foreach (var panel in menuPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
            
        State = GameState.MainMenu;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        // 일시정지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (State == GameState.Playing)
            {
                OpenMenu(MenuType.Pause);
            }
            else if (State == GameState.Paused || State == GameState.MainMenu)
            {
                CloseMenu();
            }
        }

        // 인벤토리
        if (State == GameState.Playing)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (State == GameState.Playing)
                {

                }
            }
        }
    }

    public void GameStart()
    {
        SceneManager.LoadScene(outpostScene);
    }

    public void GameOption()
    {
        OpenMenu(MenuType.Option);
    }

    public void GameGraphic()
    {
        OpenMenu(MenuType.Graphic);
    }

    public void GameAudio()
    {
        OpenMenu(MenuType.Audio);
    }

    public void GameControl()
    {
        OpenMenu(MenuType.Control);
    }

    public void OpenMenu(MenuType type)
    {
        if (curMenu == type) return;

        // 메뉴 스택 넣기
        menuStack.Push(type);

        // 메뉴 패널 열기
        ActiveMenu(type);
        CursorEnable();

        LogMenuStack("After OpenMenu");
    }

    public void CloseMenu()
    {
        if (curMenu == MenuType.None) return;

        // 현재 메뉴 닫기
        DeActiveMenu(curMenu);
        menuStack.Pop();

        if (menuStack.Count > 0)
        {
            // 스택 있음 -> 이전 메뉴 열기
            MenuType prev = menuStack.Peek();

            ActiveMenu(prev);
        }
        else
        {
            // 스택 없음 -> 메뉴 끝
            curMenu = MenuType.None;

            // 메뉴 다 닫힘
            if (State != GameState.MainMenu)
            {
                // 게임 재개
                State = GameState.Playing;

                Time.timeScale = 1f;
                CursorDisable();
            }
        }

        LogMenuStack("After CloseMenu");
    }

    public void ActiveMenu(MenuType type)
    {
        if (curMenu == type) return;

        // 기존 메뉴 닫기
        if (curMenu != MenuType.None)
        {
            DeActiveMenu(curMenu);
        }

        // 새 메뉴 열기
        GameObject panel = GetPanel(type);
        if (panel != null)
        {
            panel.SetActive(true);

            Debug.Log($"[GameManager] {type} Panel Open");
        }

        curMenu = type;

        if (State != GameState.MainMenu)
        {
            // 게임 상태 설정
            State = GameState.Paused;

            Time.timeScale = 0f;
            CursorEnable();
        }
    }

    private void DeActiveMenu(MenuType type)
    {
        GameObject panel = GetPanel(type);
        if (panel != null)
        {
            panel.SetActive(false);

            Debug.Log($"[GameManager] {type} Panel Close");
        }
    }

    private GameObject GetPanel(MenuType type)
    {
        int index = (int)type - 1;
        
        if (index >= 0 && index < menuPanels.Length)
        {
            return menuPanels[index];
        }

        return null;
    }
    
    void LogMenuStack(string tag = "")
    {
        var arr = menuStack.ToArray();

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append($"[GameManager] MenuStack {tag}, Stack Count = {menuStack.Count} : ");

        for (int i = arr.Length - 1; i >= 0; i--)
        {
            sb.Append(arr[i].ToString());
            if (i > 0)
                sb.Append(" -> ");
        }

        Debug.Log(sb.ToString());
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
        // 상태 정리
        curMenu = MenuType.None;
        Time.timeScale = 1f;

        // 메뉴 전부 닫기
        foreach (var panel in menuPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        if (scene.name == mainMenuScene)
        {
            // 메인 메뉴 씬
            State = GameState.MainMenu;

            // 메인메뉴 UI 연결
            if (mainMenu == null)
            {
                mainMenu = GameObject.Find("Main Panel");
            }
            
            CursorEnable();
        }
        else
        {
            // 게임 씬
            State = GameState.Playing;
            
            // 마우스 커서 비활성화
            CursorDisable();
        }
    }
}
