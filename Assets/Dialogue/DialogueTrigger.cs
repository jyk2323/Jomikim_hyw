using UnityEngine;
using System.Collections.Generic;

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

    [Header("자동 시작 (체크하면 닿자마자 대사가 나옴. 버튼 필요 없음)")]
    public bool autoStart = false;

    [Header("한 번만 나오게 (자동 시작과 같이 사용. 다른 씬에 갔다 와도 유지)")]
    public bool playOnce = false;

    public bool autoPickup = false; // 체크하면 대사 끝나고 자동 획득!
    public string itemName;
    public Sprite itemSprite;

    private bool hasInteracted = false;

    // 이미 본 대사를 기억하는 목록 (씬이 바뀌어도 유지, 게임을 끄면 초기화)
    private static HashSet<string> playedKeys = new HashSet<string>();

    string PlayedKey
    {
        get { return gameObject.scene.name + "/" + gameObject.name; }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (autoStart)
        {
            // 이미 한 번 본 구역이면 무시
            if (playOnce && playedKeys.Contains(PlayedKey)) return;

            OnInteractButtonClick();   // 버튼 없이 바로 대사 시작
            return;
        }

        if (interactCanvas != null) interactCanvas.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (interactCanvas != null) interactCanvas.SetActive(false);
    }

    public void OnInteractButtonClick()
    {
        if (DialogueManager.instance == null)
        {
            Debug.LogWarning("DialogueManager가 없습니다. 1층(또는 Start)에서 시작했는지 확인하세요.");
            return;
        }

        // 이미 대사 중이면 무시 (대사가 겹쳐서 시작되는 것 방지)
        if (DialogueManager.instance.IsDialogueActive) return;

        if (interactCanvas != null) interactCanvas.SetActive(false);

        if (!hasInteracted)
        {
            hasInteracted = true;
            if (playOnce) playedKeys.Add(PlayedKey);   // 본 구역으로 기록
            DialogueManager.instance.StartDialogue(firstLines, true, firstPanelSprite, autoPickup ? this : null);
        }
        else
        {
            if (repeatLines == null || repeatLines.Length == 0) return;
            DialogueManager.instance.StartDialogue(repeatLines, false, repeatPanelSprite, null);
        }
    }
}