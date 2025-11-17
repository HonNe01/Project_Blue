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
    private bool isFirst = true;

     
    [Header("Boss Arena")]
    public GameObject arenaEntrance;
    public Collider2D bossCameraArena;
    public GameObject bossPlatforms;

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

    private void Update()
    {
        BossStateCheck();
    }

    public void BattleStart()
    {
        if (isBattle && !isFirst) return;
        isBattle = true;
        isFirst = false;

        SetArenaLocked(true);
        SetBossCamera(true);
        SetGround(true);
        BossSpawn();
    }

    public void BossStateCheck()
    {
        if (!isBattle) return;

        if (curBoss.state == BossBase.BossState.Die)
        {
            isBattle = false;

            BattleFinish();
        }
    }

    public void BattleFinish()
    {
        SetArenaLocked(false);
        SetBossCamera(false);
        SetGround(false);

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
        StartCoroutine(curBoss.Co_StartBattle());
    }

    private void SetArenaLocked(bool locked)
    {
        arenaEntrance.SetActive(locked);
    }

    private void SetGround(bool enable)
    {
        if (bossPlatforms == null) return;

        bossPlatforms.SetActive(enable);
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
        if (!isBattle && isFirst && collision.CompareTag("Player"))
        {
            BattleStart();
        }
    }
}
