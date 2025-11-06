using Unity.Cinemachine;
using UnityEngine;

public class ShakeCamera : MonoBehaviour
{
    public CinemachineImpulseSource impulse;

    // 카메라 흔들기
    public void ImpulseCamera()
    {
        impulse = GetComponent<CinemachineImpulseSource>();
        impulse.GenerateImpulse();
    }
}
