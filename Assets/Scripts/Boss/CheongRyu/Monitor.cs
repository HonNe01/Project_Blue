using UnityEngine;

public class Monitor : MonoBehaviour
{
    public enum  MonitorState
    {
        Inactive,
        Active,
        Destroyed,
        Repairing
    }

    public int hp;
    public bool isDestroyed = false;
}
