using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    // 이름 → 얼굴 그림 표 (대사 앞에 [이름]을 쓰면 여기서 그림을 찾아 바꿈)
    [System.Serializable]
    public class Speaker
    {
        public string name;       // 대괄호 없이 이름만 (예: 카린)
        public Sprite portrait;   // 그 사람 얼굴 그림
    }

    public GameObject dialoguePanel;
    public Image dialoguePanelImage;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image characterImage;
    public System.Action OnDialogueEnd;

    [Header("등장인물 목록 (여러 명 대화용)")]
    public Speaker[] speakers;

    public float typingSpeed = 0.05f;

    // 타이핑 소리 방식
    // Loop    : 긴 타이핑 음원(예: 4초)을 문장이 나오는 동안 반복 재생, 출력 끝나면 정지
    // PerChar : 짧은 "톡" 소리를 몇 글자마다 한 번씩 재생
    public enum TypingSoundMode { Loop, PerChar }

    [Header("타이핑 소리 (볼륨은 시작화면 설정의 효과음 슬라이더를 따름)")]
    public AudioClip typingSound;            // 타이핑 효과음
    public TypingSoundMode soundMode = TypingSoundMode.Loop;

    [Header("Loop 방식 설정")]
    public bool randomStartPoint = true;     // 매 문장 음원의 다른 지점부터 재생 (항상 똑같이 들리지 않게)

    [Header("PerChar 방식 설정")]
    public int soundEveryChars = 2;          // 몇 글자마다 소리 낼지 (1 = 매 글자)
    [Range(0f, 0.3f)] public float pitchRandom = 0.05f; // 소리 높낮이를 살짝 랜덤하게
    private AudioSource typingAudio;

    [Header("스킵")]
    public Button skipButton;          // 대사창의 SKIP 버튼 (없어도 됨)
    public Key skipKey = Key.Escape;   // 키보드 스킵 키

    private string[] currentLines;
    private int currentIndex = 0;
    private bool isDialogueActive = false;

    // 다른 스크립트가 "지금 대사 중인가?"를 확인할 때 사용
    public bool IsDialogueActive => isDialogueActive;
    private bool isTyping = false;
    private bool useTypingEffect = true;
    private DialogueTrigger currentTrigger; // 대사 끝나고 획득할 사물!
    private string currentText = "";        // 지금 줄에서 [이름]을 뺀 실제 대사

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        dialoguePanel.SetActive(false);

        // 타이핑 소리용 AudioSource (없으면 자동으로 붙임)
        typingAudio = GetComponent<AudioSource>();
        if (typingAudio == null) typingAudio = gameObject.AddComponent<AudioSource>();
        typingAudio.playOnAwake = false;
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipDialogue);
    }

    void Update()
    {
        if (!isDialogueActive) return;
        if (Keyboard.current != null && Keyboard.current[skipKey].wasPressedThisFrame)
        {
            SkipDialogue();
            return;
        }

        if (DialogueInput.NextPressed())
        {
            if (isTyping)
            {
                // 타이핑 중에 누르면 한 번에 다 보여줌
                StopAllCoroutines();
                dialogueText.text = currentText;
                isTyping = false;
                StopTypingSound(); // 문장 한 번에 표시 → 소리도 즉시 정지
            }
            else
            {
                NextLine();
            }
        }
    }

    // 대사 시작. 말하는 사람은 각 줄 맨 앞의 [이름]으로 정함
    public void StartDialogue(string[] lines, bool useTyping, Sprite panelSprite, DialogueTrigger trigger)
    {
        if (lines == null || lines.Length == 0) return;

        currentLines = lines;
        currentIndex = 0;
        isDialogueActive = true;
        useTypingEffect = useTyping;
        currentTrigger = trigger;

        // 시작할 때는 이름/얼굴을 비워두고, 첫 줄의 [이름]으로 채움
        nameText.text = "";
        characterImage.enabled = false;

        if (panelSprite != null)
            dialoguePanelImage.sprite = panelSprite;

        dialoguePanel.SetActive(true);
        ShowLine(currentLines[currentIndex]);
    }

    void ShowLine(string line)
    {
        // "[레인] 드디어 만났군" → 이름: [레인], 대사: 드디어 만났군
        string speaker;
        currentText = ParseLine(line, out speaker);

        if (speaker != null)
            ChangeSpeaker(speaker);

        if (useTypingEffect)
        {
            StartCoroutine(TypeLine(currentText));
        }
        else
        {
            dialogueText.text = currentText;
        }
    }

    // 줄 맨 앞이 [이름]이면 이름과 대사를 나눔. 없으면 speaker = null (앞 사람이 계속 말함)
    // [] (빈 괄호)는 내레이션: 이름/얼굴 없이 대사만
    public static string ParseLine(string line, out string speaker)
    {
        speaker = null;
        if (string.IsNullOrEmpty(line)) return "";

        string trimmed = line.TrimStart();
        if (trimmed.StartsWith("["))
        {
            int end = trimmed.IndexOf(']');
            if (end >= 1)
            {
                speaker = trimmed.Substring(0, end + 1);          // "[레인]"
                return trimmed.Substring(end + 1).TrimStart();    // "드디어 만났군"
            }
        }
        return line;
    }

    // 이름칸 바꾸고, 등장인물 목록에서 얼굴을 찾아 바꿈
    // 목록에 없는 사람(예: [???], [경호원])이면 얼굴을 숨김
    void ChangeSpeaker(string speakerWithBrackets)
    {
        string pureName = speakerWithBrackets.Trim('[', ']', ' ');

        // [] = 내레이션
        if (pureName == "")
        {
            nameText.text = "";
            characterImage.enabled = false;
            return;
        }

        nameText.text = speakerWithBrackets;

        if (speakers != null)
        {
            foreach (Speaker s in speakers)
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
        isTyping = true;
        dialogueText.text = "";

        // Loop 방식: 문장 시작할 때 음원 반복 재생 시작
        if (soundMode == TypingSoundMode.Loop) StartLoopSound();

        int count = 0;
        foreach (char c in line)
        {
            dialogueText.text += c;

            // PerChar 방식: 공백이 아닌 글자마다 (soundEveryChars 간격으로) 타이핑 소리
            if (soundMode == TypingSoundMode.PerChar && !char.IsWhiteSpace(c))
            {
                if (count % Mathf.Max(1, soundEveryChars) == 0) PlayTypingSound();
                count++;
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        StopTypingSound(); // 문장 출력 끝 → 소리 즉시 정지
    }

    void StopTypingSound()
    {
        if (typingAudio != null) typingAudio.Stop();
    }

    // 음원 끝까지 재생되면 처음부터 자동으로 다시 재생 (loop)
    void StartLoopSound()
    {
        if (typingSound == null || typingAudio == null) return;

        typingAudio.clip = typingSound;
        typingAudio.loop = true;
        typingAudio.pitch = 1f;
        typingAudio.volume = AudioManager.SfxVolume;
        typingAudio.time = randomStartPoint ? Random.Range(0f, typingSound.length * 0.9f) : 0f;
        typingAudio.Play();
    }

    void PlayTypingSound()
    {
        if (typingSound == null || typingAudio == null) return;
        typingAudio.loop = false;
        typingAudio.pitch = 1f + Random.Range(-pitchRandom, pitchRandom);
        typingAudio.PlayOneShot(typingSound, AudioManager.SfxVolume);
    }

    // 버튼과 키가 같이 쓰는 스킵 함수
    public void SkipDialogue()
    {
        if (!isDialogueActive) return;

        StopAllCoroutines();
        isTyping = false;
        StopTypingSound();
        EndDialogue();
    }

    void NextLine()
    {
        currentIndex++;

        if (currentIndex >= currentLines.Length)
            EndDialogue();
        else
            ShowLine(currentLines[currentIndex]);
    }

    void EndDialogue()
    {
        StopTypingSound();
        isDialogueActive = false;
        dialoguePanel.SetActive(false);

        OnDialogueEnd?.Invoke();
        OnDialogueEnd = null;

        if (currentTrigger != null && currentTrigger.autoPickup)
        {
            Inventory.instance.AddItem(currentTrigger.itemName, currentTrigger.itemSprite);
            Destroy(currentTrigger.gameObject);
        }

        currentTrigger = null;
    }
}   // ← 클래스 닫는 중괄호 (이게 파일 맨 끝에 딱 하나만 있어야 해요)

    //IEnumerator TypeLine(string line)
    //{
    //    isTyping = true;
    //    dialogueText.text = "";

    //    foreach (char c in line)
    //    {
    //        dialogueText.text += c;
    //        yield return new WaitForSeconds(typingSpeed);
    //    }

    //    isTyping = false;
    //}

    //void NextLine()
    //{
    //    //currentIndex++;

    //    //if (currentIndex >= currentLines.Length)
    //    //{
    //    //    isDialogueActive = false;
    //    //    dialoguePanel.SetActive(false);

    //    //    // 대사 끝나고 등록된 함수 호출!
    //    //    OnDialogueEnd?.Invoke();
    //    //    OnDialogueEnd = null;

    //    //    // 대사 끝나고 자동 획득!
    //    //    if (currentTrigger != null && currentTrigger.autoPickup)
    //    //    {
    //    //        Inventory.instance.AddItem(currentTrigger.itemName, currentTrigger.itemSprite);
    //    //        Destroy(currentTrigger.gameObject);
    //    //    }

    //    //    currentTrigger = null;
    //    //}
    //    //else
    //    //{
    //    //    ShowLine(currentLines[currentIndex]);
    //    //}
//    }
//}