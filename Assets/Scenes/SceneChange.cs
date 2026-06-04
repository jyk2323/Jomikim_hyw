using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public string nextSceneName; // 이동할 씬 이름
    public Vector2 spawnPoint; // 이동 후 캐릭터 위치

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        PlayerHealth.instance.transform.position = spawnPoint;    
        SceneManager.LoadScene(nextSceneName);
    }
}