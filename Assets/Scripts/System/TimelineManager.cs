using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineManager : MonoBehaviour
{
    public static TimelineManager instance;

    [Header(" === Timeline Setting === ")]
    [Header(" Timeline Reference")]
    public PlayableDirector director;
    public CinemachineImpulseSource impulse;

    [Header(" Timeline UI")]
    public GameObject continueText;

    [Header(" === Signal Setting ===")]
    [Header(" Signal Reference")]
    public SignalReceiver receiver;

    [Header("Normal Signal")]
    public SignalAsset holdScene;
    public SignalAsset shakeCamera;

    [Header("Select Scene Signal")]
    public SignalAsset SelectPanelOpen;
    
    [Header("Helicopter Scene Signal")]
    public SignalAsset redLightOn;
    public SignalAsset redLightOff;

    private bool isHolding = false;
    private double holdTime = 0;

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
            Debug.Log("[TimeLineManager] Instance Destroy");
            Destroy(gameObject);
            return;
        }

        impulse = GetComponent<CinemachineImpulseSource>();
    }

    void Start()
    {
        if (continueText != null)
            continueText.SetActive(false);
    }

    void Update()
    {
        // 일시정지 중 키 입력 → 타임라인 재개
        if (isHolding && Input.anyKeyDown)
        {
            isHolding = false;

            if (continueText != null)
                continueText.SetActive(false);

            var rootPlayable = director.playableGraph.GetRootPlayable(0);
            rootPlayable.SetSpeed(1);
        }
    }

    // 타임라인 초기화
    public void InitTimeline()
    {
        director = FindAnyObjectByType<PlayableDirector>();
        if (director != null) receiver = director.gameObject.GetComponent<SignalReceiver>();

        // 시그널 초기화
        RegistSignal(holdScene, HoldTimeline);
        RegistSignal(shakeCamera, ShakeCamera);
        RegistSignal(SelectPanelOpen, OpenSelectPanel);
    }

    public void RegistSignal(SignalAsset asset, UnityAction action)
    {
        // 신호 가져오기
        UnityEvent unityEvent = receiver.GetReaction(asset);

        if (unityEvent == null)
        {
            // 신호 추가 
            unityEvent = new UnityEvent();
            receiver.AddReaction(asset, unityEvent);
        }
        
        // 리액션 할당
        unityEvent.AddListener(action);
    }

    // 타임라인 재생
    public void PlayTimeline()
    {
        director.Play();
    }

    // 타임라인 일시정지
    public void HoldTimeline()
    {
        if (director == null) return;
        isHolding = true;

        // 타임라인 일시정지
        var rootPlayable = director.playableGraph.GetRootPlayable(0);
        rootPlayable.SetSpeed(0);

        if (continueText != null)
            continueText.SetActive(true);
    }

    // 카메라 흔들기
    public void ShakeCamera()
    {
        impulse.GenerateImpulse();
    }

    // 캐릭터 선택창
    public void OpenSelectPanel()
    {
        var rootPlayable = director.playableGraph.GetRootPlayable(0);
        rootPlayable.SetSpeed(0);
    }
}