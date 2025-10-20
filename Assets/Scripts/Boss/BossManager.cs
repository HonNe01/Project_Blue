using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    public enum BossType { Gildal, Chyeongryu }


    [Header(" === Boss Manager === ")]
    public BossType type;
    public BossBase prefab;
    public Transform spawnPoint;

    [Header("Boss Arena")]
    public GameObject ArenaEntrance;

    public void Awake()
    {
        ArenaEntrance.SetActive(false);
    }

    public void BattleStart()
    {
        SetArenaLocked(true);

        BossSpawn();
    }

    public void BattleFinish()
    {
        SetArenaLocked(false);
    }

    private void BossSpawn()
    {

    }

    private void SetArenaLocked(bool locked)
    {
        ArenaEntrance.SetActive(locked);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            BattleStart();
        }
    }
}
