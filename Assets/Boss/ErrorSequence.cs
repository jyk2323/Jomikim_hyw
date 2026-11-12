using UnityEngine;
using System.Collections;

public class ErrorSequence : MonoBehaviour
{
    public static ErrorSequence instance;

    public GameObject[] errorPanels; // 에러창들 Inspector에서 연결!
    public float errorInterval = 0.5f; // 에러창 뜨는 간격

    public string nextSceneName; // 보스전 씬 이름

    [Header("보스전 씬에서 카린이 도착할 위치 (발사 지점과 떨어진 곳)")]
    public Vector2 spawnPoint = new Vector2(0f, -3f);

    [Header("에러창 뜰 때 효과음 (볼륨은 시작화면 효과음 슬라이더를 따름)")]
    public AudioClip errorSound;
    [Range(0f, 1f)] public float errorVolume = 1f;
    [Tooltip("소리 높낮이 (1 = 원래 소리 / 1보다 작으면 낮고 느리게 / 크면 높고 빠르게)")]
    [Range(0.3f, 2f)] public float pitch = 1f;
    [Range(0f, 0.3f)] public float pitchRandom = 0f;      // 매번 높낮이 살짝 다르게 (0 = 항상 원래 소리)
    [Tooltip("체크: 음원 앞의 조용한 부분을 자동으로 찾아서 건너뜀")]
    public bool autoSkipSilence = true;
    [Tooltip("직접 건너뛸 시간(초). Auto Skip Silence를 끄면 이 값을 사용")]
    public float skipStart = 0f;

    // 스피커 여러 개를 돌려 씀 → 소리가 서로 끊지 않고 겹쳐서 다 들림
    private AudioSource[] speakers;
    private int nextSpeaker = 0;
    private float startTime = 0f;

    void Awake()
    {
        instance = this;

        // 효과음 전용 스피커 6개
        speakers = new AudioSource[6];
        for (int i = 0; i < speakers.Length; i++)
        {
            speakers[i] = gameObject.AddComponent<AudioSource>();
            speakers[i].playOnAwake = false;
        }

        startTime = autoSkipSilence ? FindSoundStart(errorSound) : skipStart;
    }

    // 음원에서 실제 소리가 시작되는 시간 찾기 (앞의 조용한 부분 건너뛰기)
    float FindSoundStart(AudioClip clip)
    {
        if (clip == null) return 0f;
        if (clip.loadState != AudioDataLoadState.Loaded) clip.LoadAudioData();

        float[] data = new float[clip.samples * clip.channels];
        if (!clip.GetData(data, 0)) return skipStart;   // 못 읽으면 직접 넣은 값 사용

        for (int i = 0; i < data.Length; i++)
        {
            if (Mathf.Abs(data[i]) > 0.02f)
                return (float)(i / clip.channels) / clip.frequency;
        }
        return 0f;
    }

    public void StartErrors()
    {
        StartCoroutine(ShowErrors());
    }

    IEnumerator ShowErrors()
    {
        // 에러창 순서대로 띄우기
        foreach (GameObject panel in errorPanels)
        {
            panel.SetActive(true);
            PlayErrorSound();
            yield return new WaitForSeconds(errorInterval);
        }

        // 에러창 다 뜨면 잠깐 대기
        yield return new WaitForSeconds(1f);

        // Fade Out 후 씬 이동! (도착 위치를 spawnPoint로)
        FadeManager.instance.LoadScene(nextSceneName, spawnPoint);
    }

    // 에러창 뜰 때마다 소리 재생 (앞의 조용한 부분은 건너뛰고, 이전 소리는 끊지 않음)
    void PlayErrorSound()
    {
        if (errorSound == null) return;

        AudioSource sp = speakers[nextSpeaker];
        nextSpeaker = (nextSpeaker + 1) % speakers.Length;

        sp.Stop();
        sp.clip = errorSound;
        sp.volume = errorVolume * AudioManager.SfxVolume;
        sp.pitch = pitch + Random.Range(-pitchRandom, pitchRandom);
        sp.time = Mathf.Clamp(startTime, 0f, errorSound.length - 0.01f);
        sp.Play();
    }
}