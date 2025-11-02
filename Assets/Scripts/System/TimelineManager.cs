using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineManager : MonoBehaviour
{

    [Header("Timeline Reference")]
    public PlayableDirector director;
    public CinemachineImpulseSource impulseSource;

    [Header("UI 안내문")]
    public GameObject continueText; 

    private bool isHolding = false;
    private double holdTime = 0;

    void Start()
    {
        if (continueText != null)
            continueText.SetActive(false); // 시작할 때 안내문 비활성화
    }

    // 타임라인 일시정지 메서드
    public void HoldTimeline()
    {
        if (director == null) return;

        holdTime = director.time;   // 현재 시점 저장
        isHolding = true;

        // 안내문 표시
        if (continueText != null)
            continueText.SetActive(true);
    }

    public void ShakeCamera()
    {
        if (impulseSource != null)
            impulseSource.GenerateImpulse();
    }

    void Update()
    {
        if (isHolding)
        {
            director.time = holdTime;
            director.Evaluate();
        }

        // 일시정지 상태에서 아무 키나 누르면 재생
        if (isHolding && Input.anyKeyDown)
        {
            isHolding = false;
            if (continueText != null)
                continueText.SetActive(false);

            director.Play(); 
        }

        // 일시정지 상태가 아닐 때 아무 키나 누르면 타임라인 스킵
        if (!isHolding && Input.anyKeyDown)
        {
            SkipTimeline();
        }
    }

    void SkipTimeline()
    {
        if (director == null) return;

        director.time = director.duration; 
        director.Evaluate();               
        director.Stop();                   
        Debug.Log("Timeline skipped!");
    }
}