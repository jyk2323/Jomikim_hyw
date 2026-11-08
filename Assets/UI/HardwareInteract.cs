using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HardwareInteract : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactHint;   // "E키로 해킹" 안내 UI (선택)

    [Header("진행도 창 (하드웨어 자식 World Space Canvas)")]
    [SerializeField] private GameObject progressPanel;  // ProgressCanvas
    [SerializeField] private Image progressImage;       // ProgressCanvas 아래 ProgressImage
    [SerializeField] private float hideDelay = 1f;      // 완료 후 창이 사라질 때까지 시간

    [Header("이벤트")]
    public UnityEvent onHackStart;
    public UnityEvent onHackComplete;

    private bool playerInRange;
    private bool hacking;
    private bool used;

    void Start()
    {
        if (interactHint != null) interactHint.SetActive(false);
        if (progressPanel != null) progressPanel.SetActive(false);

        if (HackingManager.Instance != null)
            HackingManager.Instance.onHackComplete.AddListener(OnComplete);
    }

    void OnDestroy()
    {
        if (HackingManager.Instance != null)
            HackingManager.Instance.onHackComplete.RemoveListener(OnComplete);
    }

    void Update()
    {
        if (!playerInRange || used) return;

        if (Input.GetKeyDown(interactKey))
        {
            used = true;
            hacking = true;
            if (interactHint != null) interactHint.SetActive(false);
            onHackStart?.Invoke();
            HackingManager.Instance?.StartHacking(progressImage);
        }
    }

    void OnComplete()
    {
        if (!hacking) return;   // 다른 하드웨어의 완료 이벤트는 무시
        hacking = false;
        onHackComplete?.Invoke();
        Invoke(nameof(HidePanel), hideDelay);
    }

    void HidePanel()
    {
        if (progressPanel != null) progressPanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        if (used) return;

        if (interactHint != null) interactHint.SetActive(true);
        if (progressPanel != null) progressPanel.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;

        if (interactHint != null) interactHint.SetActive(false);
        if (!hacking && progressPanel != null) progressPanel.SetActive(false);
    }
}