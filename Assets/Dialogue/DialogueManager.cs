using UnityEngine;
using TMPro;
using UnityEngine.UI;
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
    }

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = currentText;
                isTyping = false;
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

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void NextLine()
    {
        currentIndex++;

        if (currentIndex >= currentLines.Length)
        {
            isDialogueActive = false;
            dialoguePanel.SetActive(false);

            // 대사 끝나고 등록된 함수 호출!
            OnDialogueEnd?.Invoke();
            OnDialogueEnd = null;

            // 대사 끝나고 자동 획득!
            if (currentTrigger != null && currentTrigger.autoPickup)
            {
                Inventory.instance.AddItem(currentTrigger.itemName, currentTrigger.itemSprite);
                Destroy(currentTrigger.gameObject);
            }

            currentTrigger = null;
        }
        else
        {
            ShowLine(currentLines[currentIndex]);
        }
    }
}