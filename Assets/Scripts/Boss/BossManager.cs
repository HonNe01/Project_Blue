using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    public enum BossType { Gildal, Chyeongryu }


    [Header(" === Boss Manager === ")]
    public BossType bossType;
    public GameObject bossPrefab;
    public Transform spawnPoint;

    private BossBase curBoss;
    private bool isBattle = false;

     
    [Header("Boss Arena")]
    public GameObject ArenaEntrance;

    public void Awake()
    {
        if (ArenaEntrance != null)
        {
            ArenaEntrance.SetActive(false);
        }
    }

    public void BattleStart()
    {
        if (isBattle) return;
        isBattle = true;

        SetArenaLocked(true);
        BossSpawn();
    }

    public void BattleFinish()
    {
        SetArenaLocked(false);

        Debug.Log("[BossManager] Boss Battle Finish");
    }

    private void BossSpawn()
    {
        if (bossPrefab == null) return;

        // 보스 소환
        Vector3 spawnPos = spawnPoint ? spawnPoint.position : transform.position;
        GameObject bossObject = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        curBoss = bossObject.GetComponent<BossBase>();

        // 보스 타겟 세팅
        if (curBoss.target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                curBoss.target = player.transform;
            }
        }

        // 보스 전투 시작
        Debug.Log($"[BossManager] {bossType} Boss Spawn Complete");
        StartCoroutine(curBoss.StartBattle());
    }

    private void SetArenaLocked(bool locked)
    {
        ArenaEntrance.SetActive(locked);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isBattle && collision.CompareTag("Player"))
        {
            BattleStart();
        }
    }
}
