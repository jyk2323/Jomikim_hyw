using UnityEngine;
using UnityEngine.Events;

public class HardwareInteract : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactHint;   // "E키로 해킹" 안내 UI (선택)

    [Header("이벤트")]
    public UnityEvent onHackStart;      // 플레이어 조작 막기 등을 연결
    public UnityEvent onHackComplete;   // 문 열기, 조작 풀기 등을 연결

    private bool playerInRange;
    private bool used;

    void Start()
    {
        if (interactHint != null) interactHint.SetActive(false);

        // 해킹이 끝나면 이 하드웨어의 완료 이벤트도 실행
        HackingManager.Instance?.onHackComplete.AddListener(OnComplete);
    }

    void Update()
    {
        if (!playerInRange || used) return;

        if (Input.GetKeyDown(interactKey))
        {
            used = true;
            if (interactHint != null) interactHint.SetActive(false);
            onHackStart?.Invoke();
            HackingManager.Instance?.StartHacking();
        }
    }

    void OnComplete() => onHackComplete?.Invoke();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        if (interactHint != null && !used) interactHint.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (interactHint != null) interactHint.SetActive(false);
    }
}