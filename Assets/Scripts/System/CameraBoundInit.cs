using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraBoundInit : MonoBehaviour
{
    public enum CameraType { None, First }
    public CameraType type;

    private CinemachineConfiner2D confiner;
    public Collider2D areaCollider;

    private void Awake()
    {
        areaCollider = GetComponent<Collider2D>();

        if (areaCollider.isTrigger != true) areaCollider.isTrigger = true;
    }

    private void Start()
    {
        if (type == CameraType.First) StartCoroutine(CameraInit());
    }

    private IEnumerator CameraInit()
    {
        yield return null;

        if (confiner == null && PlayerState.instance != null)
        {
            confiner = PlayerState.instance.cinemachineComposer.gameObject.GetComponent<CinemachineConfiner2D>();
        }

        confiner.BoundingShape2D = areaCollider;
        confiner.InvalidateBoundingShapeCache();

        Debug.Log($"[Camera] Camera Area Change -> {gameObject.name}");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(CameraInit());
        }
    }

    private void OnDrawGizmos()
    {
        if (areaCollider == null) return;

        Gizmos.color = Color.yellow;
        if (areaCollider is BoxCollider2D box)
        {
            Vector2 size = box.size;
            Gizmos.DrawWireCube(transform.position, size);
        }
        
    }
}
