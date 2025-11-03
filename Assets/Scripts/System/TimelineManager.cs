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
            continueText.SetActive(false);
    }

    // 타임라인 일시정지
    public void HoldTimeline()
    {
        if (director == null) return;

        holdTime = director.time;
        isHolding = true;

        // 타임라인 완전 일시정지
        director.Pause();

        if (continueText != null)
            continueText.SetActive(true);
    }

    // 카메라 흔들기
    public void ShakeCamera()
    {
        if (impulseSource != null)
            impulseSource.GenerateImpulse();
    }

    void Update()
    {
        // 일시정지 중 키 입력 → 타임라인 재개
        if (isHolding && Input.anyKeyDown)
        {
            isHolding = false;

            if (continueText != null)
                continueText.SetActive(false);

            director.time = holdTime;
            director.Play();
        }
    }
}