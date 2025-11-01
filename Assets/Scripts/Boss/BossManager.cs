using System.Collections.Generic;
using Unity.Cinemachine;
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
    public GameObject arenaEntrance;
    public Collider2D bossCameraArena;

    private CinemachineConfiner2D confiner;
    private Collider2D originCameraArena;

    public void Awake()
    {
        if (arenaEntrance != null)
        {
            arenaEntrance.SetActive(false);
        }
    }

    private void Start()
    {
        // 가상 카메라 참조
        var vcam = FindAnyObjectByType<CinemachineCamera>();

        if (vcam != null)
        {
            confiner = vcam.GetComponent<CinemachineConfiner2D>();
        }
        if (confiner != null)   // 원래 카메라 영역 저장
        {
            originCameraArena = confiner.BoundingShape2D;
        }
    }

    public void BattleStart()
    {
        if (isBattle) return;
        isBattle = true;

        SetArenaLocked(true);
        SetBossCamera(true);
        BossSpawn();
    }

    public void BattleFinish()
    {
        SetArenaLocked(false);
        SetBossCamera(false);

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
        arenaEntrance.SetActive(locked);
    }

    private void SetBossCamera(bool enable)
    {
        if (confiner == null) return;

        if (enable)     // 보스 카메라 영역으로 변경
        {
            if (bossCameraArena != null)
            {
                confiner.BoundingShape2D = bossCameraArena;
                confiner.InvalidateBoundingShapeCache();
            }
        }
        else            // 원래 카메라 영역으로 변경
        {
            confiner.BoundingShape2D = originCameraArena;
            confiner.InvalidateBoundingShapeCache();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isBattle && collision.CompareTag("Player"))
        {
            BattleStart();
        }
    }
}
