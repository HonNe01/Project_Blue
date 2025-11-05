using UnityEngine;
using UnityEngine.Rendering;

public class RedLightOn : MonoBehaviour
{
    public Volume volume;
    public float maxWeight = 0.5f;
    public float blinkSpeed = 1f; // 1초 주기

    private float timer;

    void Update()
    {
        if (volume == null) return;

        timer += Time.deltaTime;
        // 0 ~ 1 초 주기로 PingPong
        volume.weight = Mathf.PingPong(timer * maxWeight * 2 / blinkSpeed, maxWeight);
    }
}