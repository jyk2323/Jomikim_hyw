using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동에 필요한 코드

public class SceneChanger : MonoBehaviour
{
    // 버튼을 눌렀을 때 실행되는 함수
    public void GoToNextScene()
    {
        // 현재 씬 번호 + 1 번 씬으로 이동
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene + 1);
    }
}