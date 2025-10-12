using UnityEngine;

public class Portal : MonoBehaviour
{
    public enum PortalType { OutPost, Gildal, ChyeongRyu }
    [SerializeField] private PortalType targetScene;

    [SerializeField] private GameObject promptUI;
    private bool playerInRange = false;

    private void Awake()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange)
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                GoScene();
            }
        }
    }

    private void GoScene()
    {
        switch (targetScene)
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
