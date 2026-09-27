using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private CanvasGroup tutorialPanelGroup; // ← 추가 1
    [SerializeField] private PlayerMove playerMove;

    private void Start()
    {
        tutorialPanel.SetActive(true);
        tutorialPanelGroup.blocksRaycasts = true; // ← 추가 2
        playerMove.canMove = false;
    }

    private void Update()
    {
        if (tutorialPanel.activeSelf && Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            CloseTutorial();
        }
    }

    private void CloseTutorial()
    {
        tutorialPanel.SetActive(false);
        tutorialPanelGroup.blocksRaycasts = false; // ← 추가 3 (선택)
        playerMove.canMove = true;
    }
}