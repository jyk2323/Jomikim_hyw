using UnityEngine;
using UnityEngine.SceneManagement;

public class MonitorInteract : MonoBehaviour
{
    private bool playerInRange = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        UnityEngine.Debug.Log("트리거 진입: " + other.name);

        if (other.CompareTag("Player"))
        {
            UnityEngine.Debug.Log("Player 태그 확인됨");
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            UnityEngine.Debug.Log("트리거 이탈");   // ← 이 줄만 추가
            playerInRange = false;
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