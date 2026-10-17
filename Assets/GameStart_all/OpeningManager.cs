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
    public Image fadeImage;

    [Header("주요 인물 얼굴 (여기 등록된 이름만 얼굴이 나옴)")]
    public Image characterImage;
    public DialogueManager.Speaker[] speakers;

    [Header("설정")]
    public float typingSpeed = 0.05f;
    public float fadeTime = 0.5f;
    //public Key nextKey = Key.Space;
    //public Key skipKey = Key.Escape;
    public string nextSceneName = "1F_Scene";

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
        dialoguePanel.SetActive(false);
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

        foreach (char ch in line)
        {
            if (!isTyping) yield break;
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