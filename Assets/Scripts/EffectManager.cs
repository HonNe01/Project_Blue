using System;
using UnityEngine;


public class EffectManager : MonoBehaviour
{
    public static EffectManager instance;

    public enum EffectType
    {
        // Move : 4
        Walk, Dash, Jump, AirJump, Slide,

        // Attack : 8
        Attack1, Attack2, Attack3,
        ChargeAttack, UpAttack, 
        JumpAttack, JumpUpAttack, DownAttack,

        // Skill : 2
        Skill, SkillDown,

        // Module : 7
        Attack1_Module, Attack2_Module, Attack3_Module,
        ChargeAttack_Module, UpAttack_Module,
        JumpAttack_Module, JumpUpAttack_Module, DownAttack_Module,

        // Hit : 6
        MusinHit,
        AttackHit, SkillHit,
        ExplosionNormalHit, ExplosionElectronicHit, ExplosionFireHit,
    }

    public GameObject[] MoveEffects;
    public GameObject[] AttackEffects;
    public GameObject[] SkillEffects;
    public GameObject[] ModuleEffects;
    public GameObject[] HitEffect;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.Log("[EffectManager] Instance Destroy");
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        EffectsCheck();
    }

    private void EffectsCheck() // 이펙트 개수 체크
    {
        int totalEnumCount = Enum.GetNames(typeof(EffectType)).Length;
        int totalArrayCount = MoveEffects.Length + AttackEffects.Length + SkillEffects.Length + ModuleEffects.Length + HitEffect.Length;

        if (totalEnumCount != totalArrayCount)
        {
            Debug.LogError($"[EffectManager] 이펙트 개수 불일치! : Enum {totalEnumCount} vs Array {totalArrayCount}");
        }

        Vector3 farPos = new Vector3(9999f, 9999f, 0f);

        foreach (EffectType t in Enum.GetValues(typeof(EffectType)))
        {
            PlayEffect(t, farPos, false);
        }
    }

    public void PlayEffect(EffectType type, Vector3 position, bool flipX = false)
    {
        // 이펙트 가져오기
        GameObject vfx = GetEffect(type);
        if (vfx == null) return;

        // 이펙트 위치 및 회전 설정
        vfx.transform.position = position;
        if (flipX)
        {
            vfx.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            vfx.transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        // 이펙트 활성화
        vfx.SetActive(true);
    }

    public void MoveEffect(EffectType type, float moveSpeed)
    {
        GameObject vfx = GetEffect(type);
        Rigidbody2D rb = vfx.GetComponent<Rigidbody2D>();

        if (PlayerState.instance.isRight > 0 && rb != null)
        {
            rb.AddForce(new Vector2(moveSpeed, 0), ForceMode2D.Impulse);
        }
        else
        {
            rb.AddForce(new Vector2(-moveSpeed, 0), ForceMode2D.Impulse);
        }
    }

    public void StopEffect(EffectType type)
    {
        // 이펙트 가져오기
        GameObject vfx = GetEffect(type);
        if (vfx == null) return;

        // 이펙트 비활성화
        vfx.SetActive(false);
    }

    public GameObject GetEffect(EffectType type)
    {
        int index = (int)type;

        // MoveEffects 배열 범위 체크
        if (index < MoveEffects.Length)
        {
            return MoveEffects[index];
        }
        index -= MoveEffects.Length;

        // AttackEffects 배열 범위 체크
        if (index < AttackEffects.Length)
        {
            return AttackEffects[index];
        }
        index -= AttackEffects.Length;

        // SkillEffects 배열 범위 체크
        if (index < SkillEffects.Length)
        {
            return SkillEffects[index];
        }
        index -= SkillEffects.Length;

        // ModuleEffects 배열 범위 체크
        if (index < ModuleEffects.Length)
        {
            return ModuleEffects[index];
        }

        index -= ModuleEffects.Length;

        // HitEffect 배열 범위 체크
        if (index < HitEffect.Length)
        {
            return HitEffect[index];
        }

        // 찾지 못했을 경우
        Debug.LogWarning($"[EffectManager] GetEffect: Unknown type {type}");
        return null;
    }
}
