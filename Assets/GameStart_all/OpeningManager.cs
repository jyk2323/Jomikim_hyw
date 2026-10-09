using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

// 오프닝 씬 전용: 사진(컷)을 차례로 보여주고, 각 컷마다 대사창으로 스토리 전달
// 마지막 컷이 끝나면 1층 씬으로 이동
// (1층의 DialogueManager와는 따로 동작함 — 오프닝 씬에는 DialogueManager를 넣지 말 것)
public class OpeningManager : MonoBehaviour
{
    [System.Serializable]
    public class Cut
    {
        public Sprite image;              // 이 컷에 보여줄 사진
        [TextArea(2, 5)]
        public string[] lines;            // 대사들. 줄 맨 앞에 [이름], 내레이션은 []
        public float autoNextTime = 2f;   // 대사가 없을 때 몇 초 뒤 다음 컷으로
    }

    [Header("컷 목록 (위에서부터 순서대로)")]
    public Cut[] cuts;

    [Header("UI 연결")]
    public Image cutImage;                // 화면 전체 사진
    public GameObject dialoguePanel;      // 대사창
    public TextMeshProUGUI nameText;      // 이름
    public TextMeshProUGUI dialogueText;  // 대사
    public Image fadeImage;               // 화면 전체 검은 이미지 (컷 전환 페이드용, 없어도 됨)

    [Header("설정")]
    public float typingSpeed = 0.05f;     // 글자 나오는 속도
    public float fadeTime = 0.5f;         // 컷 바뀔 때 어두워지는 시간
    public KeyCode nextKey = KeyCode.Space;
    public KeyCode skipKey = KeyCode.Escape; // 오프닝 전체 건너뛰기
    public string nextSceneName = "1F_Scene";

    private bool isTyping = false;
    private bool nextPressed = false;
    private bool isEnding = false;
    private string currentLine = "";

    void Start()
    {
        dialoguePanel.SetActive(false);
        if (fadeImage != null)
        {
            fadeImage.raycastTarget = false;
            SetFade(1f); // 처음엔 검은 화면에서 시작
        }
        StartCoroutine(PlayOpening());
    }

    void Update()
    {
        if (isEnding) return;

        if (Input.GetKeyDown(skipKey))
        {
            StopAllCoroutines();
            StartCoroutine(EndOpening());
            return;
        }

        if (Input.GetKeyDown(nextKey) || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // 타이핑 중이면 문장 한 번에 다 보여주기
                isTyping = false;
                dialogueText.text = currentLine;
            }
            else
            {
                nextPressed = true;
            }
        }
    }

    IEnumerator PlayOpening()
    {
        for (int c = 0; c < cuts.Length; c++)
        {
            Cut cut = cuts[c];

            // 컷 전환: 어둡게 → 사진 교체 → 밝게
            if (c > 0) yield return Fade(0f, 1f);
            dialoguePanel.SetActive(false);
            if (cutImage != null) cutImage.sprite = cut.image;
            yield return Fade(1f, 0f);

            if (cut.lines == null || cut.lines.Length == 0)
            {
                // 대사 없는 컷: 잠시 보여주고 넘어감 (Space로 빨리 넘기기 가능)
                nextPressed = false;
                float t = 0f;
                while (t < cut.autoNextTime && !nextPressed)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
                continue;
            }

            // 대사 있는 컷
            dialoguePanel.SetActive(true);
            SetSpeaker(null); // 컷마다 이름칸 초기화

            foreach (string rawLine in cut.lines)
            {
                // 규칙은 1층 대사창과 같음
                // [이름] 대사 → 화자 변경 / [이름] 없음 → 앞 사람 계속 / [] → 내레이션
                string speaker;
                string line = DialogueManager.ParseLine(rawLine, out speaker);
                if (speaker != null) SetSpeaker(speaker);

                yield return TypeLine(line);

                // 다음 입력 기다리기
                nextPressed = false;
                while (!nextPressed) yield return null;
            }
        }

        yield return EndOpening();
    }

    void SetSpeaker(string speakerName)
    {
        bool hasName = !string.IsNullOrEmpty(speakerName) && speakerName.Trim('[', ']', ' ') != "";
        nameText.gameObject.SetActive(hasName);
        if (hasName) nameText.text = speakerName;
    }

    IEnumerator TypeLine(string line)
    {
        currentLine = line;
        isTyping = true;
        dialogueText.text = "";

        foreach (char ch in line)
        {
            if (!isTyping) yield break; // 중간에 Space 누르면 Update에서 전체 표시
            dialogueText.text += ch;
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }

    IEnumerator EndOpening()
    {
        isEnding = true;
        dialoguePanel.SetActive(false);
        yield return Fade(GetFade(), 1f);
        SceneManager.LoadScene(nextSceneName);
    }

    // ───────── 페이드 ─────────
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