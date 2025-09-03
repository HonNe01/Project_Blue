using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Boss/Gildal/Pattern_SwingAttack")]
public class Gildal_SwingAttack : AttackPatternSO
{
    [Header("Timing")]
    [Tooltip("선딜(텔레그래프) 시간")]
    public float windup = 0.3f; // 선딜
    [Tooltip("히트 활성 시간(실제 판정 시간)")]
    public float activeTime = 0.12f;

    [Header("Hitbox")]
    [Tooltip("보스 위치 기준, 전방으로 밀어낼 오프셋")]
    public Vector2 forwardOffset = new Vector2(1.4f, 0.2f);
    [Tooltip("판정 반지름(전방 원형)")]
    public float radius = 2.2f;
    [Tooltip("좌우로 퍼지는 각도(도) - 0이면 정면만, 180이면 반원")]
    public float range = 3.5f;  // 공격 범위
    [Tooltip("맞출 레이어")]
    public LayerMask hitMask;

    [Header("Damage")]
    [Tooltip("기본 데미지 (보스 배율 곱해짐)")]
    public float baseDmaage = 1f;
    [Tooltip("넉백 힘")]
    public float knockback = 7f;

    [Header("VFX/SFX")]
    public GameObject swingVfxPrefab;
    public AudioClip swingSfx;
    [Tooltip("피격 시 짧은 히트스톱(초)")]
    public float hitStop = 0.05f;

    // 내부 캐시(디버그용)
    private readonly List<Collider2D> _results = new(8);

    public override IEnumerator Execute(BossBase boss)
    {
        var tr = boss.transform;
        var anim = boss.GetComponent<Animator>();

        // 0) 조준 방향(보스 -> 플레이어) 기준으로 좌우 플립
        int facingDir = (int)Mathf.Sign(boss.GetAimPoint().x - tr.position.x);
        if (facingDir == 0) facingDir = 1;

        // 좌우 반전
        var sr = tr.GetComponentInChildren<SpriteRenderer>();
        if (sr) sr.flipX = (facingDir < 0);

        // 선딜
        anim.SetTrigger("SwingPrep");
        yield return new WaitForSeconds(windup);

        // 공격
        anim.SetTrigger("Swing");
        // sfx..
        //if (swingVfxPrefab) Object.Instantiate(swingVfxPrefab, tr.position, Quaternion.identity);

        // 후딜은 Boss가 recoveryTime만큼 기다림
    }
}
