using UnityEngine;
using System.Collections;

// 씬 BGM: 작게 시작해서 점점 커지다가 Start 설정창의 BGM 볼륨에서 멈춤
// BGM 오브젝트(AudioSource 있는 곳)에 붙이기
[RequireComponent(typeof(AudioSource))]
public class SceneBgm : MonoBehaviour
{
    [Tooltip("소리가 다 커지기까지 걸리는 시간(초)")]
    public float fadeInTime = 3f;

    private AudioSource source;

    IEnumerator Start()
    {
        source = GetComponent<AudioSource>();
        source.loop = true;
        source.volume = 0f;
        if (!source.isPlaying) source.Play();

        // 0 → 설정한 BGM 볼륨까지 천천히
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, AudioManager.BgmVolume, t / fadeInTime);
            yield return null;
        }
        source.volume = AudioManager.BgmVolume;
    }
}