using UnityEngine;


public class EffectManager : MonoBehaviour
{
    public static EffectManager instance;

    public enum EffectType 
    { 
        // Move
        Walk, Jump, AirJump, Slide,

        // Attack
        Attack1, Attack2, Attack3,
        ChargeAttack, UpAttack,
        JumpAttack, JumpUpAttack, DownAttack,

        // Skill
        Skill, SkillDown,

        // Module
        Attack1_Module, Attack2_Module, Attack3_Module,
        ChargeAttack_Module, UpAttack_Module,
        JumpAttack_Module, JumpUpAttack_Module, DownAttack_Module,
    }

    public GameObject[] MoveEffects;
    public GameObject[] AttackEffects;
    public GameObject[] SkillEffects;
    public GameObject[] ModuleEffects;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
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

        // 찾지 못했을 경우
        Debug.LogWarning($"[EffectManager] GetEffect: Unknown type {type}");
        return null;
    }
}
