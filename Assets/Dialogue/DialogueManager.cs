using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    public GameObject dialoguePanel;
    public Image dialoguePanelImage;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image characterImage;

    public float typingSpeed = 0.05f;

    private string[] currentLines;
    private int currentIndex = 0;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private bool useTypingEffect = true;
    private DialogueTrigger currentTrigger; // 대사 끝나고 획득할 사물!

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
                dialogueText.text = currentLines[currentIndex];
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    public void StartDialogue(string speakerName, Sprite speakerImage, string[] lines, bool useTyping, Sprite panelSprite, DialogueTrigger trigger)
    {
        currentLines = lines;
        currentIndex = 0;
        isDialogueActive = true;
        useTypingEffect = useTyping;
        currentTrigger = trigger;
        nameText.text = speakerName;

        if (speakerImage != null)
            characterImage.sprite = speakerImage;

        if (panelSprite != null)
            dialoguePanelImage.sprite = panelSprite;

        dialoguePanel.SetActive(true);
        ShowLine(currentLines[currentIndex]);
    }

    void ShowLine(string line)
    {
        if (useTypingEffect)
        {
            StartCoroutine(TypeLine(line));
        }
        else
        {
            dialogueText.text = line;
        }
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