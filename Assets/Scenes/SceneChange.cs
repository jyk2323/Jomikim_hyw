using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    public string nextSceneName;
    public Vector2 spawnPoint;

    private bool isChanging = false;

    // void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if (!collision.CompareTag("Player")) return;
    //     if (isChanging) return;

    //     isChanging = true;

    //     // FadeManager에만 맡기기!
    //     FadeManager.instance.LoadScene(nextSceneName, spawnPoint);
    // }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (isChanging) return;

        isChanging = true;

        Debug.Log("FadeManager: " + FadeManager.instance);
        Debug.Log("nextSceneName: " + nextSceneName);

        if (FadeManager.instance != null)
            FadeManager.instance.LoadScene(nextSceneName, spawnPoint);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }
    
}