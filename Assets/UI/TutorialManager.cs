using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug; // ← 추가

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private CanvasGroup tutorialPanelGroup;
    [SerializeField] private Button closeButton;
    [SerializeField] private PlayerMove playerMove;

    [Header("튜토리얼 직후 대사")]
    [TextArea]
    [SerializeField] private string[] afterTutorialLines;
    [SerializeField] private Sprite afterTutorialPanelSprite; // 비우면 기본 대사창

    private bool waitingDialogueEnd = false;

    // 튜토리얼은 게임당 한 번만 (1층에 다시 와도 안 뜸)
    private static bool alreadyShown = false;

    private void Start()
    {
        // 1층 씬에 놓인 카린은 두 번째부터 지워지므로, 실제로 따라다니는 카린을 사용
        if (PlayerHealth.instance != null)
            playerMove = PlayerHealth.instance.GetComponent<PlayerMove>();

        if (alreadyShown)
        {
            tutorialPanel.SetActive(false);
            tutorialPanelGroup.blocksRaycasts = false;
            enabled = false;
            return;
        }

        alreadyShown = true;
        tutorialPanel.SetActive(true);
        tutorialPanelGroup.blocksRaycasts = true;
        playerMove.canMove = false;

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseTutorial);
        else
            Debug.LogError("TutorialManager: Close Button이 Inspector에 연결되지 않았어요.");
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
        if (!tutorialPanel.activeSelf) return; // 중복 호출 방지 (맨 위에서 검사)

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