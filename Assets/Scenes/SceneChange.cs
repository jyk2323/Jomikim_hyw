using UnityEngine;

public class SceneChange : MonoBehaviour
{
    public string nextSceneName;
    public Vector2 spawnPoint;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        // 이 두 줄을 지우고
        // PlayerHealth.instance.transform.position = spawnPoint;    
        // SceneManager.LoadScene(nextSceneName);

        // 이걸로 교체!
        FadeManager.instance.LoadScene(nextSceneName, spawnPoint);
    }
}