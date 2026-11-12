using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System;

public class OpeningManager : MonoBehaviour
{
    [Header("컷 데이터 에셋 (연결하면 아래 Cuts보다 우선)")]
    public CutsceneData cutsceneData;

    [Header("컷 목록 (위에서부터 순서대로)")]
    public CutsceneData.Cut[] cuts;

    [Header("UI 연결")]
    public Image cutImage;
    public GameObject dialoguePanel;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image fadeImage;                // 화면 전체 검은 이미지 (컷 전환 페이드용, 없어도 됨)
    public GameObject skipButtonObj;      // ← 추가: 오프닝용 SKIP 버튼

    [Header("주요 인물 얼굴 (여기 등록된 이름만 얼굴이 나옴)")]
    public Image characterImage;
    public DialogueManager.Speaker[] speakers;

    [Header("설정")]
    public float typingSpeed = 0.05f;
    public float fadeTime = 0.5f;
    //public Key nextKey = Key.Space;
    //public Key skipKey = Key.Escape;
    public string nextSceneName = "1F_Scene";

    // ← 추가: 타이핑 소리 (1층 대사창 DialogueManager와 같은 방식)
    // Loop    : 긴 타이핑 음원을 문장이 나오는 동안 반복 재생, 출력 끝나면 정지
    // PerChar : 짧은 "톡" 소리를 몇 글자마다 한 번씩 재생
    [Header("타이핑 소리 (볼륨은 시작화면 설정의 효과음 슬라이더를 따름)")]
    public AudioClip typingSound;
    public DialogueManager.TypingSoundMode soundMode = DialogueManager.TypingSoundMode.Loop;
    [Tooltip("Loop 방식: 매 문장 음원의 다른 지점부터 재생")]
    public bool randomStartPoint = true;
    [Tooltip("PerChar 방식: 몇 글자마다 소리 낼지 (1 = 매 글자)")]
    public int soundEveryChars = 2;
    [Range(0f, 0.3f)] public float pitchRandom = 0.05f;
    private AudioSource typingAudio;

    private bool isTyping = false;
    private bool nextPressed = false;
    private bool isEnding = false;
    private bool skipCut = false;
    private string currentLine = "";

    CutsceneData.Cut[] GetCuts()
    {
        if (cutsceneData != null && cutsceneData.cuts != null && cutsceneData.cuts.Length > 0)
            return cutsceneData.cuts;
        return cuts != null ? cuts : new CutsceneData.Cut[0];
    }

#if UNITY_EDITOR
    // 컴포넌트 우클릭 메뉴에서 실행: 현재 Cuts를 에셋 파일로 복사
    [ContextMenu("Cuts를 Cutscene Data 에셋으로 복사")]
    void CopyCutsToAsset()
    {
        if (cutsceneData == null || cuts == null)
        {
            Debug.LogWarning("Cutscene Data 칸에 에셋을 먼저 연결하세요.");
            return;
        }
        var copy = new CutsceneData.Cut[cuts.Length];
        for (int i = 0; i < cuts.Length; i++)
        {
            copy[i] = new CutsceneData.Cut
            {
                image = cuts[i].image,
                lines = cuts[i].lines != null ? (string[])cuts[i].lines.Clone() : null,
                autoNextTime = cuts[i].autoNextTime
            };
        }
        cutsceneData.cuts = copy;
        UnityEditor.EditorUtility.SetDirty(cutsceneData);
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log("컷 " + copy.Length + "개를 에셋으로 복사했습니다.");
    }
#endif

