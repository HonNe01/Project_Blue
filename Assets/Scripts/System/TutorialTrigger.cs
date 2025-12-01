using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public GameObject targetObject;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && targetObject != null)
        {
            targetObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && targetObject != null)
        {
            targetObject.SetActive(false);
        }
    }
}
