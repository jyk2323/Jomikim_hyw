using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioSource bgmSource;    // BGM용 오디오소스
    public AudioSource sfxSource;    // 효과음용 오디오소스

    // BGM 슬라이더 조절
    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = volume;
    }

    // 효과음 슬라이더 조절
    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}