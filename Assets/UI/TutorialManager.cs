using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private CanvasGroup tutorialPanelGroup;
    [SerializeField] private PlayerMove playerMove;

    [Header("튜토리얼 직후 대사")]
    [TextArea]
    [SerializeField] private string[] afterTutorialLines;
    [SerializeField] private Sprite afterTutorialPanelSprite; // 비우면 기본 대사창

    private bool waitingDialogueEnd = false;

    private void Start()
    {
        tutorialPanel.SetActive(true);
        tutorialPanelGroup.blocksRaycasts = true;
        playerMove.canMove = false;
    }

    private void Update()
    {
        if (tutorialPanel.activeSelf && Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            CloseTutorial();
        }

        // 대사가 끝나면 그때 플레이어를 움직이게 함
        if (waitingDialogueEnd && !DialogueManager.instance.IsDialogueActive)
        {
            waitingDialogueEnd = false;
            playerMove.canMove = true;
        }
    }

    private void CloseTutorial()
    {
        tutorialPanel.SetActive(false);
        tutorialPanelGroup.blocksRaycasts = false;

        if (afterTutorialLines != null && afterTutorialLines.Length > 0)
        {
            // canMove는 false인 채로 두고, 대사가 끝난 뒤 Update에서 true로 바꿈
            DialogueManager.instance.StartDialogue(afterTutorialLines, true, afterTutorialPanelSprite, null);
            waitingDialogueEnd = true;
        }
        else
        {
            playerMove.canMove = true;
        }
    }
}