    void Start()
    {
        // ← 추가: 타이핑 소리용 AudioSource
        typingAudio = GetComponent<AudioSource>();
        if (typingAudio == null) typingAudio = gameObject.AddComponent<AudioSource>();
        typingAudio.playOnAwake = false;

        dialoguePanel.SetActive(false);
        if (skipButtonObj != null) skipButtonObj.SetActive(false);   // ← 추가
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = false;
            SetFade(1f);
        }
        StartCoroutine(PlayOpening());
    }

    void Update()
    {
        if (isEnding) return;

        // Esc: 오프닝 전체 건너뛰기
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SkipOpening();
            return;
        }

        if (DialogueInput.NextPressed())
        {
            if (isTyping)
            {
                isTyping = false;
                dialogueText.text = currentLine;
                StopTypingSound();   // ← 추가: 문장 한 번에 표시 → 소리도 정지
            }
            else
            {
                nextPressed = true;
            }
        }
    }

    public void SkipOpening()
    {
        if (isEnding) return;
        skipCut = true;
        isTyping = false;
        nextPressed = true;
        StopTypingSound();   // ← 추가
    }

    IEnumerator PlayOpening()
    {
        CutsceneData.Cut[] activeCuts = GetCuts();

        for (int c = 0; c < activeCuts.Length; c++)
        {
            CutsceneData.Cut cut = activeCuts[c];

            if (c > 0) yield return Fade(0f, 1f);
            dialoguePanel.SetActive(false);
            if (cutImage != null) cutImage.sprite = cut.image;
            yield return Fade(1f, 0f);
            if (skipButtonObj != null) skipButtonObj.SetActive(true);   // ← 추가

            if (cut.lines == null || cut.lines.Length == 0)
            {
                nextPressed = false;
                float t = 0f;
                while (t < cut.autoNextTime && !nextPressed)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
                continue;
            }

            dialoguePanel.SetActive(true);
            SetSpeaker(null);
            skipCut = false;

            foreach (string rawLine in cut.lines)
            {
                if (skipCut) break;

                string speaker;
                string line = DialogueManager.ParseLine(rawLine, out speaker);
                if (speaker != null) SetSpeaker(speaker);

                yield return TypeLine(line);

                nextPressed = false;
                while (!nextPressed && !skipCut) yield return null;
            }
        }

        yield return EndOpening();
    }

    void SetSpeaker(string speakerName)
    {
        string pureName = string.IsNullOrEmpty(speakerName) ? "" : speakerName.Trim('[', ']', ' ');
        bool hasName = pureName != "";

        nameText.gameObject.SetActive(hasName);
        if (hasName) nameText.text = speakerName;

        SetPortrait(pureName);
    }

    // 목록에 있는 주요 인물만 얼굴을 보여주고, 내레이션/NPC는 숨김
    void SetPortrait(string pureName)
    {
        if (characterImage == null) return;

        if (pureName != "" && speakers != null)
        {
            foreach (DialogueManager.Speaker s in speakers)
            {
                if (s.name == pureName && s.portrait != null)
                {
                    characterImage.sprite = s.portrait;
                    characterImage.enabled = true;
                    return;
                }
            }
        }

        characterImage.enabled = false;
    }

    IEnumerator TypeLine(string line)
    {
        currentLine = line;
        isTyping = true;
        dialogueText.text = "";

        // ← 추가: Loop 방식이면 문장 시작할 때 소리 시작
        if (soundMode == DialogueManager.TypingSoundMode.Loop) StartLoopSound();

        int count = 0;
        foreach (char ch in line)
        {
            if (!isTyping) { StopTypingSound(); yield break; }
            dialogueText.text += ch;

            // ← 추가: PerChar 방식이면 몇 글자마다 "톡"
            if (soundMode == DialogueManager.TypingSoundMode.PerChar && !char.IsWhiteSpace(ch))
            {
                if (count % Mathf.Max(1, soundEveryChars) == 0) PlayTypingSound();
                count++;
            }

            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
        StopTypingSound();   // ← 추가: 문장 출력 끝 → 소리 정지
    }

    // ───────── ← 추가: 타이핑 소리 ─────────
    void StopTypingSound()
    {
        if (typingAudio != null) typingAudio.Stop();
    }

    void StartLoopSound()
    {
        if (typingSound == null || typingAudio == null) return;

        typingAudio.clip = typingSound;
        typingAudio.loop = true;
        typingAudio.pitch = 1f;
        typingAudio.volume = AudioManager.SfxVolume;
        typingAudio.time = randomStartPoint ? UnityEngine.Random.Range(0f, typingSound.length * 0.9f) : 0f;
        typingAudio.Play();
    }

    void PlayTypingSound()
    {
        if (typingSound == null || typingAudio == null) return;
        typingAudio.loop = false;
        typingAudio.pitch = 1f + UnityEngine.Random.Range(-pitchRandom, pitchRandom);
        typingAudio.PlayOneShot(typingSound, AudioManager.SfxVolume);
    }

    IEnumerator EndOpening()
    {
        isEnding = true;
        StopTypingSound();   // ← 추가
        if (skipButtonObj != null) skipButtonObj.SetActive(false);   // ← 추가
        dialoguePanel.SetActive(false);
        yield return Fade(GetFade(), 1f);
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator Fade(float from, float to)
    {
        if (fadeImage == null) yield break;

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            SetFade(Mathf.Lerp(from, to, t / fadeTime));
            yield return null;
        }
        SetFade(to);
    }

    void SetFade(float a)
    {
        if (fadeImage == null) return;
        Color col = fadeImage.color;
        col.a = a;
        fadeImage.color = col;
    }

    float GetFade()
    {
        return fadeImage != null ? fadeImage.color.a : 0f;
    }
}