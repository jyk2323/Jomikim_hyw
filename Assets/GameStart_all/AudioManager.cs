using UnityEngine;
using UnityEngine.UI;

// 시작화면 설정창의 볼륨 조절.
// 정한 볼륨은 게임 전체에서 쓰이고(AudioManager.SfxVolume 등), 게임을 껐다 켜도 기억됨.
public class AudioManager : MonoBehaviour
{
    public AudioSource bgmSource;    // BGM용 오디오소스
    public AudioSource sfxSource;    // 효과음용 오디오소스

    [Header("슬라이더 (연결하면 저장된 볼륨으로 위치를 맞춰줌)")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    // 게임 전체에서 쓰는 볼륨 (0 ~ 1). 처음 읽을 때 저장된 값을 불러옴
    private static float bgm = 1f, sfx = 1f;
    private static bool loaded = false;

    public static float BgmVolume { get { Load(); return bgm; } private set { bgm = value; } }
    public static float SfxVolume { get { Load(); return sfx; } private set { sfx = value; } }

    void Awake()
    {
        Load();

        if (bgmSource != null) bgmSource.volume = BgmVolume;
        if (sfxSource != null) sfxSource.volume = SfxVolume;

        // 슬라이더 위치를 저장된 값으로 (이때 OnValueChanged가 불려도 같은 값이라 문제 없음)
        if (bgmSlider != null) bgmSlider.value = BgmVolume;
        if (sfxSlider != null) sfxSlider.value = SfxVolume;
    }

    static void Load()
    {
        if (loaded) return;
        loaded = true;
        bgm = PlayerPrefs.GetFloat("BgmVolume", 1f);
        sfx = PlayerPrefs.GetFloat("SfxVolume", 1f);
    }

    // BGM 슬라이더 조절
    public void SetBGMVolume(float volume)
    {
        BgmVolume = volume;
        if (bgmSource != null) bgmSource.volume = volume;
        PlayerPrefs.SetFloat("BgmVolume", volume);
    }

    // 효과음 슬라이더 조절 (대사 타이핑 소리 등 모든 효과음에 적용)
    public void SetSFXVolume(float volume)
    {
        SfxVolume = volume;
        if (sfxSource != null) sfxSource.volume = volume;
        PlayerPrefs.SetFloat("SfxVolume", volume);
    }
}