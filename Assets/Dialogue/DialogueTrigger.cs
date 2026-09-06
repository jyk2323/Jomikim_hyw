using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public string speakerName;
    public Sprite speakerImage;
    public Sprite firstPanelSprite;

    public string repeatSpeakerName;
    public Sprite repeatSpeakerImage;
    public Sprite repeatPanelSprite;

    [TextArea]
    public string[] firstLines;

    [TextArea]
    public string[] repeatLines;

    public GameObject interactCanvas;

    public bool autoPickup = false; // 체크하면 대사 끝나고 자동 획득!
    public string itemName;
    public Sprite itemSprite;

    private bool hasInteracted = false;

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("충돌 감지! : " + collision.gameObject.name);
        if (!collision.CompareTag("Player")) return;
        Debug.Log("플레이어 감지!");
        interactCanvas.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        interactCanvas.SetActive(false);
    }

    public void OnInteractButtonClick()
    {
        interactCanvas.SetActive(false);

        if (!hasInteracted)
        {
            hasInteracted = true;
            DialogueManager.instance.StartDialogue(speakerName, speakerImage, firstLines, true, firstPanelSprite, autoPickup ? this : null);
        }
        else
        {
            if (repeatLines == null || repeatLines.Length == 0) return;
            DialogueManager.instance.StartDialogue(repeatSpeakerName, repeatSpeakerImage, repeatLines, false, repeatPanelSprite, null);
        }
    }
}