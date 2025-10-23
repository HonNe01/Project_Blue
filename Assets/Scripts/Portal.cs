using System.Collections;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public enum PortalType { OutPost, Gildal, ChyeongRyu }

    [Header("Portal Setting")]
    [SerializeField] private PortalType fromScene;  // 포탈 위치
    [SerializeField] private PortalType toScene;    // 포탈 목적지

    [Header("Portal UI")]
    [SerializeField] private GameObject promptUI;
    private bool playerInRange = false;

    private void Awake()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void Update()
    {
        GoScene();
    }

    private void GoScene()
    {
        if (!playerInRange) return;

        if (Input.GetKey(KeyCode.V))
        {
            // 이동 시 이전 위치를 기록
            PortalTransit.SetNextFrom(fromScene);

            // 해당 씬으로 이동
            switch (toScene)
            {
                case PortalType.OutPost:
                    GameManager.instance.GoToOP();

                    break;
                case PortalType.Gildal:
                    GameManager.instance.GoToGD();

                    break;
                case PortalType.ChyeongRyu:
                    GameManager.instance.GoToCR();

                    break;
            }
        }
    }

    private void Start()
    {
        // 씬 로드시 실행
        if (!PortalTransit.HasPending) return;
        if (PortalTransit.NextFrom != fromScene) return;

        StartCoroutine(Co_Start());
    }

    private IEnumerator Co_Start()
    {
        yield return null;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && PortalTransit.NextFrom == fromScene)
        {
            player.transform.position = transform.position;
        }

        PortalTransit.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            if (promptUI != null)
                promptUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }
}
