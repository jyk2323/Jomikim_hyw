using UnityEngine;
using UnityEngine.SceneManagement;

public class MonitorInteract : MonoBehaviour
{
    private bool playerInRange = false;

    // ← 추가: 가까이 가면 보이는 "E키" 안내 (InteractCanvas 끌어다 넣기)
    public GameObject keyHint;

    void Start()
    {
        if (keyHint != null) keyHint.SetActive(false);   // ← 추가: 처음엔 숨김
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        UnityEngine.Debug.Log("트리거 진입: " + other.name);

        if (other.CompareTag("Player"))
        {
            UnityEngine.Debug.Log("Player 태그 확인됨");
            playerInRange = true;
            if (keyHint != null) keyHint.SetActive(true);    // ← 추가
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            UnityEngine.Debug.Log("트리거 이탈");   // ← 이 줄만 추가
            playerInRange = false;
            if (keyHint != null) keyHint.SetActive(false);   // ← 추가
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            UnityEngine.Debug.Log("E키 입력 감지, 씬 전환 시도");   // ← 여기
            SceneManager.LoadScene("ComputerUI");
        }
    }
}