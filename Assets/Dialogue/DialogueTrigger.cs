using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    // 대사는 줄마다 맨 앞에 [이름]을 쓴다. 예) [카린] 여기 숨어볼까?
    [TextArea]
    public string[] firstLines;       // 처음 조사할 때 대사
    public Sprite firstPanelSprite;   // 처음 대사창 배경 (비우면 기본)

    [TextArea]
    public string[] repeatLines;      // 두 번째부터 대사 (비우면 대사 없음)
    public Sprite repeatPanelSprite;  // 반복 대사창 배경 (비우면 기본)

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
        // 이미 대사 중이면 무시 (대사가 겹쳐서 시작되는 것 방지)
        if (DialogueManager.instance.IsDialogueActive) return;

        interactCanvas.SetActive(false);

        if (!hasInteracted)
        {
            hasInteracted = true;
            DialogueManager.instance.StartDialogue(firstLines, true, firstPanelSprite, autoPickup ? this : null);
        }
        else
        {
            if (repeatLines == null || repeatLines.Length == 0) return;
            DialogueManager.instance.StartDialogue(repeatLines, false, repeatPanelSprite, null);
        }
    }
}