using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public enum BGM 
    { 
        Main, 
        OutPost, 
        Gildal, 
        CheongRyu 
    }

    [Header(" === BGM Settings === ")]
    public AudioClip bgmClip;
    public float bgmVolume = 0.5f;
    AudioSource bgmPlayer;


    public enum SFX
    {
        // System


        // Player
        // - Musin

        // - Moonsin


        // Enemy
        // - Gildal

        // - CheongRyu
    }

    [Header(" === SFX Settings === ")]
    public AudioClip[] sfxClip;
    public float sfxVolume = 0.2f;
    public int channelCount = 5;
    AudioSource[] sfxPlayers;
    int channelIndex = 0;

    [Header(" === Volume Sliders === ")]
    [Range(0, 1)] public float bgmSlider;
    [Range(0, 1)] public float sfxSlider;


    private void Awake()
    {
        // 인스턴스 초기화
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // 플레이어 초기화
            Init();

            // 플레이어 볼륨 로드
            LoadVolume();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Init()
    {
        // BGM 플레이어 초기화
        GameObject bgmObject = new GameObject("BGM Player");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClip;

        // SFX 플레이어 초기화
        GameObject sfxObject = new GameObject("SFX Player");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channelCount];

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            sfxPlayers[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].loop = false;
            sfxPlayers[i].volume = sfxVolume;
        }
    }

    public void PlayBGM()
    {
        if (bgmPlayer == null) return;

        // 오디오 페이드
        /*
        AudioClip nextClip = null ; // 변경할 클립 설정
        switch (bgmName)
        {
            case "Start":
            case "Map":
                nextClip = mainClip;
                break;
            case "Stage":
                nextClip = battleClip;
                break;
            case "Boss":
                nextClip = bossClip;
                break;
        }

        if (bgmPlayer.clip == nextClip) return;

        FadeBGM(nextClip, 1f);
        */

        bgmPlayer.Play();
    }

    public void UpdateVolumeBGM()
    {
        if (bgmPlayer == null) return;
        
        bgmPlayer.volume = bgmVolume * bgmSlider;
    }

    public void FadeBGM(AudioClip nextClip, float fadeTime = 1f)
    {
        StartCoroutine(Co_FadeBGM(nextClip, fadeTime));
    }

    IEnumerator Co_FadeBGM(AudioClip newClip, float fadeTime)
    {
        if (!bgmPlayer.isPlaying) yield break;

        // 페이드 아웃
        float startVolume = bgmPlayer.volume;
        float time = 0f;
        while (time < fadeTime)
        {
            time += Time.deltaTime;
            bgmPlayer.volume = Mathf.Lerp(startVolume, 0f, time / fadeTime);

            yield return null;
        }
        bgmPlayer.Stop();
        bgmPlayer.volume = 0f;

        // 다음 클립 세팅
        bgmPlayer.clip = newClip;
        bgmPlayer.Play();

        // 페이드 인
        float targetVolume = bgmVolume * bgmSlider;
        time = 0f;
        while (time < fadeTime)
        {
            time += Time.deltaTime;
            bgmPlayer.volume = Mathf.Lerp(0f, targetVolume, time / fadeTime);

            yield return null;
        }
        bgmPlayer.volume = targetVolume;
    }

    public void PlaySFX(SFX sfx)
    {
        for (int i = 0; i < sfxPlayers.Length; i++)
        {
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
            sfxPlayers[loopIndex].clip = sfxClip[(int)sfx]; // Random Sound 예시 : sfxPlayers[loopIndex].clip = sfxClip[(int)sfx + ranIndex];
            sfxPlayers[loopIndex].Play();
            break;
        }
    }

    public void UpdateVolumeSFX()
    {
        if (sfxPlayers == null) return;

        foreach (var sfxPlayer in sfxPlayers)
        {
            sfxPlayer.volume = sfxVolume * sfxSlider;
        }
    }

    public void SaveVolume()
    {
        PlayerPrefs.SetFloat("BGM Volume", bgmSlider);
        PlayerPrefs.SetFloat("SFX Volume", sfxSlider);
        
        PlayerPrefs.Save();
    }

    public void LoadVolume()
    {
        bgmSlider = PlayerPrefs.GetFloat("BGM Volume", 0.5f);
        sfxSlider = PlayerPrefs.GetFloat("SFX Volume", 0.2f);

        UpdateVolumeBGM();
        UpdateVolumeSFX();
    }
}
