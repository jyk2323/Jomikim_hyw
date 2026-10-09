using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
    public string nextSceneName = "Opening"; // 시작하기 누르면 갈 씬 (Inspector에서 변경 가능)

    public void StartGame()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}