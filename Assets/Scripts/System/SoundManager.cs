using System.Collections;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public enum BGM 
    { 
        // 메인
        Main, 

        // 시나리오
        Select, Helicopter, Fallen,

        // 플레이
        YeonhwaEntrance, OutPost, 

        // 보스
        Gildal_Normal, Gildal_Battle,
        CheongRyu_Normal, CheongRyu_Battle,
    }

    [Header(" === Volume Sliders === ")]
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header(" === BGM Settings === ")]
    public AudioClip[] bgmClips;
    public float bgmVolume = 0.5f;
    AudioSource bgmPlayer;

    public enum SFX
    {
        // System
        Click,

        // Player
        Walk, Jump, Dash, Healing,
        // - Musin
        Attack1_Musin, Attack2_Musin, Attack3_Musin, ChargeAttack_Musin,


        Attack_Hit,
        // - Moonsin


        // Enemy
        // - Gildal
        Landing_Gildal, Start_Gildal, Cry_Gildal, Stealth_Gildal, 
        Sturn_Gildal, Shock_Gildal,
        Phase1_Attack1_Gildal, Phase1_Attack2_Gildal,
        Phase2_Attack1_Gildal, Phase2_Attack2_Gildal,
        Attack3_Gildal, 
        Special_Wave_Charge_Gildal, Special_Wave_Fire_Gildal,
        Explosion_Drone,

        // - CheongRyu
    }

    [Header(" === SFX Settings === ")]
    public float sfxVolume = 0.2f;
    public int channelCount = 5;
    AudioSource[] sfxPlayers;
    int channelIndex = 0;

    [Header("System")]
    [Tooltip("")]
    public AudioClip[] systemSFXClips;

    [Header("Player")]
    [Tooltip("")]
    public AudioClip[] playerSFXClips;
    [Tooltip("")]
    public AudioClip[] musinSFXClips;
    [Tooltip("")]
    public AudioClip[] moonsinSFXClips;

    [Header("Enemy")]
    [Tooltip("")]
    public AudioClip[] enemySFXClips;
    [Tooltip("")]
    public AudioClip[] gildalSFXClips;
    [Tooltip("")]
    public AudioClip[] cheongRyuSFXClips;

    private void Awake()
    {
        // 인스턴스 초기화
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("[SoundManager] Instance Destroy");
            Destroy(gameObject);
            return;
        }

        // 플레이어 초기화
        InitPlayer();
    }

    private void Start()
    {
        // 슬라이더 초기화
        InitSlider();

        // 플레이어 볼륨 로드
        LoadVolume();
    }

    private void InitPlayer()
    {
        // BGM 플레이어 초기화
        GameObject bgmObject = new GameObject("BGM Player");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;

        // SFX 플레이어 초기화
        GameObject sfxObject = new GameObject("SFX Player");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channelCount];

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].loop = false;
        }
    }

    private void InitSlider()
    {
        // UI Sldier 할당
        if (masterSlider == null && bgmSlider == null && sfxSlider == null)
        {
            // 슬라이더 오브젝트 찾기
            Slider[] sliders = GameManager.instance.gameObject.GetComponentsInChildren<Slider>(true);

            foreach (var slider in sliders)
            {
                if (slider.CompareTag("Master_Slider")) masterSlider = slider.GetComponent<Slider>();
                if (slider.CompareTag("BGM_Slider")) bgmSlider = slider.GetComponent<Slider>();
                if (slider.CompareTag("SFX_Slider")) sfxSlider = slider.GetComponent<Slider>();
            }
        }

        // 플레이어 사운드 초기화
        bgmPlayer.volume = bgmVolume * bgmSlider.value * masterSlider.value;
        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            sfxPlayers[i].volume = sfxVolume * sfxSlider.value * masterSlider.value;
        }
    }

    public void PlayBGM(BGM bgm)
    {
        if (bgmPlayer == null)
        {
            Debug.Log("[SoundManager] Player Loading...");
        }

        // 오디오 선택
        AudioClip nextClip = bgmClips[(int)bgm];
        if (bgmPlayer.clip == nextClip) return;

        // 오디오 페이드
        Debug.Log($"[SoundManager] BGM : {bgm} Play");
        FadeBGM(nextClip, 3f);
    }

    public void UpdateVolumeBGM()
    {
        if (bgmPlayer == null) return;
        
        bgmPlayer.volume = bgmVolume * bgmSlider.value * masterSlider.value;
    }

    public void FadeBGM(AudioClip nextClip, float fadeTime = 1f)
    {
        StartCoroutine(Co_FadeBGM(nextClip, fadeTime));
    }

    IEnumerator Co_FadeBGM(AudioClip newClip, float fadeTime)
    {
        // 사운드 세팅
        float startVolume = bgmPlayer.volume;

        float time = 0f;
        if (bgmPlayer.isPlaying)
        {
            // 페이드 아웃    
            while (time < fadeTime)
            {
                time += Time.deltaTime;
                bgmPlayer.volume = Mathf.Lerp(startVolume, 0f, time / fadeTime);

                yield return null;
            }
            bgmPlayer.Stop();
        }
        bgmPlayer.volume = 0f;

        // 다음 클립 세팅
        bgmPlayer.clip = newClip;
        bgmPlayer.Play();

        // 사운드 세팅
        float targetVolume;
        if (bgmSlider == null && masterSlider == null)
        {
            targetVolume = 0.5f;
        }
        else
        {
            targetVolume = bgmVolume * bgmSlider.value * masterSlider.value;
        }

        time = 0f;
        while (time < fadeTime)
        {
            // 페이드 인
            time += Time.deltaTime;
            bgmPlayer.volume = Mathf.Lerp(0f, targetVolume, time / fadeTime);

            yield return null;
        }
        bgmPlayer.volume = targetVolume;
    }

    public void PlaySFX(SFX sfx)
    {
        AudioClip clip = GetSFX(sfx);
        if (clip == null) return;

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            // 비어있는 플레이어 찾기
            int loopIndex = (i + channelIndex) % sfxPlayers.Length;
            if (sfxPlayers[loopIndex].isPlaying) continue;

            //Random Sound 예시
            /*
            int ranIndex = 0;
            if (sfx == SFX.Hit || sfx == SFX.Melee || sfx == SFX.Boom || sfx == SFX.Damaged) 
            {
                ranIndex = Random.Range(0, 2);
            }
            */


            channelIndex = loopIndex;
            sfxPlayers[loopIndex].clip = clip; // Random Sound 예시 : sfxPlayers[loopIndex].clip = sfxClip[(int)sfx + ranIndex];
            sfxPlayers[loopIndex].Play();
            break;
        }
    }

    public void StopSFX(SFX sfx, bool all = false)
    {
        AudioClip clip = GetSFX(sfx);
        if (clip == null) return;

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            if (sfxPlayers[i].isPlaying && sfxPlayers[i].clip == clip)
            {
                sfxPlayers[i].Stop();

                if (!all) break;
            }
        }
    }

    public AudioClip GetSFX(SFX sfx)
    {
        int index = (int)sfx;

        // System SFX Clips 배열 범위 체크
        if (index < systemSFXClips.Length)
        {
            return systemSFXClips[index];
        }
        index -= systemSFXClips.Length;

        // Player SFX Clips 배열 범위 체크
        if (index < playerSFXClips.Length)
        {
            return playerSFXClips[index];
        }
        index -= playerSFXClips.Length;

        //      MuSin SFX Clips 배열 범위 체크
        if (index < musinSFXClips.Length)
        {
            return musinSFXClips[index];
        }
        index -= musinSFXClips.Length;

        //      MoonSin SFX Clips 배열 범위 체크
        if (index < moonsinSFXClips.Length)
        {
            return moonsinSFXClips[index];
        }
        index -= moonsinSFXClips.Length;

        // Enemy SFX Clips 배열 범위 체크
        if (index < enemySFXClips.Length)
        {
            return enemySFXClips[index];
        }
        index -= enemySFXClips.Length;

        //      Gildal SFX Clips 배열 범위 체크
        if (index < gildalSFXClips.Length)
        {
            return gildalSFXClips[index];
        }
        index -= gildalSFXClips.Length;

        //      CheongRyu SFX Clips 배열 범위 체크
        if (index < cheongRyuSFXClips.Length)
        {
            return cheongRyuSFXClips[index];
        }

        // 찾지 못했을 경우
        Debug.LogWarning($"[EffectManager] GetEffect: Unknown type {sfx}");
        return null;
    }

    public void UpdateVolumeSFX()
    {
        if (sfxPlayers == null) return;

        foreach (var sfxPlayer in sfxPlayers)
        {
            sfxPlayer.volume = sfxVolume * sfxSlider.value * masterSlider.value;
        }
    }

    public void SaveVolume()
    {
        PlayerPrefs.SetFloat("Master Volume", masterSlider.value);
        PlayerPrefs.SetFloat("BGM Volume", bgmSlider.value);
        PlayerPrefs.SetFloat("SFX Volume", sfxSlider.value);
        
        PlayerPrefs.Save();
    }

    public void LoadVolume()
    {
        masterSlider.value = PlayerPrefs.GetFloat("Master Volume", 1f);
        bgmSlider.value = PlayerPrefs.GetFloat("BGM Volume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFX Volume", 1f);

        UpdateVolumeBGM();
        UpdateVolumeSFX();
    }
}
