using System.Collections;
using UnityEngine;

public abstract class AttackPatternSO : ScriptableObject
{
    [Header("Meta")]
    [Tooltip("패턴 사용 후 다시 사용 가능해지기까지 걸리는 시간(초)")]
    public float cooldown = 2f;

    [Tooltip("패턴 종료 후 기본 대기 시간(초)")]
    public float recoveryTime = 0.4f;

    [Tooltip("패턴 선택 가중치 (값이 높을수록 자주 사용")]
    public float weight = 1f;

    [Tooltip("기본 데미지 (실제 계산 시 보스 damageMultiplier를 곱함")]
    public float baseDamage = 1f;

    // 가능 여부
    public virtual bool CanExecute(BossBase boss) => true;

    // 실제 패턴 로직
    public abstract IEnumerator Execute(BossBase boss);
}
