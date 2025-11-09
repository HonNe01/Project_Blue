using System.Collections;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public enum PortalType { Ruins, OutPost, Gildal, CheongRyu }

    [Header("2 Way Setting")]
    [SerializeField] private PortalType endA;   // 포탈 목적지A
    [SerializeField] private PortalType endB;   // 포탈 목적지B
    [SerializeField] private int routeId;       // 경로 ID, 다중 경로 구분

    [Header("This Scene Tag")]
    [SerializeField] private PortalType thisScene; // 이 포탈이 속한 씬 (null이면 GameManager에서 자동 설정)
    [SerializeField] private bool useOverride = false; // 씬 태그 오버라이드 사용 여부

    [Header("Spawn Override")]
    [SerializeField] private Transform spawnPoint; // 씬 진입 시 플레이어 스폰 위치 오버라이드 (null이면 포탈 위치)

    [Header("Portal UI")]
    [SerializeField] private GameObject promptUI;
    private bool playerInRange = false;
    private bool isLoading = false;

    private void Awake()
    {
        if (promptUI != null) promptUI.SetActive(false);
    }
    private void Start()
    {
        var current = GetThisSceneType();

        if (PortalTransit.TryConsume(current, endA, endB, routeId))
        {
            StartCoroutine(Co_Spawn());
        }
    }

    private void Update()
    {
        GoScene();
    }

    private IEnumerator Co_Spawn()
    {
        yield return null;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 스폰 위치 설정
            var pos = (spawnPoint != null) ? spawnPoint.position : transform.position;

            player.transform.position = pos;
        }
    }

    private void GoScene()
    {
        // 플레이어가 포탈 범위 내에 있을 때만 작동
        if (!playerInRange || isLoading) return;

        if (Input.GetKeyDown(KeyCode.V))
        {
            var current = GetThisSceneType();
            var destination = GetDestination(current);

            PortalTransit.Set(current, endA, endB, routeId);
            isLoading = true;

            // 해당 씬으로 이동
            switch (destination)
            {
                case PortalType.Ruins:      GameManager.instance.GoToRu();  break;
                case PortalType.OutPost:    GameManager.instance.GoToOP();  break;
                case PortalType.Gildal:     GameManager.instance.GoToGD();  break;
                case PortalType.CheongRyu:  GameManager.instance.GoToCR();  break;
            }
        }
    }

    private PortalType GetThisSceneType()
    {
        if (useOverride) return thisScene;

        // GameManager에서 현재 씬에 맞는 포탈 타입 반환
        return GameManager.instance.GetCurrentSceneType();
    }

    private PortalType GetDestination(PortalType current)
    {
        if (current == endA) return endB;
        if (current == endB) return endA;

        return current;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            if (promptUI != null) promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptUI != null) promptUI.SetActive(false);
        }
    }
}
