using System.Collections.Generic;
using TMPro;
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
    [SerializeField] public string mainMenuScene = "MainMenu";
    [SerializeField] public string selectScene = "Choose Charater";
    [SerializeField] public string helicopterScene = "Helicopter";
    [SerializeField] public string fallenScene = "YeonhwaEntrace";
    [SerializeField] public string outpostScene = "OutPostScene";
    [SerializeField] public string gildalScene = "GildalScene";
    [SerializeField] public string cheongryuScene = "CheongRyuScene";

    [Header(" === UI Reference === ")]
    [SerializeField] private MenuType curMenu = MenuType.None;
    [SerializeField] private GameObject[] menuPanels;

    [Header("Graphics")]
    public TMP_Dropdown resolutionDropdown;
    private List<Resolution> resolutions = new List<Resolution>();
    private int optimalResolutionIndex = 0;

    [Header("Inventory")]
    public PlayerInventory inventory;

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

        if (resolutionDropdown != null)
        {
            ResolutionsInit();
        }
        State = GameState.MainMenu;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Update()
    {
        // 닫기 & 정지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 인벤토리 닫기
            if (inventory != null)
            {
                if (inventory.IsOpen)
                {
                    inventory.SetActiveUI(false);
                    CursorDisable();

                    return;
                }
            }

            // 일시정지
            if (State == GameState.Playing)
            {
                OpenMenu(MenuType.Pause);
            }
            else if (State == GameState.Paused || State == GameState.MainMenu)
            {
                CloseMenu();
            }
        }

        // 인벤토리 열기
        if (State == GameState.Playing && curMenu == MenuType.None)
        {
            if (Input.GetKeyDown(KeyCode.Tab) && inventory != null)
            {
                if (inventory.IsOpen)
                {
                    inventory.SetActiveUI(false);
                    CursorDisable();
                }
                else
                {
                    inventory.SetActiveUI(true);
                    CursorEnable();
                }
            }
        }
    }


    // ===== 게임 화면 관련 =====
    public void GameStart()
    {
        SceneManager.LoadScene(selectScene);
    }

    public void GameGraphic()
    {
        OpenMenu(MenuType.Graphic);
    }

    public void ResolutionsInit()
    {
        // 16:9
        resolutions.Add(new Resolution { width = 1920, height = 1080 });
        resolutions.Add(new Resolution { width = 2560, height = 1440 });
        resolutions.Add(new Resolution { width = 3840, height = 2160 });
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        for (int i = 0; i < resolutions.Count; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            // 가장 적합한 해상도에 별표를 표기합니다.
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                optimalResolutionIndex = i;
                option += " *";
            }
            options.Add(option);
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = optimalResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // 게임이 가장 적합한 해상도로 시작되도록 설정합니다.
        SetResolution(optimalResolutionIndex);
    }

    public void SetResolution(int index)
    {
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

        Debug.Log($"[GameManager] Set Resolution : {resolution.width} X {resolution.height}");
    }

    public void SetScreen(bool full)
    {
        // 전체화면 설정
        Screen.fullScreen = full;
        
        Debug.Log($"[GameManager] Full Screen : {full}");
    }

    public void SetVSync(bool enable)
    {
        // 수직동기화 설정
        if (enable)
        {
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;
        }

        Debug.Log($"[GameManager] VSync : {enable}");
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

    public void GameQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    // ===== 게임 설정 관련 =====
    public void GameOption()
    {
        OpenMenu(MenuType.Option);
    }

    public void GameAudio()
    {
        OpenMenu(MenuType.Audio);
    }

    public void MasterAudio()
    {
        BGMAudio();
        SFXAudio();
    }

    public void BGMAudio()
    {
        SoundManager.instance.UpdateVolumeBGM();
    }

    public void SFXAudio()
    {
        SoundManager.instance.UpdateVolumeSFX();
    }

    public void GameControl()
    {
        OpenMenu(MenuType.Control);
    }



    // ===== 씬 이동 관련 =====
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        State = GameState.MainMenu;
        SceneManager.LoadScene(mainMenuScene);

        CursorEnable();
    }

    public void GoToOP()
    {
        Debug.Log($"[GameManager] Load Scene : {SceneManager.GetActiveScene().name} -> {outpostScene}");

        SceneManager.LoadScene(outpostScene);
    }

    public void GoToGD()
    {
        Debug.Log($"[GameManager] Load Scene : {SceneManager.GetActiveScene().name} -> {gildalScene}");

        SceneManager.LoadScene(gildalScene);
    }

    public void GoToCR()
    {
        Debug.Log($"[GameManager] Load Scene : {SceneManager.GetActiveScene().name} -> {cheongryuScene}");

        SceneManager.LoadScene(cheongryuScene);
    }

    public Portal.PortalType GetCurrentSceneType()
    {
        // 현재 씬 이름으로 씬 타입 반환
        string sceneName = SceneManager.GetActiveScene().name;

        // 씬 이름에 따른 씬 타입 매핑
        return sceneName switch
        {
            var name when name == outpostScene => Portal.PortalType.OutPost,
            var name when name == gildalScene => Portal.PortalType.Gildal,
            var name when name == cheongryuScene => Portal.PortalType.CheongRyu,
            _ => Portal.PortalType.OutPost,
        };
    }

    // ===== 마우스 커서 상태 관련 =====
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

        // BGM 재생
        if (scene.name == mainMenuScene)
        {
            // 메인 메뉴 씬
            State = GameState.MainMenu;
            SoundManager.instance.PlayBGM(SoundManager.BGM.Main);

            // 마우스 커서 활성화
            CursorEnable();
        }
        else if (scene.name == selectScene)
        {
            State = GameState.Directing;
            SoundManager.instance.PlayBGM(SoundManager.BGM.Select);

            CursorEnable();
        }
        else if (scene.name == helicopterScene)
        {
            State = GameState.Directing;
            SoundManager.instance.PlayBGM(SoundManager.BGM.Helicopter);

            CursorEnable();
        }
        else if (scene.name == fallenScene)
        {
            State = GameState.Directing;
            SoundManager.instance.PlayBGM(SoundManager.BGM.Fallen);

            CursorEnable();

        }
        else if (scene.name == outpostScene)
        {
            // 게임 씬
            State = GameState.Playing;
            SoundManager.instance.PlayBGM(SoundManager.BGM.OutPost);

            // 마우스 커서 비활성화
            CursorDisable();
        }
        else if (scene.name == gildalScene)
        {
            // 게임 씬
            State = GameState.Playing;
            SoundManager.instance.PlayBGM(SoundManager.BGM.Gildal_Normal);

            // 마우스 커서 비활성화
            CursorDisable();
        }
        else if (scene.name == cheongryuScene)
        {
            // 게임 씬
            State = GameState.Playing;
            SoundManager.instance.PlayBGM(SoundManager.BGM.CheongRyu_Normal);

            // 마우스 커서 비활성화
            CursorDisable();
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
